using Aga.Controls.Tree;
using Common;
using System.Collections.ObjectModel;
using System.Drawing;

namespace PackFileManager.PackedTreeView
{
    public interface ITreeViewColourHelper
    {
        void SetColourBasedOnValidation(Collection<Node> nodes);
    }
}
