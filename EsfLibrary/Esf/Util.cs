using System;

namespace EsfLibrary {
    /*
     * A node visitor collecting a path to a given Node.
     * Create an instance of this, then give its Visit method to a ParentNodeIterator
     * and start iteration, or use the CreatePath utility Method.
     */
    public class NodePathCreator {
        public static string CreatePath(EsfNode node, string separator = "/") {
            NodePathCreator creator = new NodePathCreator {
                PathSeparator = separator
            };
            new ParentIterator {
                Visit = creator.Visit
            }.Iterate(node);
            return creator.Path;
        }
        
        string path = "";
        public string Path {
            get { return path; }
        }
        string pathSeparator = "/";
        public string PathSeparator {
            get { return pathSeparator; }
            set { pathSeparator = value; }
        }
        public bool Visit(EsfNode node) {
            INamedNode named = node as INamedNode;
            if (named is CompressedNode) {
                path = path.Substring (path.IndexOf(PathSeparator) + 1);
            } 
            if (!(named is MemoryMappedRecordNode) || string.IsNullOrEmpty(path)) {
                path = string.Format("{0}{1}{2}", named.GetName(), PathSeparator, path);
#if DEBUG
                Console.WriteLine("node {0} - {1}", named.GetName(), node.GetType());
#endif
            }
            return true;
        }
    }
}

