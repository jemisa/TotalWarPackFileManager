using Common;
using Filetypes.RigidModel;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;

using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using TreeViewWithCheckBoxes;
using VariantMeshEditor.Controls;
using VariantMeshEditor.Util;
using VariantMeshEditor.Views.Animation;


using Viewer.GraphicModels;

namespace VariantMeshEditor
{
    /// <summary>
    /// Interaction logic for UserControl1.xaml
    /// </summary>
    public partial class VariantMeshEditorControl : UserControl
    {

        SceneTreeViewController _treeViewController;

        EditorMainController _mainController;
        public VariantMeshEditorControl()
        {
            InitializeComponent();

            _treeViewController = new SceneTreeViewController(EditorPanel.TreeView.tree);
            _mainController = new EditorMainController(_treeViewController, RenderView.Scene, EditorPanel.ToolPanel);
            _mainController.LoadModel("variantmeshes\\variantmeshdefinitions\\brt_paladin.variantmeshdefinition");

        }
    }
}
