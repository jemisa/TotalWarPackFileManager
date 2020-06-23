using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using Common;
using Filetypes;


namespace DbEntryCorrection {
    class MainClass {
        public static void Main(string[] args) {
            if (args.Length == 0) {
                Console.Out.WriteLine("usage: dbcorrect [-cleanup] <packfile>");
                return;
            }
            bool cleanup = false;
            String inPackFileName = args[0];
            if (args.Length == 2) {
                cleanup = "-cleanup".Equals(args[0]);
                Console.WriteLine("Cleanup enabled (will not add empty db files)");
                inPackFileName = args[1];
            }
            Console.Out.WriteLine("opening {0}", inPackFileName);
            PackFile packFile = new PackFileCodec().Open(inPackFileName);

            String correctedFileName = inPackFileName.Replace(".pack", "_corrected.pack");
            String emptyFileName = inPackFileName.Replace(".pack", "_empty.pack");
            String missingFileName = inPackFileName.Replace(".pack", "_unknown.pack");
            PackFile correctedPack = new PackFile(correctedFileName, packFile.Header);
            PackFile emptyPack = new PackFile(emptyFileName, packFile.Header);
            PackFile missingPack = new PackFile(missingFileName, packFile.Header);

            DBTypeMap.Instance.InitializeTypeMap(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location));
            VirtualDirectory dbDir = packFile.Root.GetSubdirectory("db");
            foreach(PackedFile packedFile in dbDir.AllFiles) {
                PackFile targetPack = correctedPack;
                Console.Out.WriteLine(packedFile.FullPath);
                DBFileHeader header = PackedFileDbCodec.readHeader(packedFile);
                DBFileHeader newHeader = new DBFileHeader(header);
                if (header.EntryCount == 0)
                {
                    emptyPack.Add(packedFile);
                    continue;
                }
                String typeName = DBFile.Typename(packedFile.FullPath);
                byte[] fileData = packedFile.Data;
                // we only accept the exact type/version combination here
                // and only if we don't have to go around trying which one it is (yet)
                var infos = DBTypeMap.Instance.GetVersionedInfos(typeName, header.Version);
                bool added = false;
                foreach(Filetypes.TypeInfo typeInfo in infos) {
                    Console.Out.WriteLine("trying {0}", typeInfo);
                    DBFile newDbFile = new DBFile(newHeader, typeInfo);
                    try {
                        using (BinaryReader reader = new BinaryReader(new MemoryStream(fileData, 0, fileData.Length))) {
                            reader.BaseStream.Position = header.Length;
                            while (reader.BaseStream.Position != fileData.Length) {
                                // try decoding a full row of fields and add it to the new file
                                DBRow newRow = new DBRow(typeInfo);
                                foreach(Filetypes.FieldInfo info in typeInfo.Fields) {
                                    newRow[info.Name].Decode(reader);
                                    //FieldInstance instance = info.CreateInstance();
                                    //instance.Decode(reader);
                                    //newRow.Add(instance);
                                }
                                newDbFile.Entries.Add(newRow);
                            }
                            // all data read successfully! 
                            if (newDbFile.Entries.Count == header.EntryCount) {
                                Console.Out.WriteLine("{0}: entry count {1} is correct", packedFile.FullPath, newDbFile.Entries.Count);
#if DEBUG
//                                foreach(DBRow row in newDbFile.Entries) {
//                                    String line = "";
//                                    foreach(FieldInstance instance in row) {
//                                        line += String.Format("{0} - ", line);
//                                    }
//                                    Console.WriteLine(line);
//                                }
#endif
                            } else {
                                Console.Out.WriteLine("{0}: entry count {1} will be corrected to {2}", 
                                                      packedFile.FullPath, header.EntryCount, newDbFile.Entries.Count);
                            }
                            if (newDbFile.Entries.Count == 0) {
                                targetPack = emptyPack;
                            }
                            PackedFile newPackedFile = new PackedFile(packedFile.FullPath, false);
                            PackedFileDbCodec codec = PackedFileDbCodec.FromFilename(packedFile.FullPath);
                            newPackedFile.Data = codec.Encode(newDbFile);
                            targetPack.Add(newPackedFile);
                            added = true;
                            Console.Out.WriteLine("stored file with {0} entries", newDbFile.Entries.Count);
                            break;
                        }
                    } catch (Exception e) {
                        Console.Error.WriteLine("Will not add {0}: a problem occurred when reading it: {1} at entry {2}",
                                                packedFile.FullPath, e, newDbFile.Entries.Count);
                    }
                }
                if (!added)
                {
                    missingPack.Add(packedFile);
                }
            }
            Console.Out.WriteLine("saving {0}", correctedPack.Filepath);
            PackFileCodec packCodec = new PackFileCodec();
            packCodec.Save(correctedPack);
            packCodec.Save(emptyPack);
            packCodec.Save(missingPack);
        }
    }
}
