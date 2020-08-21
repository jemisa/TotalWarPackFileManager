using System;
using System.Collections.Generic;

namespace Filetypes 
{


    public class DBRow : List<FieldInstance> {
        private TypeInfo info;
        
        public DBRow (TypeInfo i, List<FieldInstance> val) : base(val) {
            info = i;
        }
        public DBRow (TypeInfo i) : this(i, CreateRow(i)) { }
        

        public FieldInstance this[string fieldName] {
            get {
                return this[IndexOfField(fieldName)];
            }
            set {
                this[IndexOfField(fieldName)] = value;
            }
        }

        /**
         * <summary>Checks whether this DBRow has equivalent data to <paramref name="row"/>.</summary>
         * <remarks>This returns true if the data is equivalent even if the schemas aren't.  e.g.: A string with the same contents as a string_ascii will still allow this to evaluate to true instead of forcing a false value.</remarks>
         * 
         * <param name="row">The DBRow for this row to be compared to.</param>
         * <returns>Whether the two DBRows have equivalent data.</returns>
         */
        public bool SameData(DBRow row)
        {
            if(Count != row.Count)
                return false;
            for(int i = 0; i < Count; ++i)
                if(!this[i].Value.Equals(row[i].Value))
                    return false;
            return true;
        }

        private int IndexOfField(string fieldName) {
            for(int i = 0; i < info.Fields.Count; i++) {
                if (info.Fields[i].Name.Equals(fieldName)) {
                    return i;
                }
            }
            throw new IndexOutOfRangeException(string.Format("Field name {0} not valid for type {1}", fieldName, info.Name));
        }
        
        public static List<FieldInstance> CreateRow(TypeInfo info) {
            List<FieldInstance> result = new List<FieldInstance>(info.Fields.Count);
            info.Fields.ForEach(f => result.Add(f.CreateInstance()));
            return result;
        }
    }
}

