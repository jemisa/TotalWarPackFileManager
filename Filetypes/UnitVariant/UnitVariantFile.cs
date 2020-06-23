namespace Filetypes
{
    using System;
    using System.Collections.Generic;
    using System.IO;
	
    public class UnitVariantFile
    {
		// "header" information
        public uint Version { get; set; }
        public byte B1 { get; set; }
        public byte B2 { get; set; }
        public byte B3 { get; set; }
        public byte B4 { get; set; }
		public uint Unknown1 { get; set; }
		public uint Unknown2 { get; set; }
		public int Unknown3 { get; set; }

		private List<UnitVariantObject> unitVariantObjects = new List<UnitVariantObject> ();
		public List<UnitVariantObject> UnitVariantObjects {
			get {
				return this.unitVariantObjects;
			}
			set {
				this.unitVariantObjects = value;
			}
		}

        public uint NumEntries { 
			get {
				return (uint) unitVariantObjects.Count;
			}
		}

        public void Add(UnitVariantObject newEntry) {
            this.unitVariantObjects.Add(newEntry);
        }

        public void InsertUVO(UnitVariantObject entry, int index) {
            this.unitVariantObjects.Insert(index, entry);
        }

        public void RemoveAt(int index) {
            this.unitVariantObjects.RemoveAt(index);
        }

        public void RemoveUVO(UnitVariantObject uvo) {
			this.unitVariantObjects.Remove (uvo);
		}
    }
}

