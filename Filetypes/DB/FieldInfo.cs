using System;
using System.IO;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using Common;

namespace Filetypes {
    /*
     * A collection of types that can be decoded from the db.
     */

    public enum DbTypesEnum
    { 
        String,
        String_ascii,
        Optstring,
        Optstring_ascii,
        Int,
        Integer,
        Autonumber,
        Short,
        Float,
        Single,
        Decimal,
        Double,
        Boolean,
        Yesno,
        List,
    }
	public class Types 
    {
        public static FieldInfo FromEnum(DbTypesEnum typeEnum)
        {
            switch (typeEnum)
            {
                case DbTypesEnum.String:
                    return StringType();
                case DbTypesEnum.String_ascii:
                    return StringTypeAscii();
                case DbTypesEnum.Optstring:
                    return OptStringType();
                case DbTypesEnum.Optstring_ascii:
                    return OptStringTypeAscii();
                case DbTypesEnum.Int:
                case DbTypesEnum.Integer:
                case DbTypesEnum.Autonumber:
                    return IntType();
                case DbTypesEnum.Short:
                    return ShortType();
                case DbTypesEnum.Float:
                case DbTypesEnum.Single:
                case DbTypesEnum.Decimal:
                case DbTypesEnum.Double:
                    return SingleType();
                // return DoubleType ();
                case DbTypesEnum.Boolean:
                case DbTypesEnum.Yesno:
                    return BoolType();
                case DbTypesEnum.List:
                    return ListType();
            }
            throw new InvalidOperationException(String.Format("Cannot create field info from {0}", typeEnum.ToString()));
        }

        public static FieldInfo FromTypeName(string typeName) {
			switch (typeName) {
			case "string":
				return StringType ();
            case "string_ascii":
                return StringTypeAscii();
			case "optstring":
				return OptStringType ();
            case "optstring_ascii":
                return OptStringTypeAscii();
			case "int":
            case "integer":
            case "autonumber":
				return IntType ();
			case "short":
				return ShortType ();
			case "float":
            case "single":
            case "decimal":
            case "double":
                return SingleType ();
                // return DoubleType ();
			case "boolean":
            case "yesno":
				return BoolType ();
            case "list":
                return ListType();
			}
			if (typeName.StartsWith ("blob")) {
				string lengthPart = typeName.Substring (4);
				int length = int.Parse (lengthPart);
				return new VarBytesType (length);
			}
            throw new InvalidOperationException(String.Format("Cannot create field info from {0}", typeName));
		}
        public static FieldInfo StringType() { return new StringType() { Name = "unknown" }; }
        public static FieldInfo StringTypeAscii() { return new StringTypeAscii() { Name = "unknown" }; }
        public static FieldInfo IntType() { return new IntType() { Name = "unknown" }; }
        public static FieldInfo ShortType() { return new ShortType() { Name = "unknown" }; }
        public static FieldInfo BoolType() { return new BoolType() { Name = "unknown" }; }
        public static FieldInfo OptStringType() { return new OptStringType() { Name = "unknown" }; }
        public static FieldInfo OptStringTypeAscii() { return new OptStringTypeAscii() { Name = "unknown" }; }
        public static FieldInfo SingleType() { return new SingleType() { Name = "unknown" }; }
        public static FieldInfo DoubleType() { return new DoubleType() { Name = "unknown" }; }
        public static FieldInfo ByteType() { return new VarBytesType(1) { Name = "unknown" }; }
        public static FieldInfo ListType() { return new ListType() { Name = "unknown" }; }
    }
 
    /*
     * A reference to a field in a specific table.
     */
    public class FieldReference {
        static char[] SEPARATOR = { '.' };
  
        /*
         * Create reference to given table and field.
         */
        public FieldReference(string table, string field) {
            Table = table;
            Field = field;
        }
        /*
         * Parse encoded reference (see #FormatReference).
         */
        public FieldReference(string encoded) 
        {
            string[] parts = encoded.Split(SEPARATOR);
            if (parts.Length == 2)
            {
                Table = parts[0];
                Field = parts[1];
            }
        }
        /*
         * Create an empty reference.
         */
        public FieldReference() {
        }

