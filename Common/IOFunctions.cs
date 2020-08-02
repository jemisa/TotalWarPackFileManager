using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace Common {

    /*
     * Utility methods to read common data from streams.
     */
    public class IOFunctions {
        // filter string for tsv files
        public static string TSV_FILTER = "TSV Files (*.csv,*.tsv)|*.csv;*.tsv|Text Files (*.txt)|*.txt|All Files|*.*";
        // filter string for pack files
        public static string PACKAGE_FILTER = "Package File (*.pack)|*.pack|Any File|*.*";

        /*
         * Read a unicode string from the given reader.
         */
        public static string ReadCAString(BinaryReader reader) {
            return ReadCAString(reader, Encoding.Unicode);
        }

        /*
         * Read a string from the given reader, using the given encoding.
         * First 2 bytes contain the string length, string is not zero-terminated.
         */
        public static string ReadCAString(BinaryReader reader, Encoding encoding) 
        {
            int num = reader.ReadInt16();
            // Unicode is 2 bytes per character; UTF8 is variable, but the number stored is the number of bytes, so use that
            int bytes = (encoding == Encoding.Unicode ? 2 : 1) * num;
            // enough data left?
            if (reader.BaseStream.Length - reader.BaseStream.Position < bytes)
            {
                throw new InvalidDataException(string.Format("Cannot read string of length {0}: only {1} bytes left",
                    bytes, reader.BaseStream.Length - reader.BaseStream.Position));
            }

            var strByteData = reader.ReadBytes(bytes);
            return encoding.GetString(strByteData);
        }

        public static bool TryReadReadCAString(BinaryReader reader, Encoding encoding, out string stringResult)
        {
            if (reader.BaseStream.Length - reader.BaseStream.Position < 2)
            {
                stringResult = $"Cannot read length of string {reader.BaseStream.Length - reader.BaseStream.Position} bytes left";
                return false;
            }

            int num = reader.ReadInt16();
            if (0 > num)
            {
                stringResult = "Negative file length";
                return false;
            }

            // Unicode is 2 bytes per character; UTF8 is variable, but the number stored is the number of bytes, so use that
            int bytes = (encoding == Encoding.Unicode ? 2 : 1) * num;
            // enough data left?
            if (reader.BaseStream.Length - reader.BaseStream.Position < bytes)
            {
                stringResult = string.Format("Cannot read string of length {0}: only {1} bytes left",
                    bytes, reader.BaseStream.Length - reader.BaseStream.Position);
                return false;
            }

            var strByteData = reader.ReadBytes(bytes);
            stringResult = encoding.GetString(strByteData);
            return true;
        }

        /*
         * Read a zero-terminated Unicode string.
         */
        public static string ReadZeroTerminatedUnicode(BinaryReader reader) {
            byte[] bytes = reader.ReadBytes(0x200);
            StringBuilder builder = new StringBuilder();
            for (int i = 0; bytes[i] != 0; i += 2) {
                builder.Append(Encoding.Unicode.GetChars(bytes, i, 2));
            }
            return builder.ToString();
        }
        /*
         * Read a zero-terminated ASCII string.
         */
        static public byte[] staticBuffer = new byte[1024];
        public static string TheadUnsafeReadZeroTerminatedAscii(BinaryReader reader) 
        {
            var index = 0;
            byte ch2 = reader.ReadByte();
            while (ch2 != '\0') 
            {
                staticBuffer[index++] = ch2;
                ch2 = reader.ReadByte();
            }

            return Encoding.ASCII.GetString(staticBuffer, 0, index);
        }
        /*
         * Write the given zero-terminated ASCII string to the given writer.
         */
        public static void WriteZeroTerminatedAscii(BinaryWriter writer, string toWrite) {
            writer.Write(toWrite.ToCharArray());
            writer.Write((byte) 0);
        }
        /*
         * Write the given string to the given writer in Unicode.
         */
        public static void WriteCAString(BinaryWriter writer, string value) {
            WriteCAString (writer, value, Encoding.Unicode);
        }
        /*
         * Writer the given string to the given writer in the given encoding.
         * First writes out 2 bytes containing the string length, then the string
         * (not zero-terminated).
         */
        public static void WriteCAString(BinaryWriter writer, string value, Encoding encoding) {
            byte[] buffer = encoding.GetBytes(value);
            // utf-8 stores the number of bytes, not characters... inconsistent much?
            int len = (encoding == Encoding.UTF8) ? buffer.Length : value.Length;
            writer.Write((ushort) len);
            writer.Write(encoding.GetBytes(value));
        }
        /*
         * Write the given string to the given writer in unicode (zero-terminated).
         */
        public static void WriteZeroTerminatedUnicode(BinaryWriter writer, string value) {
            byte[] array = new byte[0x200];
            Encoding.Unicode.GetBytes(value).CopyTo(array, 0);
            writer.Write(array);
        }
  
        /*
         * Fills the given list from the given reader with data created by the given item reader.
         */
        public static void FillList<T>(List<T> toFill, ItemReader<T> readItem, BinaryReader reader, 
                                          bool skipIndex = true, int itemCount = -1) {
            try {

#if DEBUG
                long listStartPosition = reader.BaseStream.Position;
#endif
                if (itemCount == -1) {
                    itemCount = reader.ReadInt32();
                }
#if DEBUG
                Console.WriteLine("Reading list at {0:x}, {1} entries", listStartPosition, itemCount);
#endif
                for (int i = 0; i < itemCount; i++) {
                    try {
                        if (skipIndex) {
                            reader.ReadInt32();
                        }
                        toFill.Add(readItem(reader));
                    } catch (Exception ex) {
                        throw new ParseException(string.Format("Failed to read item {0}", i), 
                                                 reader.BaseStream.Position, ex);
                    }
                }
            } catch (Exception ex) {
                throw new ParseException(string.Format("Failed to entries for list {0}"), 
                                         reader.BaseStream.Position, ex);
            }
        }
        /*
         * Delegate for methods reading data from a reader.
         */
        public delegate T ItemReader<T>(BinaryReader reader);
    }
}

