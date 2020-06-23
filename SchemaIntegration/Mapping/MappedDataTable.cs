using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Filetypes;

using NameMapping = System.Tuple<string, string>;

namespace SchemaIntegration.Mapping {
    class MappedDataTable : MappedTable {
        public MappedDataTable(string name) : base(name) { }

        #region Data Tables
        // tables containing fields with actual data
        private DataTable packData = new DataTable();
        private DataTable xmlData = new DataTable();
        public DataTable PackData {
            get {
                return packData;
            }
        }
        public DataTable XmlData {
            get {
                return xmlData;
            }
        }
        #endregion

        public override List<string> PackDataFields {
            get { return packData.Fields; }
        }
        public override List<string> XmlDataFields {
            get { return xmlData.Fields; }
        }
        
        Dictionary<string, string> xmlDataTypes = new Dictionary<string, string>();
        public Dictionary<string, string> XmlDataTypes {
            get { return xmlDataTypes; }
        }
        
        public override Dictionary<string, string> ConstantXmlValues {
            get {
                Dictionary<string, string> result = new Dictionary<string, string>();
                if (mappedFields.Count == PackDataFields.Count) {
                    // all pack fields are mapped... we'll provide values
                    // for the unmapped xml ones
                    foreach(string xmlField in XmlDataFields) {
                        if (mappedFields.ContainsValue(xmlField)) {
                            continue;
                        }
                        string value = "";
                        if (xmlDataTypes.ContainsKey(xmlField)) {
                            string fieldType = xmlDataTypes[xmlField];
                            value = GenerateDefaultValue(fieldType);
                        }
                        result.Add(xmlField, value);
                    }
                }
                return result;
            }
        }

        public override Dictionary<string, string> ConstantPackValues {
            get {
                Dictionary<string, string> result = new Dictionary<string, string>();

                if (UnmappedXmlFieldNames.Count == 0) {
                    // there are more fields in the pack than in the xml...
                    // that data had to come from somewhere.
                    // if all columns in those fields have the same value,
                    // assume it is constant and just inserted by the export.
                    List<string> unmapped = new List<string>(PackDataFields);

                    // we cannot use UnmappedPackFieldNames here, because it 
                    // calls ConstantValues...
                    unmapped.RemoveAll(IsMappedPackField);
                    foreach (string packFieldName in unmapped) {                                                              
                        List<string> values = packData.Values(packFieldName);                                                 
                        if (values.Count != 0) {                                                                              
                            string lastValue = values[0];                                                                     
                            bool allValuesEqual = true;                                                                       
                            foreach (string value in values) {                                                                
                                allValuesEqual &= value.Equals(lastValue);                                                    
                                if (!allValuesEqual) {                                                                        
                                    break;                                                                                    
                                }
                            }
                            if (allValuesEqual) {
                                result.Add(packFieldName, lastValue);
                            }
                        }
                    }
                }
                return result;
            }
        }
        
        private bool IsMappedPackField(string fieldName) {
            return mappedFields.ContainsKey(fieldName);
        }

        static string GenerateDefaultValue(string fieldType) {
            switch (fieldType) {
            case "integer":
            case "longinteger":
            case "autonumber":
            case "decimal":
            case "single":
            case "double":
            case "yesno":
                return "0";
            case "text":
            case "memo":
                return "";
            }
            throw new InvalidOperationException(string.Format("unknown type {0}", fieldType));
        }
    }

    class DataTable {
        Dictionary<string, List<string>> values = new Dictionary<string, List<string>>();

        public List<string> Fields {
            get {
                return new List<string>(values.Keys);
            }
        }

        public List<string> Values(string field) {
            return values[field];
        }

        public List<string> FieldsContainingValues(List<string> toMatch) {
            List<string> fields = new List<string>();
            foreach (string field in values.Keys) {
                if (SameValues(toMatch, values[field])) {
                    fields.Add(field);
                }
            }
            return fields;
        }

        public void AddFieldValue(string fieldName, string value) {
            List<string> list;
            if (values.ContainsKey(fieldName)) {
                list = values[fieldName];
            } else {
                list = new List<string>();
                values[fieldName] = list;
            }
            list.Add(value);
        }

        /*
         * Helper method to compare values from the given list.
         * Performs some conversions (rounds CA floats to 2 digits and transforms
         * binary's bools to ints (1 and 0 respectively).
         */
        public static bool SameValues(List<string> values1, List<string> values2) {
            bool result = values1.Count == values2.Count;
            if (result) {
                for (int i = 0; i < values1.Count; i++) {
                    result = values1[i].Equals(values2[i]);
                    if (!result) {
                        // maybe floats? Those are rounded differently
                        try {
                            double value1;
                            double value2;
                            bool bValue1;
                            int iValue2;
                            string v2 = values2[i].Replace(".", ",");
                            bool parsed = double.TryParse(values1[i], out value1);
                            parsed &= double.TryParse(v2, out value2);
                            if (parsed) {
                                value1 = Math.Round(value1, 2);
                                value2 = Math.Round(value2, 2);
                                result = value1 == value2;
                            } else if (bool.TryParse(values1[i], out bValue1) &&
                                       int.TryParse(values2[i], out iValue2)) {
                                int iValue1 = bValue1 ? 1 : 0;
                                result = iValue1 == iValue2;
                            }
                        } catch { }
                        if (!result) {
                            break;
                        }
                    }
                }
            }
            return result;
        }
    }
}
