using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using Common;

namespace Filetypes {
//    public abstract class ModelCodec<M> : Codec<ModelFile<M>> {
//        
//        public ModelFile<M> Decode(Stream stream) {
//            ModelFile<M> result = null;
//            using (var reader = new BinaryReader(stream)) {
//                result = CreateFile();
//                try {
//                    result.Header = PackedFileDbCodec.readHeader(reader);
//                    // LastReadHeader = result.Header;
//#if DEBUG
//                // Console.WriteLine("Reading {0} models", result.Header.EntryCount);
//#endif
//                } catch (Exception ex) {
//                    throw new InvalidDataException(string.Format("Failed to read header from {0}", stream), ex);
//                }
//                for (int modelIndex = 0; modelIndex < result.Header.EntryCount; modelIndex++) {
//#if DEBUG
//                    // Console.WriteLine("Reading model {0}", modelIndex);
//#endif
//                    M model;
//                    try {
//                        model = ReadModel(reader);
//                    } catch (Exception ex) {
//                        throw new InvalidDataException(string.Format("Failed to read model data {0}", modelIndex), ex);
//                    }
//                    
//                    result.Models.Add(model);
//                }
//            }
//            return result;
//        }
//        public void Encode(Stream stream, ModelFile<M> file) {
//            file.Header.EntryCount = (uint) file.Models.Count;
//            using (var writer = new BinaryWriter(stream)) {
//                PackedFileDbCodec.WriteHeader(writer, file.Header);
//                file.Models.ForEach(toEncode => {
//                    WriteModel(writer, toEncode);
//                });
//            }
//        }
//        protected abstract ModelFile<M> CreateFile();
//        public abstract M ReadModel(BinaryReader reader);
//        
//        protected abstract void WriteModel(BinaryWriter writer, M model);
//        
//        #region Read Coordinates Utility Methods
//        protected static void WriteCoordinates(BinaryWriter writer, IEnumerable entry) {
//            foreach(Coordinates angle in entry) {
//                foreach(float f in angle) {
//                    writer.Write (f); 
//                }
//            }
//        }
//        protected static void ReadCoordinates(BinaryReader reader, ModelEntry entry) {
//            foreach (Coordinates angle in entry) {
//                ReadCoordinate(angle, reader);
//            }
//        }
//        protected static void ReadCoordinate(Coordinates coordinates, BinaryReader reader) {
//            coordinates.XCoordinate = reader.ReadSingle();
//            coordinates.YCoordinate = reader.ReadSingle();
//            coordinates.ZCoordinate = reader.ReadSingle();
//        }
//        #endregion
//
//        #region List Reader Utility Functions
////        public static List<T> ReadList<T>(BinaryReader reader, ItemReader<T> readItem, bool skipIndex = false, int itemCount = -1) {
////        }
//        #endregion
//    }
}

