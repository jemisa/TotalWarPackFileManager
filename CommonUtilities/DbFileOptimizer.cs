using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Common;
using Filetypes;
using Filetypes.Codecs;

namespace CommonUtilities {
    /*
     * Analyses the db files of a pack whether they contain changes
     * to the original game files. Creates a new pack with only changed entries.
     */
    public class DbFileOptimizer {
        public DbFileOptimizer(Game game) {
            // game packs in correct load order
            packPaths = new PackLoadSequence() {
                IgnorePack = PackLoadSequence.IsDbCaPack
            }.GetPacksLoadedFrom(game.GameDirectory);
#if DEBUG
            Console.WriteLine("packs: {0}", string.Join(",", packPaths));
#endif
        }

        /*
         * Iterator through packed loaded from game.
         */
        public IEnumerable<PackedFile> PackedInGame {
            get {
                return new MultiPackEnumerable(packPaths);
            }
        }
        List<string> packPaths;
        
        /**
         * <summary>Removes all elements identical between the selected game's <see cref="PackFile">PackFiles</see> and passed PackFile.</summary>
         * <remarks>
         * Does not alter non-DB files in the passed <see cref="PackFile"/>.
         * DB files in the passed PackFile have all identical rows removed from the DB file.
         * For rows to be identical the schemas must be the same.
         * <see cref="PackedFile">PackedFiles</see> that have no unique rows are removed from the final <see cref="PackFile"/>.
         * </remarks>
         * 
         * <param name="toOptimize">The <see cref="PackFile"/> to be optimized</param>
         * <returns>A new <see cref="PackFile"/> that contains the optimized data of <paramref name="toOptimize"/>.</returns>
         * 
         * XXX This could be optimized better by multi-threading the foreach loop if adding <see cref="PackedFile">PackedFiles</see> to a <see cref="PackFile"/> was thread-safe.
         */
        public PackFile CreateOptimizedFile(PackFile toOptimize) {
            PFHeader header = new PFHeader(toOptimize.Header);
            string newPackName = Path.Combine(Path.GetDirectoryName(toOptimize.Filepath),
                                              string.Format("optimized_{0}", Path.GetFileName(toOptimize.Filepath)));
            PackFile result = new PackFile(newPackName, header);

            ConcurrentDictionary<string, List<DBFile>> gameDBFiles = FindGameDBFiles();
            PackedFile optimized;
            List<DBFile> referenceFiles;
            foreach(PackedFile file in toOptimize)
            {
                if(file.FullPath.StartsWith("db" + Path.DirectorySeparatorChar))
                {
                    if(gameDBFiles.TryGetValue(DBFile.Typename(file.FullPath), out referenceFiles))
                        optimized = OptimizePackedDBFile(file, referenceFiles);
                    else
                    {
                        result.Add(file);
                        continue;
                    }
                    if(optimized != null)
                        result.Add(optimized);
                }
                else
                    result.Add(file);
            }
            return result;
        }

        /**
         * <summary>Removes all entries identical between the <paramref name="unoptimizedFile"/> and the <paramref name="referenceFiles"/> from the <paramref name="unoptimizedFile"/>.</summary>
         * <remarks>This function was intended to be passed a <see cref="PackedFile"/> that contains DB tables.  If it is passed a PackedFile without DB tables it will not work properly.</remarks>
         * 
         * <param name="unoptimizedFile">The <see cref="PackedFile"/> to be optimized.  It must contain a DB table for the method to work.</param>
         * <param name="referenceFiles">A <see cref="List{DBFile}"/> of <see cref="DBFile">DBFiles</see> that should be checked for identical table rows in the <paramref name="unoptimizedFile"/>.</param>
         * 
         * <returns>A new <see cref="PackedFile"/> that contains the optimized data from the <paramref name="unoptimizedFile"/> or null if the resulting <see cref="PackedFile"/> would be empty.</returns>
         */
        public PackedFile OptimizePackedDBFile(PackedFile unoptimizedFile, List<DBFile> referenceFiles)
        {
            PackedFile result = unoptimizedFile;
            DBFile modDBFile = FromPacked(unoptimizedFile);

            if(modDBFile != null)
            {
                foreach(DBFile file in referenceFiles)
                    if(TypesCompatible(modDBFile, file))
                        modDBFile.Entries.RemoveAll(file.ContainsRow);
                if(modDBFile.Entries.Count != 0)
                    result.Data = PackedFileDbCodec.GetCodec(unoptimizedFile).Encode(modDBFile);
                else
                    result = null;
            }

            return result;
        }
        
