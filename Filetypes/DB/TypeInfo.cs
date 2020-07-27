using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;

namespace Filetypes
{
    public class TypeVersionTuple {
        public string Type { get; set; }
        public int MaxVersion { get; set; }
    }

    [StructLayout(LayoutKind.Sequential)]
    public class TypeInfo : IComparable<TypeInfo> {
		public string Name { get; set;}
        public int Version { get; set; }
        public List<FieldInfo> Fields { get; private set; } = new List<FieldInfo>();

        public FieldInfo this[string name] {
            get 
            {
                foreach(FieldInfo field in Fields) 
                {
                    if (field.Name == name) 
                        return field;
                }
                return null;
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
            bool typesMatch = Fields.Count == other.Fields.Count;
            if (typesMatch) {
                for (int i = 0; i < Fields.Count && typesMatch; i++) {
                    if (!Fields[i].TypeName.Equals(other.Fields[i].TypeName)) {
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

