using System;
using System.Collections.Generic;

namespace Filetypes
{
    public enum DbTypesEnum
    { 
        String,
        String_ascii,
        Optstring,
        Optstring_ascii,
        Integer,
        Short,
        Single,
        Boolean,
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
                case DbTypesEnum.Integer:
                    return IntType();
                case DbTypesEnum.Short:
                    return ShortType();
                case DbTypesEnum.Single:
                    return SingleType();
                case DbTypesEnum.Boolean:
                    return BoolType();
                case DbTypesEnum.List:
                    return ListType();
            }
            throw new InvalidOperationException(String.Format("Cannot create field info from {0}", typeEnum.ToString()));
        }

        public static FieldInfo StringType() { return new StringType() { Name = "unknown" }; }
        public static FieldInfo StringTypeAscii() { return new StringTypeAscii() { Name = "unknown" }; }
        public static FieldInfo IntType() { return new IntType() { Name = "unknown" }; }
        public static FieldInfo ShortType() { return new ShortType() { Name = "unknown" }; }
        public static FieldInfo BoolType() { return new BoolType() { Name = "unknown" }; }
        public static FieldInfo OptStringType() { return new OptStringType() { Name = "unknown" }; }
        public static FieldInfo OptStringTypeAscii() { return new OptStringTypeAscii() { Name = "unknown" }; }
        public static FieldInfo SingleType() { return new SingleType() { Name = "unknown" }; }
        public static FieldInfo ListType() { return new ListType() { Name = "unknown" }; }
    }
 
    /*
     * A reference to a field in a specific table.
     */
    public class FieldReference
    {
        static char[] SEPARATOR = { '.' };
  
        public FieldReference(string encoded) 
        {
            string[] parts = encoded.Split(SEPARATOR);
            if (parts.Length == 2)
            {
                Table = parts[0];
                Field = parts[1];
            }
        }

        public string Table { get; set; }
        public string Field { get; set; }

        public override string ToString() 
        {
            string result = "";
            if (!string.IsNullOrEmpty(Table) && !string.IsNullOrEmpty(Field)) 
                result = FormatReference(Table, Field);
  
            return result;
        }

        string FormatReference(string table, string field) 
        {
            return string.Format("{0}.{1}", table, field);
        }
    }

    /*
     * The info determining a column of a db table.
     */
    [System.Diagnostics.DebuggerDisplay("{Name} - {TypeName}; {Optional}")]
    public abstract class FieldInfo 
    {
        public string Name { get;set;}
        public DbTypesEnum TypeEnum { get;  set; }
        public virtual string TypeName { get; set; }
        public TypeCode TypeCode { get; set; }

        /*
         * Primary keys have to be unique amonst a given table data set.
         * There may be more than one primary key, in which case the combination
         * of their values has to be unique.
         */
        public bool PrimaryKey { get; set; }

        FieldReference reference;
        public string ForeignReference 
        {
            get { return reference != null ? reference.ToString() : "";}
            set {reference = new FieldReference(value);}
        }

        public abstract FieldInstance CreateInstance();

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
	}

	public class StringType : FieldInfo {
		public StringType () {
			TypeName = "string";
			TypeCode = TypeCode.String;
            TypeEnum = DbTypesEnum.String;

        }
        public override FieldInstance CreateInstance() {
            return new StringField() {
                Name = this.Name,
                Value = ""
            };
        }
    }

    public class StringTypeAscii : FieldInfo {
         public StringTypeAscii () {
             TypeName = "string_ascii";
             TypeCode = TypeCode.String;
            TypeEnum = DbTypesEnum.String_ascii;
         }
        public override FieldInstance CreateInstance() {
            return new StringFieldAscii() {
                Name = this.Name,
                Value = ""
            };
        }
    }

    public class IntType : FieldInfo {
		public IntType () {
			TypeName = "int";
			TypeCode = TypeCode.Int32;
            TypeEnum = DbTypesEnum.Integer;

        }
        public override FieldInstance CreateInstance() {
            return new IntField() {
                Name = this.Name,
                Value = "0"
            };
        }
    }

    public class ShortType : FieldInfo {
		public ShortType () {
			TypeName = "short";
			TypeCode = TypeCode.Int16;
            TypeEnum = DbTypesEnum.Short;
        }
        public override FieldInstance CreateInstance() {
            return new ShortField() {
                Name = this.Name,
                Value = "0"
            };
        }

    }

    public class SingleType : FieldInfo {
		public SingleType () {
			TypeName = "float";
			TypeCode = TypeCode.Single;
            TypeEnum = DbTypesEnum.Single;

        }
        public override FieldInstance CreateInstance() {
            return new SingleField() {
                Name = this.Name,
                Value = "0"
            };
        }
    }

    public class BoolType : FieldInfo {
		public BoolType () {
			TypeName = "boolean";
			TypeCode = TypeCode.Boolean;
            TypeEnum = DbTypesEnum.Boolean;

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
            TypeEnum = DbTypesEnum.Optstring;

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
            TypeEnum = DbTypesEnum.Optstring_ascii;
        }
        public override FieldInstance CreateInstance() {
            return new OptStringFieldAscii() {
                Name = this.Name,
                Value = ""
            };
        }
    }

    public class ListType : FieldInfo {
        public ListType() {
            TypeName = "list";
            TypeCode = TypeCode.Object;
            TypeEnum = DbTypesEnum.List;
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