        #region Find Corresponding File in Game
        /*
         * Retrieve the db file corresponding to the given packed file from game.
         */
        List<DBFile> FindInGamePacks(PackedFile file) {
            List<DBFile> result = new List<DBFile>();
            string typeName = DBFile.Typename(file.FullPath);
            
            foreach (PackedFile gamePacked in PackedInGame)
                if (DBFile.Typename(gamePacked.FullPath).Equals(typeName))
                    result.Add(FromPacked(gamePacked));

            return result;
        }

        /**
         * <summary>Finds all DB Files from a selected <see cref="Game">Game's</see> <see cref="PackFile">PackFiles</see>.</summary>
         * 
         * <returns>A <see cref="ConcurrentDictionary{string, List{DBFile}}"/> that contains all the <see cref="DBFile">DBFiles</see> from the selected <see cref="Game"/> (not including mods) sorted into <see cref="List{DBFile}">Lists</see> identified by the folder they belong to.</returns>
         */
        public ConcurrentDictionary<string, List<DBFile>> FindGameDBFiles()
        {
            ConcurrentDictionary<string, List<DBFile>> result = new ConcurrentDictionary<string, List<DBFile>>();

            Parallel.ForEach(PackedInGame, (packedFile) =>
            {
                if(packedFile.FullPath.StartsWith("db" + Path.DirectorySeparatorChar))
                    result.GetOrAdd(DBFile.Typename(packedFile.FullPath), new List<DBFile>()).Add(FromPacked(packedFile));
            });

            return result;
        }

        /**
         * <summary>Finds the <see cref="PackedFile"/> with the given <see cref="PackedFile.FullPath">Path</see> in the given <see cref="PackFile"/>.</summary>
         * 
         * <param name="pack">The <see cref="PackFile"/> to be searched.</param>
         * <param name="name">The <see cref="PackedFile.FullPath"/> of the <see cref="PackedFile"/> to search for.</param>
         * <returns>The <see cref="PackedFile"/> that was being searched for or null if it was not found.</returns>
         */
        PackedFile FindInPack(PackFile pack, string name)
        {
            foreach(PackedFile file in pack)
                if(file.FullPath.Equals(name))
                    return file;
            return null;
        }

        /*
         * Create db file from the given packed file.
         * Will not throw an exception on error, but return null.
         */
        DBFile FromPacked(PackedFile packed) {
            DBFile result = null;
            try {
                PackedFileDbCodec codec = PackedFileDbCodec.GetCodec(packed);
                if (codec != null) {
                    result = codec.Decode(packed.Data);
                }
            } catch {}
            return result;
        }

        /**
         * <summary>Checks if the passed <see cref="DBFile">DBFiles</see> have the equivalent schema.</summary>
         * 
         * <param name="file1">One of the <see cref="DBFile">DBFiles</see> to be compared.</param>
         * <param name="file2">The other <see cref="DBFile"/> to be compared.</param>
         * <returns>Whether the passed <see cref="DBFile">DBFiles</see> have equivalent shchema.</returns>
         */
        bool TypesCompatible(DBFile file1, DBFile file2)
        {
            if(file1 == null || file2 == null)
                return false;

            List<FieldInfo> infos1 = file1.CurrentType.Fields;
            List<FieldInfo> infos2 = file2.CurrentType.Fields;

            if(infos1.Count != infos2.Count)
                return false;
            for(int i = 0; i < infos1.Count; ++i)
                if(infos1[i] != infos2[i])
                    return false;
            return true;
        }
        #endregion
    }
}

