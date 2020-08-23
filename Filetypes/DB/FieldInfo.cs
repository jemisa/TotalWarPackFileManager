using System;
using System.Collections.Generic;

namespace Filetypes
{
    public enum DbTypesEnum
    { 
        Byte,
        String,
        String_ascii,
        Optstring,
        Optstring_ascii,
        Integer,
        uint32,
        Short,
        Single,
        Boolean,
        List,
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