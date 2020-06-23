using System;
using System.Collections.Generic;

namespace EsfLibrary {
    public abstract class NodeIterator {
        // return false to stop iteration
        public delegate bool Visitor(EsfNode node);

        public virtual Visitor Visit { get; set; }
        public abstract bool Iterate(EsfNode node);
    }
    public class ParentIterator : NodeIterator {
        public override bool Iterate(EsfNode node) {
            bool result = (node != null && Visit(node));
            if (result) {
                Iterate(node.Parent as ParentNode);
            }
            return result;
        }
    }
    public class SiblingIterator : NodeIterator {
        public override bool Iterate(EsfNode fromNode) {
            ParentNode parent = fromNode.Parent as ParentNode;
            bool result = (parent != null);
            if (result) {
                bool pastNode = false;
                foreach(EsfNode candidate in parent.AllNodes) {
                    if (pastNode) {
                        if (!Visit(candidate)) {
                            result = false;
                            break;
                        }
                    } else {
                        pastNode = (candidate == fromNode);
                    }
                }
            }
            return result;
        }
    }
    public class ChildIterator : NodeIterator {
        public override bool Iterate(EsfNode node) {
            bool result = Visit(node);
            if (result) {
                ParentNode parent = node as ParentNode;
                if (parent != null) {
                    parent.AllNodes.ForEach(n => Iterate (n));
                }
            }
            return result;
        }
    }
}

