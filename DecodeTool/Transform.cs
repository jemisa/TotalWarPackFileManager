using System;
using System.Collections.Generic;
using Filetypes;

namespace DecodeTool {
    public interface ITransform {
        string Name { get; }
        List<FieldInfo> Transform(List<FieldInfo> transformFrom);
        bool CanTransform(List<FieldInfo> info);
    }

    /*
     * All available transformations.
     */
    public class Transformations {
        public static readonly ITransform[] TRANSFORMS = new ITransform[] {
            new FixedTransform("optstring_ascii", "boolean"),
            new EncodingTransform(),
            new FixedTransform("4*boolean", "int"),
            new FixedTransform("int", "float"),
            new FixedTransform ("string", "int"),
            new ListTransform(),
            new ListAdd(),
            new ListDissolve()
        };
    }
    
    public class EncodingTransform : ITransform {
        public string Name { get { return "change Encoding"; }}
        static List<string> valid = new List<string>(new string[] {
            "string", "string_ascii", "optstring", "optstring_ascii"
        });
        
        public bool CanTransform(List<FieldInfo> infos) {
            bool result = infos.Count > 0;
            foreach(FieldInfo info in infos) {
                if (!valid.Contains(info.TypeName)) {
                    result = false;
                    break;
                }
            }
            return result;
        }
        
        public List<FieldInfo> Transform(List<FieldInfo> infos) {
            List<FieldInfo> result = new List<FieldInfo>();
            foreach(FieldInfo info in infos) {
                string newTypeName = "";
                if (info.TypeName.Contains("_ascii")) {
                    newTypeName = info.TypeName.Remove(info.TypeName.IndexOf("_ascii"));
                } else {
                    newTypeName = string.Format("{0}_ascii", info.TypeName);
                }
                FieldInfo transformed = Types.FromTypeName(newTypeName);
                transformed.Name = info.Name;
                result.Add (transformed);
            }
            return result;
        }
    }
    
    /*
     * Creates a list from a single int and a number of additional entries.
     */
    public class ListTransform : ITransform {
        public string Name { get { return "Fold List"; } }
  
        /* Can transform if the first item is an int value. */
        public bool CanTransform(List<FieldInfo> infos) {
            bool result = infos.Count >= 1;
            if (result) {
                result = infos [0].TypeCode == TypeCode.Int32;
            }
            return result;
        }
        /* Create a list of types beginning with the second entry of the given list 
         * (first is entry number). */
        public List<FieldInfo> Transform(List<FieldInfo> infos) {
            List<FieldInfo> result = new List<FieldInfo> ();
            List<FieldInfo> contained = new List<FieldInfo> ();
            for (int i = 1; i < infos.Count; i++) {
                contained.Add (infos [i]);
            }
            ListType list = new ListType () {
                Name = "unknown",
                Infos = contained
            };
            result.Add (list);
            return result;
        }
    }
    
    /*
     * If a list type first item in the list to transform,
     * add all following types to the definition of that list.
     */
    public class ListAdd : ITransform {
        public string Name { get { return "Add to List"; } }
  
        /* Can transform when the first item is a list type and there are 2 or more
         * items in the list (otherwise there's nothing to add) */
        public bool CanTransform(List<FieldInfo> infos) {
            bool result = infos.Count > 1;
            if (result) {
                result = infos [0].TypeCode == TypeCode.Object;
            }
            return result;
        }
        /* Add to the list in the first entry all entries following it. */
        public List<FieldInfo> Transform(List<FieldInfo> infos) {
            List<FieldInfo> result = new List<FieldInfo> ();
            ListType listType = infos [0] as ListType;
            for (int i = 1; i < infos.Count; i++) {
                listType.Infos.Add (infos [i]);
            }
            result.Add (listType);
            return result;
        }
    }
    
    /*
     * Dissolves a list to its elements.
     */
    public class ListDissolve : ITransform {
        public string Name { get { return "Dissolve List"; } }
        
        /* Can transform when all items are lists. */
        public bool CanTransform(List<FieldInfo> infos) {
            bool result = true;
            infos.ForEach (i => result &= i.TypeCode == TypeCode.Object);
            return result;
        }
        
        /* Go through all items and replace the lists by the entry count int
         * and contained types. */
        public List<FieldInfo> Transform(List<FieldInfo> infos) {
            List<FieldInfo> result = new List<FieldInfo> ();
            result.Add(Types.IntType());
            infos.ForEach (i => result.AddRange ((i as ListType).Infos));
            return result;
        }
    }
    
    /*
     * A transformer that will transform a (fixed number of types) to other type(s).
     */
    public class FixedTransform : ITransform {
        public string Name { get; private set; }
        
        List<string> fromTypes = new List<string>();
        List<string> toTypes = new List<string>();
        
        static readonly string SEPARATOR = ",";
        static readonly string MULTIPLIER = "*";
        
        public FixedTransform (string parseFrom, string parseTo) {
            Parse (ref fromTypes, parseFrom);
            Parse (ref toTypes, parseTo);
            Name = string.Format ("{0}<->{1}", string.Join (",", fromTypes), string.Join (",", toTypes));
        }

        /* Can transform when the list matches either way of this transformation. */
        public bool CanTransform(List<FieldInfo> infos) {
            return IsMatch (infos, fromTypes) || IsMatch (infos, toTypes);
        }
        
        /* Transform by taking out the types defined by fromTypes, and replacing them
         * by the ones in toTypes. */
        public List<FieldInfo> Transform(List<FieldInfo> transformFrom) {
            List<FieldInfo> result = new List<FieldInfo> ();
            List<string> targetTypes = IsMatch (transformFrom, fromTypes) ? toTypes : fromTypes;
            targetTypes.ForEach (type => result.Add (Types.FromTypeName (type)));
            return result;
        }
        
        /* Utility method to see if the given infos match the type names in matchTo. */
        static bool IsMatch(List<FieldInfo> infos, List<string> matchTo) {
            bool result = infos.Count == matchTo.Count;
            if (result) {
                for (int i = 0; i < infos.Count; i++) {
                    result &= infos [i].TypeName.Equals (matchTo [i]);
                }
            }
            return result;
        }
        
        /* Utility method to parse a list of types: 
         * typelist = numType(","numType)*
         * numType = num"*"typename
         */
        static void Parse(ref List<string> typeNames, string toParse) {
            string[] typeList = toParse.Split (SEPARATOR.ToCharArray (), StringSplitOptions.RemoveEmptyEntries);
            foreach (string numberedType in typeList) {
                string[] tuple = numberedType.Split (MULTIPLIER.ToCharArray (), StringSplitOptions.RemoveEmptyEntries);
                int addCount = (tuple.Length == 2) ? int.Parse (tuple [0]) : 1;
                int typeNameIndex = (tuple.Length == 2) ? 1 : 0;
                FieldInfo parsedInfo = Types.FromTypeName (tuple [typeNameIndex]);
                if (parsedInfo == null) {
                    throw new InvalidOperationException ("Not a valid type string: " + tuple [1]);
                }
                for (int i = 0; i < addCount; i++) {
                    typeNames.Add (tuple [typeNameIndex]);
                }
            }
        }
    }
}

