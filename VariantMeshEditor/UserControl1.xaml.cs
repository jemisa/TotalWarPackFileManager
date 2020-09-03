using Common;
using Filetypes.RigidModel;
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
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using TreeViewWithCheckBoxes;
using VariantMeshEditor.Util;
using VariantMeshEditor.Views.Animation;

using VariantMeshEditor.Views.VariantMesh;

namespace VariantMeshEditor
{
    /// <summary>
    /// Interaction logic for UserControl1.xaml
    /// </summary>
    public partial class UserControl1 : UserControl
    {

        List<PackFile> Load()
        {
            var output = new List<PackFile>();
            PackLoadSequence allFiles = new PackLoadSequence
            {
                IncludePacksContaining = delegate (string s) { return true; }
            };

            List<string> packPaths = allFiles.GetPacksLoadedFrom(Game.TWH2.GameDirectory);
            packPaths.Reverse();

            PackFileCodec codec = new PackFileCodec();
            foreach (string path in packPaths)
            {
                PackFile pack = codec.Open(path);
                output.Add(pack);
            }

            return output;
        }




        public UserControl1()
        {

           

            InitializeComponent();

            MyTree.tree.DataContext = TreeViewDataModel.CreateFoos();
            /*var b = new Button();
            b.Content = "Code button";
            DockPanel.SetDock(b, Dock.Top);
           // b.dock
            DockPanel.Children.Add(b);*/




            var packedFiles = Load();

            var path = @"C:\Users\ole_k\Desktop\ModelDecoding\brt_paladin\";
            //var variantMeshDefinition = VariantMeshDefinition.Create(path + "VariantMeshDef.txt");


            SceneLoader sceneLoader = new SceneLoader(Load());
            var scene = sceneLoader.Load("variantmeshes\\variantmeshdefinitions\\brt_paladin.variantmeshdefinition");


            //var animationControl = new AnimationControl();
            //animationControl.Initialize();
            //this.ContentControl.Children.Add(animationControl);


            //var VariantMeshContainer = new VariantMeshControl();
            //VariantMeshContainer.Initialize(variantMeshDefinition);
            //this.ContentControl.Children.Add(VariantMeshContainer);





            /*
             
                         var test = new List<TreeModel>();
            test.Add(new TreeModel() { Name = "ole" });
            test.Add(new TreeModel() { Name = "Dole" });
            test.Add(new TreeModel() { Name = "Doffe" });

            test[1].Children.Add(new TreeModel() { Name = "Wa" });
            this.ItemsSourceData = test;//treeList is of type IList<TreeModel>
             */
            //ContentTreeView.ItemsSourceData = new List<TreeModel>() { Create(scene) };
         
           ;
            
        }

        /*TreeModel Create(FileSceneElement scene, TreeModel parent = null)
        {
            TreeModel node = new TreeModel()
            {
                Name = scene.ToString(),
                IsChecked = true,
            };

            node.PropertyChanged += Node_PropertyChanged;
            

            foreach (var item in scene.Children)
                Create(item, node);
            
            if(parent != null)
                parent.Children.Add(node);

            return node;
        }*/

        private void Node_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
           // throw new NotImplementedException();
        }
    }
}
