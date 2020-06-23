using Filetypes;
using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace DbSql {
    using QueryResult = List<FieldInstance>;
 
    /*
     * A class filtering out db rows by placing conditions on them.
     */
    public class WhereClause {
        // form  of the where clause
        public static Regex WHERE_RE = new Regex("where (.*)", RegexOptions.RightToLeft);
        
        Predicate<QueryResult> Accepts;
        
        /*
         * Parse given string to create where clause.
         */
        public WhereClause(string toParse) {
            toParse = toParse.Trim().Replace("where", "").Trim();
            if (OrWherePart.OR_RE.IsMatch(toParse)) {
                Accepts = new OrWherePart(toParse).Accept;
            } else if (AndWherePart.AND_RE.IsMatch(toParse)) {
                Accepts = new AndWherePart(toParse).Accept;
            } else if (!string.IsNullOrEmpty(toParse)) {
                Accepts = new FieldWherePart(toParse).Accept;
            } else {
                Accepts = r => { return true; };
            }
        }
        /*
         * Query if the given row matches this where clause.
         */
        public bool Accept(QueryResult row) {
            return Accepts(row);
        }
    }
    /*
     * Class connecting several where parts by or.
     */
    class OrWherePart {
        private List<Predicate<QueryResult>> parts = new List<Predicate<QueryResult>>();
        public static Regex OR_RE = new Regex(" or ");
        public OrWherePart(string toParse) {
            string[] split = OR_RE.Split(toParse);
            foreach(string part in split) {
                if (AndWherePart.AND_RE.IsMatch(part)) {
                    parts.Add(new AndWherePart(part).Accept);
                } else {
                    parts.Add(new FieldWherePart(part).Accept);
                }
            }
        }
        public bool Accept(QueryResult row) {
            bool result = false;
            foreach(Predicate<QueryResult> accept in parts) {
                result = result || accept(row);
                if (result) {
                    break;
                }
            }
            return result;
        }
    }
    /*
     * Class connecting several where parts by and.
     */
    class AndWherePart {
        private List<Predicate<QueryResult>> parts = new List<Predicate<QueryResult>>();
        public static Regex AND_RE = new Regex(" and ");
        public AndWherePart(string toParse) {
            string[] split = AND_RE.Split(toParse);
            foreach(string part in split) {
                parts.Add(new FieldWherePart(part).Accept);
            }
        }
        public bool Accept(QueryResult row) {
            bool result = true;
            foreach(Predicate<QueryResult> accept in parts) {
                result = result && accept(row);
                if (!result) {
                    break;
                }
            }
            return result;
        }
    }
    /*
     * Where part checking if a value is either equal ("=") or contains ("like") a given value.
     */
    class FieldWherePart {
        private string fieldName;
        private Predicate<string> valueMatches;
        static Regex LIKE_RE = new Regex(" like ");
        public FieldWherePart(string toParse) {
            if (toParse.Contains("=")) {
                string[] split = toParse.Split('=');
                fieldName = split[0].Trim();
                valueMatches = delegate(string s) {
                    return s.Equals(split[1].Trim());
                };
            } else if (toParse.Contains("like")) {
                string[] split = LIKE_RE.Split(toParse);
                fieldName = split[0].Trim();
                Regex re = new Regex(split[1].Trim());
                valueMatches = delegate(string s) {
                    return re.IsMatch(s);
                };
            } else {
                throw new Exception(string.Format("Could not parse {0}", toParse));
            }
        }
        public bool Accept(QueryResult row) {
            bool result = false;
            foreach(FieldInstance instance in row) {
                if (instance.Info.Name.Equals(fieldName)) {
                    string val = instance.Value;
                    result = valueMatches(val);
                    break;
                }
            }
            return result;
        }
    }
}

