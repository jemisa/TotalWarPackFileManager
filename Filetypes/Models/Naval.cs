using System;
using System.Collections.Generic;
using System.IO;
using Common;

namespace Filetypes {
//    #region Naval Model
//    public class ShipModelFile : ModelFile<ShipModel> { }
//
//    public abstract class NavalModel<T> : EntryContainer<T> {
//        public string ModelId { get; set; }
//        public string RiggingLogicPath { get; set; }
//        public string RigidModelPath { get; set; }
//
//        private List<NavalCam> cams = new List<NavalCam>();
//        public List<NavalCam> NavalCams {
//            get {
//                return cams;
//            }
//        }
//        
//        private List<PartPositionInfo> partPositions = new List<PartPositionInfo>();
//        public List<PartPositionInfo> PositionInfos {
//            get {
//                return partPositions;
//            }
//        }
//        
//        private List<T> partEntries = new List<T>();
//        public List<T> Parts {
//            get {
//                return partEntries;
//            }
//        }
//
//        public bool Unknown { get; set; }
//
//        /* Four trailing ints.. probably references */
//        public uint Side1 {
//            get; set;
//        }
//        public uint Side2 {
//            get; set;
//        }
//        public uint Side3 {
//            get; set;
//        }
//        public uint Side4 {
//            get; set;
//        }
//    }
//    
//    public class NavalCam : EntryContainer<uint> {
//        public string Name { get; set; }
//    }
//    
//    public class PartPositionInfo : ModelEntry {
//        List<uint> unknown = new List<uint>();
//        public List<uint> Unknown {
//            get {
//                return unknown;
//            }
//        }
//    }
//    #endregion
//
//    #region Ship Model
//    public class ShipModel : NavalModel<ShipElement> {
//        public uint UnknownUint {
//            get; set;
//        }
//    }
//    
//    public class ShipElement : EntryContainer<PartEntry> {
//        public string PartName { get; set; }
//        public uint Unknown {
//            get; set;
//        }
//    }
//    
//    public class PartEntry : ModelEntry {
//        public uint Unknown {
//            get; set;
//        }
//
//        /* Four trailing ints.. probably references */
//        public uint Side1 {
//            get; set;
//        }
//        public uint Side2 {
//            get; set;
//        }
//        public uint Side3 {
//            get; set;
//        }
//        public uint Side4 {
//            get; set;
//        }
//        
//        /* The bools seem to be associated to the coordinate blocks */
//        public bool Coord1Tag {
//            get; set;
//        }
//        public bool Coord2Tag {
//            get; set;
//        }
//        public bool Coord3Tag {
//            get; set;
//        }
//        public void FlagCoordinate(Coordinates toFlag, bool flagAs) {
//            if (toFlag == Coordinates1) {
//                Coord1Tag = flagAs;
//            } else if (toFlag == Coordinates2) {
//                Coord2Tag = flagAs;
//            } else if (toFlag == Coordinates3) {
//                Coord3Tag = flagAs;
//            }
//        }
//        public bool GetCoordinateFlag(Coordinates getFor) {
//            if (getFor == Coordinates1) {
//                return Coord1Tag;
//            } else if (getFor == Coordinates2) {
//                return Coord2Tag;
//            } else if (getFor == Coordinates3) {
//                return Coord3Tag;
//            }
//            throw new InvalidOperationException();
//        }
//    }
//
//    public class SomeList : EntryContainer<SomeEntry> {
//        public uint Unknown1 { get; set; }
//        public uint Unknown2 { get; set; }
//    }
//    
//    public class SomeEntry : ModelEntry {
//        public uint Unknown1 { get; set; }
//        public uint Coord1Tag {
//            get; set;
//        }
//        public uint Coord2Tag {
//            get; set;
//        }
//        public uint Coord3Tag {
//            get; set;
//        }
//        public void TagCoordinate(Coordinates toFlag, uint flagAs) {
//            if (toFlag == Coordinates1) {
//                Coord1Tag = flagAs;
//            } else if (toFlag == Coordinates2) {
//                Coord2Tag = flagAs;
//            } else if (toFlag == Coordinates3) {
//                Coord3Tag = flagAs;
//            }
//        }
//        public uint GetCoordinateTag(Coordinates getFor) {
//            if (getFor == Coordinates1) {
//                return Coord1Tag;
//            } else if (getFor == Coordinates2) {
//                return Coord2Tag;
//            } else if (getFor == Coordinates3) {
//                return Coord3Tag;
//            }
//            throw new InvalidOperationException();
//        }
//    }
//
//#endregion
//    
//    #region Ship Codec
//    public abstract class NavalCodec<T> : ModelCodec<T> {
//        protected NavalCam ReadNavalCam(BinaryReader reader) {
//#if DEBUG
//            //Console.WriteLine("Reading cams starting at {0:x}", reader.BaseStream.Position);
//#endif
//            NavalCam cam = new NavalCam {
//                Name = IOFunctions.readCAString(reader)
//            };
//            for (int dataIndex = 0; dataIndex < 16; dataIndex++) {
//                cam.Entries.Add(reader.ReadUInt32());
//            }
//            return cam;
//        }
//
//        protected PartPositionInfo ReadPositionEntry(BinaryReader reader) {
//#if DEBUG
//            //Console.WriteLine("Reading position entry at {0:x}", reader.BaseStream.Position);
//#endif
//            PartPositionInfo entry = new PartPositionInfo();
//            ReadCoordinates(reader, entry);
//            
//            int moreEntries = reader.ReadInt32();
//            for (int i = 0; i < moreEntries; i++) {
//                entry.Unknown.Add(reader.ReadUInt32());
//            }
//            Console.Out.Flush();
//            return entry;
//        }
//        
//    }
//    
//    /*
//     * The models codec for earlier games; only contains ship parts.
//     */
//    public class ShipModelCodec : NavalCodec<ShipModel> {
//        private static ShipModelCodec instance = new ShipModelCodec();
//        public static ShipModelCodec Instance {
//            get {
//                return instance;
//            }
//        }
//        
//        protected override ModelFile<ShipModel> CreateFile() {
//            return new ShipModelFile();
//        }
//        
//        public override ShipModel ReadModel(BinaryReader reader) {
//#if DEBUG
//            Console.WriteLine("Reading Ship Model at {0:x}", reader.BaseStream.Position);
//#endif
//            string name = IOFunctions.readCAString(reader);
//            ShipModel ship = new ShipModel {
//                ModelId =  name,
//                RigidModelPath = IOFunctions.readCAString(reader),
//                RiggingLogicPath = IOFunctions.readCAString(reader)
//            };
//            // post-stw:
////               ShipModel result = new ShipModel {
////                ModelId = IOFunctions.readCAString(reader),
////                RiggingLogicPath = IOFunctions.readCAString(reader),
////                Unknown = reader.ReadBoolean(),
////                RigidModelPath = IOFunctions.readCAString(reader)
////            };
//
//            IOFunctions.FillList(ship.NavalCams, ReadNavalCam, reader, false);
//            IOFunctions.FillList(ship.PositionInfos, ReadPositionEntry, reader, true);
//            // ship.UnknownUint = reader.ReadUInt32();
//            IOFunctions.FillList(ship.Parts, ReadShipElement, reader, false);
//            
//            List<object> test = new List<object>();
//            IOFunctions.FillList(test, ReadTest, reader, false);
//            
//            return ship;
//        }
//  
//        object ReadTest(BinaryReader reader) {
//            SomeList list = new SomeList {
//                Unknown1 = reader.ReadUInt32(),
//                Unknown2 = reader.ReadUInt32()
//            };
//            IOFunctions.FillList(list.Entries, ReadSomeEntry, reader);
//            
//            return list;
//        }
//        
//        public SomeEntry ReadSomeEntry(BinaryReader reader) {
//            SomeEntry entry = new SomeEntry();
//            foreach(Coordinates coord in entry) {
//                ReadCoordinate(coord, reader);
//                entry.TagCoordinate(coord, reader.ReadUInt32());
//            }
//            return entry;
//        }
//        
//        public ShipElement ReadShipElement(BinaryReader reader) {
//#if DEBUG
//            // Console.WriteLine("Reading Ship Element at {0:x}", reader.BaseStream.Position);
//#endif
//            //PartEntry part = new PartEntry();
//            ShipElement element = new ShipElement {
//                PartName = IOFunctions.readCAString(reader),
//                Unknown = reader.ReadUInt32()
//            };
//            IOFunctions.FillList(element.Entries, ReadPartEntry, reader, true);
//            return element;
//        }
//        
//        protected PartEntry ReadPartEntry(BinaryReader reader) {
//#if DEBUG
//            // Console.WriteLine("Reading Part entry at {0:x}", reader.BaseStream.Position);
//#endif
//            PartEntry part = new PartEntry {
//                Unknown = reader.ReadUInt32()
//            };
//            Coordinates[] coords = { part.Coordinates1, part.Coordinates2, part.Coordinates3 };
//            foreach(Coordinates coord in coords) {
//                ReadCoordinate(coord, reader);
//                part.FlagCoordinate(coord, reader.ReadBoolean());
//            }
//            part.Side1 = reader.ReadUInt32();
//            part.Side2 = reader.ReadUInt32();
//            part.Side3 = reader.ReadUInt32();
//            part.Side4 = reader.ReadUInt32();
//            return part;
//        }
//        
//        protected override void WriteModel(BinaryWriter writer, ShipModel model) {
//            throw new NotImplementedException ();
//        }
//    }
//    #endregion
//    
}

