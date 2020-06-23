using System;
using System.Collections.Generic;
using Common;

namespace Filetypes {
    /*
     * Class updating a db file to the highest version available for the file's type.
     * This is one of the reasons why the schema files have to be separated and released
     * per game, because each game has a different range of versions it can load at all
     * and we can't let a user update a table to a version the game doesn't even support.
     */
    public class DBFileUpdate {
        public delegate string GuidDeterminator(List<string> options);
        
        public GuidDeterminator DetermineGuid { get; set; }
        public string GetGuid(List<string> options) {
            if (DetermineGuid != null) {
                return DetermineGuid(options);
            }
            throw null;
        }
        
        // this could do with an update; since the transition to schema.xml,
        // we also know obsolete fields and can remove them,
        // and we can add fields in the middle instead of assuming they got appended.
        public void UpdatePackedFile(PackedFile packedFile) {
            string key = DBFile.Typename(packedFile.FullPath);
            if (DBTypeMap.Instance.IsSupported(key)) {
                PackedFileDbCodec codec = PackedFileDbCodec.FromFilename(packedFile.FullPath);
                int maxVersion = DBTypeMap.Instance.MaxVersion(key);
                DBFileHeader header = PackedFileDbCodec.readHeader(packedFile);
                if (header.Version < maxVersion) {
                    // found a more recent db definition; read data from db file
                    DBFile updatedFile = PackedFileDbCodec.Decode(packedFile);

                    TypeInfo dbFileInfo = updatedFile.CurrentType;
                    TypeInfo targetInfo = GetTargetTypeInfo (key, maxVersion);
                    if (targetInfo == null) {
                        throw new Exception(string.Format("Can't decide new structure for {0} version {1}.", key, maxVersion));
                    }
     
                    // identify FieldInstances missing in db file
                    for (int i = 0; i < targetInfo.Fields.Count; i++) {
                        FieldInfo oldField = dbFileInfo[targetInfo.Fields[i].Name];
                        if (oldField == null) {
                            foreach(List<FieldInstance> entry in updatedFile.Entries) {
                                entry.Insert(i, targetInfo.Fields[i].CreateInstance());
                            }
                        }
                    }
                    //updatedFile.Header.GUID = guid;
                    updatedFile.Header.Version = maxVersion;
                    packedFile.Data = codec.Encode(updatedFile);
                }
            }
        }
        /*
         * Find the type info for the given type and version to update to.
         */
        TypeInfo GetTargetTypeInfo(string key, int maxVersion) {
            TypeInfo targetInfo = DBTypeMap.Instance.GetVersionedInfos(key, maxVersion)[0];
            return targetInfo;
        }
    }
}

