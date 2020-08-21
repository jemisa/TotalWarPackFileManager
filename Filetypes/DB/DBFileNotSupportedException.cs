using System;

namespace Filetypes {
    /*
     * Exception having to do with the pack file database.
     */
    public class DbException : Exception {
        public DBFile DbFile { get; set; }

        public DbException(string message) : base(message) {
        }

        public DbException (string message, Exception x) : base(message, x) {
        }
    }
    
    /*
     * An exception being thrown when trying to read a db file that fails to be decoded.
     */
    public class DBFileNotSupportedException : DbException {
        public DBFileNotSupportedException(string message) : base(message) {
        }


		public DBFileNotSupportedException (string message, Exception x) : base(message, x) {
		}

        public DBFileNotSupportedException(string message, DBFile file) : base(message) {
            DbFile = file;
        }
    }
}

