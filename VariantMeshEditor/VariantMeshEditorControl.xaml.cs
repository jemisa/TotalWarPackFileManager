using System.Windows.Controls;
using VariantMeshEditor.Controls;

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
