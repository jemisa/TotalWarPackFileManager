using Filetypes.ByteParsing;
using Filetypes.DB;
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
        private List<DBRow> _entries = new List<DBRow>();
        public DBFileHeader Header { get; private set; }
        public DbTableDefinition CurrentType {
            get;
            set;
        }

		#region Attributes
		// the entries of this file
        public List<DBRow> Entries {
            get {
                return this._entries;
            }
        }

        // access by row/column
		public DbField this [int row, int column] {
			get {
				return _entries [row][column];
			}
		}
		#endregion

        #region Constructors
        /*
         * Create db file with the given header and the given type.
         */
        public DBFile (DBFileHeader h, DbTableDefinition info) {
			Header = h;
			CurrentType = info;
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
         * Helper to retrieve type name from a file path.
         */
		public static string Typename(string fullPath) {
			return Path.GetFileName(Path.GetDirectoryName (fullPath));
		}
	}
}