        public string Table { get; set; }
        public string Field { get; set; }

        public override string ToString() {
            string result = "";
            if (!string.IsNullOrEmpty(Table) && !string.IsNullOrEmpty(Field)) {
                result = FormatReference(Table, Field);
            }
            return result;
        }

        /*
         * Encode given table and field to format "table.field".
         */
        public static string FormatReference(string table, string field) {
            return string.Format("{0}.{1}", table, field);
        }
    }
	
    /*
     * The info determining a column of a db table.
     */
	[System.Diagnostics.DebuggerDisplay("{Name} - {TypeName}; {Optional}")]
    public abstract class FieldInfo {
        /*
         * The column name.
         */
		public string Name {
			get;
			set;
		}
        public virtual string TypeName { get; set; }
        public TypeCode TypeCode { get; set; }

        /*
         * Primary keys have to be unique amonst a given table data set.
         * There may be more than one primary key, in which case the combination
         * of their values has to be unique.
         */
        public bool PrimaryKey { get; set; }
        /*
         * There are string fields which do not need to contain data, in which
         * case they will only contain a "0" in the packed file.
         * This attribute is true for those fields.
         */
        public bool Optional { get; set; }
  
        #region Reference
        /*
         * The referenced table/field containing the valid values for this column.
         */
        FieldReference reference;
        public FieldReference FieldReference {
            get {
                return reference;
            }
            set {
                reference = value;
            }
        }
        /*
         * The referenced table/field as a string.
         */
        public string ForeignReference {
            get {
                return reference != null ? reference.ToString() : "";
            }
            set {
                reference = new FieldReference(value);
            }
        }

        // The referenced table; empty string if no reference
        public string ReferencedTable {
            get {
                return reference != null ? reference.Table : "";
            }
        }
        // The referenced field in the referenced table; empty string if no reference
        public string ReferencedField {
            get {
                return reference != null ? reference.Field : "";
            }
            set {
                reference.Field = value;
            }
        }
        #endregion

        /*
         * Create an instance valid for this field.
         */
        public abstract FieldInstance CreateInstance();

        #region Framework Overrides
        public override bool Equals(object other) {
            bool result = false;
            if (other is FieldInfo) {
                FieldInfo info = other as FieldInfo;
                result = Name.Equals(info.Name);
                result &= TypeName.Equals(info.TypeName);
            }
            return result;
        }

        public override int GetHashCode() {
            return 2*Name.GetHashCode() +
                3*TypeName.GetHashCode();
        }
        
        public override string ToString() {
            return string.Format("{0}:{1}", Name, TypeName);
        }
        #endregion
	}

	class StringType : FieldInfo {
		public StringType () {
			TypeName = "string";
			TypeCode = TypeCode.String;
		}
        public override FieldInstance CreateInstance() {
            return new StringField() {
                Name = this.Name,
                Value = ""
            };
        }
	}
    class StringTypeAscii : FieldInfo {
         public StringTypeAscii () {
             TypeName = "string_ascii";
             TypeCode = TypeCode.String;
         }
        public override FieldInstance CreateInstance() {
            return new StringFieldAscii() {
                Name = this.Name,
                Value = ""
            };
        }
    }

	class IntType : FieldInfo {
		public IntType () {
			TypeName = "int";
			TypeCode = TypeCode.Int32;
		}
        public override FieldInstance CreateInstance() {
            return new IntField() {
                Name = this.Name,
                Value = "0"
            };
        }
	}

	class ShortType : FieldInfo {
		public ShortType () {
			TypeName = "short";
			TypeCode = TypeCode.Int16;
		}
        public override FieldInstance CreateInstance() {
            return new ShortField() {
                Name = this.Name,
                Value = "0"
            };
        }
	}

