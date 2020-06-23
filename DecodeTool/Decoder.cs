using Common;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using Filetypes;

namespace DecodeTool {

    public delegate void ValueChanged();

    class Decoder {
        byte[] bytes = {};
        List<FieldInfo> descriptions = new List<FieldInfo>();

        public event ValueChanged ChangeListener;

        public byte[] Bytes {
            set {
                bytes = value;
                if (ChangeListener != null) {
                    ChangeListener();
                }
            }
        }
        public List<string> CurrentDecoded {
            get {
                List<string> result = new List<string>();
                using (BinaryReader reader = new BinaryReader(new MemoryStream(bytes))) {
                    descriptions.ForEach(delegate(FieldInfo d) { result.Add(Util.decodeSafe(d, reader)); });
                }
                return result;
            }
        }
//        public int DecodedByteCount {
//            get {
//                int count = 0;
//                using (BinaryReader reader = new BinaryReader(new MemoryStream(bytes))) {
//                    foreach (FieldInfo d in descriptions) {
//                        try {
//                            string decoded = d.Decode(reader);
//                            count += d.Length(decoded);
//                        } catch {
//                            break;
//                        }
//                    }
//                }
//                return count;
//            }
//        }
        public void addType(FieldInfo type) {
            descriptions.Add(type);
            ChangeListener();
        }
    }
}
