using Aga.Controls.Tree;
using Common;
using Filetypes;
using Filetypes.Codecs;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PackFileManager.PackedTreeView
{
    class TreeViewColourHelper
    {
        public static void SetColourBasedOnValidation(Collection<Node> nodes)
        {
            foreach (var node in nodes)
            {
                var packedFile = node.Tag as PackedFile;
                if (packedFile != null)
                {
                    if (packedFile.FullPath.StartsWith("db"))
                    {
                        var colourNode = node as TreeNode;
                        DBFileHeader header = PackedFileDbCodec.readHeader(packedFile);
                        string mouseover;

                        if (header.EntryCount == 0) // empty db file
                        {
                            colourNode.Colour = Color.Blue;
                            SetColourForParent(colourNode, colourNode.Colour.Value);
                        }
                        else if (!PackedFileDbCodec.CanDecode(packedFile, out mouseover))
                        {
                            colourNode.Colour = Color.Red;
                            SetColourForAllParents(colourNode, colourNode.Colour.Value);

                            colourNode.ToolTipText = mouseover;
                        }
                        else if (HeaderVersionObsolete(packedFile))
                        {
                            colourNode.Colour = Color.Yellow;
                            SetColourForParent(colourNode, colourNode.Colour.Value);
                        }
                    }
                }
                else
                {
                    SetColourBasedOnValidation(node.Nodes);
                }
            }
        }

        public static bool HeaderVersionObsolete(PackedFile packedFile)
        {
            DBFileHeader header = PackedFileDbCodec.readHeader(packedFile);
            string type = DBFile.Typename(packedFile.FullPath);
            int maxVersion = GameManager.Instance.GetMaxDbVersion(type);
            return DBTypeMap.Instance.IsSupported(type) && maxVersion != 0 && (header.Version < maxVersion);
        }

        static void SetColourForAllParents(TreeNode node, Color c)
        {
            if (node.Parent != null)
            {
                var cNode = node.Parent as TreeNode;
                if (cNode != null)
                {
                    cNode.Colour = c;
                    SetColourForAllParents(cNode, c);
                }
            }
        }

        static void SetColourForParent(TreeNode node, Color c)
        {
            if (node.Parent != null)
            {
                var cNode = node.Parent as TreeNode;
                if (cNode != null)
                {
                    cNode.Colour = c;
                }
            }
        }

    }
}
