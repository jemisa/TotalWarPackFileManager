using Common;
using Filetypes.Codecs;
using Filetypes.RigidModel;
using Pfim;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Runtime.Remoting.Channels;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using VariantMeshEditor.ViewModels;
using VariantMeshEditor.Views.EditorViews;
using VariantMeshEditor.Views.EditorViews.Util;

namespace VariantMeshEditor.Controls.EditorControllers
{
    class RigidModelController
    {
        RigidModelEditorView _view;
        RigidModelElement _element;
        List<PackFile> _loadedContent;
        public RigidModelController(RigidModelEditorView view, RigidModelElement element, List<PackFile> loadedContent )
        {
            _view = view;
            _element = element;
            _loadedContent = loadedContent;

            PopulateUi(_view, _element);
        }

        private void PopulateUi(RigidModelEditorView view, RigidModelElement element)
        {
            bool firstSub = true;
            bool first = true;
            foreach (var lod in element.Model.LodInformations)
            {
                var item = new CollapsableButtonControl($"Lod - {lod.LodLevel}");

                var stackpanel = new StackPanel();
                item.Content = stackpanel;

                
                foreach (var mesh in lod.LodModels)
                {

                    var sub = new CollapsableButtonControl($"{mesh.modelName}");
                    var substackpanel = new StackPanel();
                    sub.Content = substackpanel;

                    var meshView = new RigidModelMeshEditorView();
                    meshView.ModelType.Text = mesh.GroupType.ToString();
                    meshView.VertexType.Text = mesh.vertexType.ToString();
                    meshView.VertexCount.Text = mesh.vertexCount.ToString();
                    meshView.FaceCount.Text = mesh.faceCount.ToString();
                    meshView.SkeltonName.Text = "";// mesh.
                    meshView.MaterialName.Text = mesh.materiaType;
                    meshView.TextureDirectory.Text = mesh.textureDirectory;

                    meshView.DiffusePath.Text = GetTexuterName(mesh, TexureType.Diffuse);
                    meshView.DiffuseView.Click += (sender, file) => DisplayTexture(TexureType.Diffuse, meshView.DiffusePath);

                    meshView.AlphaPath.Text = GetTexuterName(mesh, TexureType.Alpha);
                    meshView.AlphaView.Click += (sender, file) => DisplayTexture(TexureType.Alpha, meshView.AlphaPath);

                    meshView.NormalPath.Text = GetTexuterName(mesh, TexureType.Normal);
                    meshView.NormalView.Click += (sender, file) => DisplayTexture(TexureType.Normal, meshView.NormalPath);

                    meshView.GlossPath.Text = GetTexuterName(mesh, TexureType.Gloss);
                    meshView.GlossView.Click += (sender, file) => DisplayTexture(TexureType.Gloss, meshView.GlossPath);

                    meshView.SpecularPath.Text = GetTexuterName(mesh, TexureType.Specular);
                    meshView.SpecularView.Click += (sender, file) => DisplayTexture(TexureType.Specular, meshView.SpecularPath);

                    AddUnknownTexture(meshView, mesh);

                    substackpanel.Children.Add(meshView);
                    stackpanel.Children.Add(sub);

                    if (firstSub)
                        sub.OnClick();
                    firstSub = false;
                }
                
                view.LodStackPanel.Children.Add(item);
                if(first)
                    item.OnClick();
                first = false;
            }
        }

        void DisplayTexture(TexureType type, TextBox pathContainer )
        {
            var path = pathContainer.Text;
            var file = PackFileLoadHelper.FindFile(_loadedContent, path);




            //var data = file.Data;


            
          //var img =  SixLabors.ImageSharp.Image.Load(data);
          // var format = SixLabors.ImageSharp.Image.DetectFormat(data);
            using (MemoryStream stream = new MemoryStream(file.Data))
            {
                var image = Pfim.Pfim.FromStream(stream);

                var handle = GCHandle.Alloc(image.Data, GCHandleType.Pinned);
                try
                {
                    var data = Marshal.UnsafeAddrOfPinnedArrayElement(image.Data, 0);
                    var bitmap = new Bitmap(image.Width, image.Height, image.Stride, PixelFormat.Format24bppRgb, data);
                    bitmap.Save(@"c:\temp\myImage.png", System.Drawing.Imaging.ImageFormat.Png);
                }
                finally
                {
                    handle.Free();
                }

                /*ss
                                using (var bitMap = BitmapCodec.Instance.Decode(stream))
                                { 


                                }*/
            }
        }

        void AddUnknownTexture(RigidModelMeshEditorView view, LodModel model)
        {
            foreach (var item in model.Materials)
            {
                var isDefined = Enum.IsDefined(typeof(TexureType), item.TypeRaw);
                if (!isDefined)
                {
                    if (view.DebugInfo.Text.Length != 0)
                        view.DebugInfo.Text += "\n";

                    view.DebugInfo.Text += $"Unknown Texture Type: {item.TypeRaw} - {item.Name}"; 
                }
            }
          
        }

        string GetTexuterName(LodModel model, TexureType type)
        {
            foreach (var material in model.Materials)
            {
                if (material.Type == type)
                    return material.Name;
            }

            return "";
        }

        void CreateLod()
        { }

        RigidModelMeshEditorView CreateMesh(LodModel model)
        {
            var item = new RigidModelMeshEditorView();
            item.Resources.Add("Header", model.modelName + " ");
            return item;
        }
    }
}