	class SingleType : FieldInfo {
		public SingleType () {
			TypeName = "float";
			TypeCode = TypeCode.Single;
		}
        public override FieldInstance CreateInstance() {
            return new SingleField() {
                Name = this.Name,
                Value = "0"
            };
        }
	}

    class DoubleType : FieldInfo {
     public DoubleType () {
         TypeName = "double";
         TypeCode = TypeCode.Single;
     }
        public override FieldInstance CreateInstance() {
            return new DoubleField() {
                Name = this.Name,
                Value = "0"
            };
        }
 }

	class BoolType : FieldInfo {
		public BoolType () {
			TypeName = "boolean";
			TypeCode = TypeCode.Boolean;
		}
        public override FieldInstance CreateInstance() {
            return new BoolField() {
                Name = this.Name,
                Value = "false"
            };
        }
	}

	class OptStringType : FieldInfo {
		public OptStringType () {
			TypeName = "optstring";
			TypeCode = TypeCode.String;
		}
        public override FieldInstance CreateInstance() {
            return new OptStringField() {
                Name = this.Name,
                Value = ""
            };
        }
	}
    class OptStringTypeAscii : FieldInfo {
        public OptStringTypeAscii () {
            TypeName = "optstring_ascii";
            TypeCode = TypeCode.String;
        }
        public override FieldInstance CreateInstance() {
            return new OptStringFieldAscii() {
                Name = this.Name,
                Value = ""
            };
        }
    }

	public class VarBytesType : FieldInfo {
        int byteCount;
		public VarBytesType (int bytes) {
			TypeName = string.Format("blob{0}", byteCount);
			TypeCode = TypeCode.Empty;
            byteCount = bytes;
		}
        public override FieldInstance CreateInstance() {
            return new VarByteField(byteCount) {
                Name = this.Name
            };
        }
	}
    
    public class ListType : FieldInfo {
        public ListType() {
            TypeName = "list";
            TypeCode = TypeCode.Object;
        }
        
        public override string TypeName {
            get {
                return "list";
            }
        }
        
        List<FieldInfo> containedInfos = new List<FieldInfo>();
        public List<FieldInfo> Infos {
            get {
                return containedInfos;
            }
            set {
                containedInfos.Clear();
                if (value != null) {
                    containedInfos.AddRange(value);
                }
            }
        }
        public override FieldInstance CreateInstance() {
            ListField field = new ListField(this);
            // containedInfos.ForEach(i => field.Contained.Add(i.CreateInstance()));
            return field;
        }
        public List<FieldInstance> CreateContainedInstance() {
            List<FieldInstance> result = new List<FieldInstance>();
            containedInfos.ForEach(i => result.Add(i.CreateInstance()));
            return result;
        }
        
        public bool EncodeItemIndices {
            get {
                return false;
            }
            set {
                // ignore
            }
        }
        
        int itemIndexAt = -1;
        public int ItemIndexAt {
            get { return itemIndexAt >= Infos.Count ? -1 : itemIndexAt; }
            set { itemIndexAt = value; }
        }
        int nameAt = -1;
        public int NameAt {
            get {
                int result = nameAt >= Infos.Count ? -1 : nameAt;
                if (result == -1) {
                    // use the first string we find
                    for (int i = 0; i < Infos.Count; i++) {
                        if (Infos[i].TypeCode == System.TypeCode.String) {
                            result = i;
                            break;
                        }
                    }
                }
                return result;
            }
            set { nameAt = value; }
        }

        public override bool Equals(object other) {
            bool result = base.Equals(other);
            if (result) {
                ListType type = other as ListType;
                result &= type.containedInfos.Count == containedInfos.Count;
                if (result) {
                    for (int i = 0; i < containedInfos.Count; i++) {
                        result &= containedInfos[i].Equals(type.containedInfos[i]);
                    }
                }
            }
            return result;
        }
        
        public override int GetHashCode() {
            return 2*Name.GetHashCode() + 3*Infos.GetHashCode();
        }
    }
}