using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Text.RegularExpressions;
using Common;
using Filetypes;

namespace MMS {
    public class PackImporter {
        public PackImporter () {
        }

        static readonly Regex PACK_FILE_RE = new Regex(".pack");
        public void ImportExistingPack(string packFileName) {
            string modName = PACK_FILE_RE.Replace(Path.GetFileName(packFileName), "");

            // trigger creation of backup folder
            new Mod(modName);
            MultiMods.Instance.AddMod(modName);

            PackFile pack = new PackFileCodec().Open(packFileName);
            foreach (PackedFile packed in pack) {
                // extract to working_data as binary
                List<string> extractPaths = new List<string>();
                byte[] data = null;
                if (DBTypeMap.Instance.IsSupported(DBFile.typename(packed.FullPath))) {
                    PackedFileDbCodec codec = PackedFileDbCodec.GetCodec(packed);
                    Codec<DBFile> writerCodec = new ModToolDBCodec(FieldMappingManager.Instance);
                    DBFile dbFile = codec.Decode(packed.Data);
                    using (var stream = new MemoryStream()) {
                        writerCodec.Encode(stream, dbFile);
                        data = stream.ToArray();
                    }
                    string extractFilename = string.Format("{0}.xml", packed.Name);
                    extractPaths.Add(Path.Combine(ModTools.Instance.InstallDirectory, "raw_data", "db", extractFilename));
                    extractPaths.Add(Path.Combine(ModTools.Instance.RawDataPath, "EmpireDesignData", "db", extractFilename));
                } else {
                    extractPaths.Add(Path.Combine(ModTools.Instance.WorkingDataPath, packed.FullPath));
                    data = packed.Data;
                }
                foreach (string path in extractPaths) {
                    string extractDir = Path.GetDirectoryName(path);
                    if (!Directory.Exists(extractDir)) {
                        Directory.CreateDirectory(extractDir);
                    }
                    File.WriteAllBytes(path, data);
                }
            }
        }
    }
}

