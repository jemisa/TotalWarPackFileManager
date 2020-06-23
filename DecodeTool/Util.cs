using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using Filetypes;

namespace DecodeTool {       
    public class Util {
        public static String FormatHex(byte[] bytes, long offset, long count) {
            offset = Math.Min(offset, bytes.Length);
            if (bytes.Length <= offset) {
                return "";
            }

            count = Math.Min(count, bytes.Length - offset);
   
			StringBuilder result = new StringBuilder (3 * (int)count);
			result.Append (string.Format ("{0:x2}", bytes [offset]));
			for (long i = 1; i < count; i++) {
				result.Append (string.Format (" {0:x2}", bytes[offset + i]));
			}
			return result.ToString ();
		}

        public static string decodeSafe(FieldInfo d, BinaryReader reader) {
			string result = "failure";
			try {
                FieldInstance field = d.CreateInstance();
				field.Decode (reader);
                result = field.Value;
			} catch (Exception x) {
#if DEBUG
                // Console.WriteLine(x);
#endif
				result = x.Message.Replace ("\n", "-");
			}
			return result;
		}

        /*
        public static List<FieldInfo> Convert(FieldInfo info, ref List<String> names) {
            bool nextOptional = false;
            names.Clear();
            List<FieldInfo> descriptions = new List<FieldInfo>();
            foreach (FieldInfo field in info.fields) {
                FieldInfo description;
                if (field.Mod == FieldInfo.Modifier.NextFieldIsConditional) {
                    nextOptional = true;
                    continue;
                } else if (nextOptional) {
                    description = Types.OptStringType;
                } else {
                    description = Convert(field);
                }
                nextOptional = false;
                names.Add(field.Name);
                descriptions.Add(description);
            }
            return descriptions;
        }

        /*
        public static FieldInfo Convert(FieldInfo info) {
			switch (info.TypeCode) {
			case PackTypeCode.Boolean:
				return Types.BoolType;
			case PackTypeCode.UInt16:
				return Types.ShortType;
			case PackTypeCode.Int32:
			case PackTypeCode.UInt32:
				return Types.IntType;
			case PackTypeCode.Single:
				return Types.SingleType;
			case PackTypeCode.String:
				return Types.StringType;
			case PackTypeCode.Empty:
				return new VarBytesDescription (info.Length);
			}
			throw new InvalidDataException ("unknown type");
		}
         * */
    }
}
