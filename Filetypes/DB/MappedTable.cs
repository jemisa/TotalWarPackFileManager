using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Filetypes {
    using NameMapping = System.Tuple<string, string>;

    public abstract class MappedTable {
        private string tableName;
        public MappedTable(string name) {
            tableName = name;
        }
        public string TableName {
            get { return tableName; }
        }

        public string Guid { get; set; }
        public abstract List<string> PackDataFields { get; }
        public abstract List<string> XmlDataFields { get; }
        public abstract Dictionary<string, string> ConstantPackValues { get; }
        public abstract Dictionary<string, string> ConstantXmlValues { get; }

        #region Mapping
        public bool IsFullyMapped {
            get {
                // will we have values for all xml elements: 
                // either from the pack or a constant?
                return mappedFields.Count + ConstantPackValues.Count == PackDataFields.Count ||
                    // or all packed fields are mapped, meaning the remaining xml fields are ignored
                    UnmappedXmlFieldNames.Count == 0;
            }
        }
        public List<string> UnmappedPackFieldNames {
            get {
                List<string> result = new List<string>();
                // List<string> constantPackFields = new List<string>(ConstantPackValues.Keys);
                PackDataFields.ForEach(f => { 
                    if (!mappedFields.ContainsKey(f) && !ConstantPackValues.ContainsKey(f)) { 
                        result.Add(f); 
                    } 
                });
                return result;
            }
        }
        public List<string> UnmappedXmlFieldNames {
            get {
                List<string> result = new List<string>();
                foreach (string f in XmlDataFields) {
                    if (!mappedFields.ContainsValue(f) && !ConstantXmlValues.ContainsKey(f)) {
                        result.Add(f);
                    }
                }
                return result;
            }
        }

        protected Dictionary<string, string> mappedFields = new Dictionary<string, string>();
        public virtual void AddMapping(string packField, string xmlField) {
            mappedFields[packField] = xmlField;
        }
        public Dictionary<string, string> Mappings {
            get { return mappedFields; }
        }
        #endregion

        #region References
        private Dictionary<string, FieldReference> references = new Dictionary<string, FieldReference>();
        public Dictionary<string, FieldReference> References {
            get { return references; }
        }
        public FieldReference GetReference(string field) {
            FieldReference result = null;
            if (references.ContainsKey(field)) {
                result = references[field];
            }
            return result;
        }
        #endregion

        public virtual string GetConstantValue(string packedFieldName) {
            string result = null;
            if (ConstantPackValues.ContainsKey(packedFieldName)) {
                result = ConstantPackValues[packedFieldName];
            }
            return result;
        }
    }

    class MappedInfoTable : MappedTable {
        public MappedInfoTable(string name) : base(name) { }

        public override void AddMapping(string packField, string xmlField) {
            base.AddMapping(packField, xmlField);
            packDataFields.Add(packField);
            xmlDataFields.Add(xmlField);
        }

        private List<string> packDataFields = new List<string>();
        public override List<string> PackDataFields {
            get {
                return packDataFields;
            }
        }
        private List<string> xmlDataFields = new List<string>();
        public override List<string> XmlDataFields {
            get {
                return xmlDataFields;
            }
        }

        protected Dictionary<string, string> constantPackValues = new Dictionary<string, string>();
        public override Dictionary<string, string> ConstantPackValues {
            get { return constantPackValues; }
        }
        public void AddConstantPackValue(string field, string value) {
            packDataFields.Add(field);
            constantPackValues.Add(field, value);
        }

        protected Dictionary<string, string> constantXmlValues = new Dictionary<string, string>();
        public override Dictionary<string, string> ConstantXmlValues {
            get { return constantXmlValues; }
        }
        public void AddConstantXmlValue(string field, string value) {
            packDataFields.Add(field);
            constantXmlValues.Add(field, value);
        }
    }
}
