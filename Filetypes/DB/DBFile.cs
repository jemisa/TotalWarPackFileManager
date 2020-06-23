using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace Filetypes {
	/*
	 * Class representing the first bytes in a db file;
	 * these contain information about db version, maybe an id,
	 * and a count of contained entries.
	 */
    public class DBFileHeader {
        public string GUID { get; set; }
        public bool HasVersionMarker { get; set; }
        public int Version { get; set; }
        public uint EntryCount { get; set; }
        /*
         * The length of the encoded header.
         */
		public int Length {
			get {
				int result = 5;
				result += (GUID.Length != 0) ? 78 : 0;
				result += HasVersionMarker ? 8 : 0;
				return result;
			}
		}
        /*
         * Create header with the given GUID, version and entry count.
         */
        public DBFileHeader(string guid, int version, uint entryCount, bool marker) {
            GUID = guid;
            Version = version;
            EntryCount = entryCount;
            HasVersionMarker = marker;
        }
        
        /*
         * Create copy of given header.
         */
        public DBFileHeader(DBFileHeader toCopy) : this(toCopy.GUID, toCopy.Version, 0, toCopy.HasVersionMarker) {}

		#region Framework Overrides
        public override bool Equals(object other) {
            bool result = false;
            if (other is DBFileHeader) {
                DBFileHeader header2 = (DBFileHeader)other;
                result = GUID.Equals(header2.GUID);
                result &= Version.Equals(header2.Version);
                result &= EntryCount.Equals(header2.EntryCount);
            }
            return result;
        }
        public override int GetHashCode() {
            return GUID.GetHashCode();
        }
		#endregion
            }

	/*
	 * Class representing a database file.
	 */
    public class DBFile {
        private List<DBRow> entries = new List<DBRow>();
        public DBFileHeader Header;
        public TypeInfo CurrentType {
            get;
            set;
        }

		#region Attributes
		// the entries of this file
        public List<DBRow> Entries {
            get {
                return this.entries;
            }
        }

        // access by row/column
		public FieldInstance this [int row, int column] {
			get {
				return entries [row][column];
			}
		}
		#endregion

        #region Constructors
        /*
         * Create db file with the given header and the given type.
         */
        public DBFile (DBFileHeader h, TypeInfo info) {
			Header = h;
			CurrentType = info;
		}
        /*
         * Create copy of the given db file.
         */
        public DBFile (DBFile toCopy) : this(toCopy.Header, toCopy.CurrentType) {
			Header = new DBFileHeader (toCopy.Header.GUID, toCopy.Header.Version, toCopy.Header.EntryCount, toCopy.Header.HasVersionMarker);
            // we need to create a new instance for every field so we don't write through to the old data
			toCopy.entries.ForEach (entry => entries.Add (new DBRow (toCopy.CurrentType, entry)));
		}
        #endregion

        /**
         * <summary>Checks whether this DBFile contains a <see cref="DBRow"/> with equivalent data to a given DBRow.</summary>
         * 
         * <param name="checkRow">The <see cref="DBRow"/> to be compared to this DBFile's rows.</param>
         * <returns>Whether the DBFile contains a DBRow with equivalent data to <paramref name="checkRow"/>.</returns>
         */
        public bool ContainsRow(DBRow checkRow)
        {
            foreach(DBRow row in Entries)
                if(row.SameData(checkRow))
                    return true;
            return false;
        }

        /*
         * Create new entry for the data base.
         * Note that the entry will not be added to the entries by this.
         */
        public DBRow GetNewEntry() {
			return new DBRow(CurrentType);
		}
  
        /*
         * Add data contained in the given db file to this one.
         */
        public void Import(DBFile file) {
			if (CurrentType.Name != file.CurrentType.Name) {
				throw new DBFileNotSupportedException 
					("File type of imported DB doesn't match that of the currently opened one", this);
			}
			// check field type compatibility
			for (int i = 0; i < file.CurrentType.Fields.Count; i++) {
				if (file.CurrentType.Fields [i].TypeCode != CurrentType.Fields [i].TypeCode) {
					throw new DBFileNotSupportedException 
						("Data structure of imported DB doesn't match that of currently opened one at field " + i, this);
				}
			}
			DBFileHeader h = file.Header;
			Header = new DBFileHeader (h.GUID, h.Version, h.EntryCount, h.HasVersionMarker);
			CurrentType = file.CurrentType;
			// this.entries = new List<List<FieldInstance>> ();
			entries.AddRange (file.entries);
            Header.EntryCount = (uint) entries.Count;
		}
        /*
         * Helper to retrieve type name from a file path.
         */
		public static string Typename(string fullPath) {
			return Path.GetFileName(Path.GetDirectoryName (fullPath));
		}
	}
}
