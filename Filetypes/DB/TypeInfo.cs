using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.InteropServices;

namespace Filetypes {
    public class TypeVersionTuple {
        public string Type { get; set; }
        public int MaxVersion { get; set; }
    }

    [StructLayout(LayoutKind.Sequential)]
    public class TypeInfo : IComparable<TypeInfo> {
		public string Name {
            get; set;
        }
        int version = 0;
        public int Version {
            get {
                return version;
            }
            set {
                version = value;
            }
        }
		List<FieldInfo> fields = new List<FieldInfo> ();
        public List<FieldInfo> Fields {
            get {
                return fields;
            }
        }
        public FieldInfo this[string name] {
            get {
                FieldInfo result = null;
                foreach(FieldInfo field in fields) {
                    if (field.Name.Equals(name)) {
                        result = field;
                        break;
                    }
                }
                return result;
            }
        }
  
        #region Constructors
		public TypeInfo () {
		}

		public TypeInfo (List<FieldInfo> addFields) {
			Fields.AddRange(addFields);
		}

		public TypeInfo (TypeInfo toCopy) {
			Name = toCopy.Name;
            Version = toCopy.Version;
			Fields.AddRange (toCopy.Fields);
		}
        #endregion
  
        public bool SameTypes(TypeInfo other) {
            bool typesMatch = fields.Count == other.fields.Count;
            if (typesMatch) {
                for (int i = 0; i < fields.Count && typesMatch; i++) {
                    if (!fields[i].TypeName.Equals(other.fields[i].TypeName)) {
                        typesMatch = false;
                    }
                }
            }
            return typesMatch;
        }

        public int CompareTo(TypeInfo other) {
            int result = Name.CompareTo(other.Name);
            if (result == 0) {
                result = Version - other.Version;
            }
            if (result == 0) {
                result = Fields.Count - other.Fields.Count;
            }
            if (result == 0) {
                for (int i = 0; i < Fields.Count; i++) {
                    result = Fields[i].Name.CompareTo(other.Fields[i].Name);
                    if (result == 0) {
                        result = Fields[i].TypeName.CompareTo(other.Fields[i].TypeName);
                    }
                    if (result != 0) {
                        break;
                    }
                }
            }
            return result;
        }
        
        public override string ToString() {
            return string.Format("Name={0}, Version={1}, {2} Fields", Name, Version, Fields.Count);
        }
    }
}

