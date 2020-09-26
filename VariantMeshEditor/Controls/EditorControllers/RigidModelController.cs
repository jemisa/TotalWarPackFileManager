using Common;
using Filetypes.RigidModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Controls;
using VariantMeshEditor.ViewModels;
using VariantMeshEditor.Views.EditorViews;
using VariantMeshEditor.Views.EditorViews.Util;
using Viewer.GraphicModels;

namespace VariantMeshEditor.Controls.EditorControllers
{
    public class RigidModelController
    {
        RigidModelEditorView _view;
        RigidModelElement _element;
        List<PackFile> _loadedContent;

        Dictionary<RigidModelMeshEditorView, MeshInstance> _modelEditors = new Dictionary<RigidModelMeshEditorView, MeshInstance>();
         
        public RigidModelController(RigidModelEditorView view, RigidModelElement element, List<PackFile> loadedContent )
        {
            _view = view;
            _element = element;
            _loadedContent = loadedContent;

            PopulateUi(_view, _element);
        }

        public void AssignModel(MeshInstance meshInstance, int lodIndex, int modelIndex)
        {
            var item = _modelEditors.Where(x => x.Key.ModelIndex == modelIndex && x.Key.LodIndex == lodIndex).First();
            _modelEditors[item.Key] = meshInstance;
        }

        public void SetVisible(int lodIndex, bool state)
        {
            foreach (var item in _modelEditors)
            {
                var editor = item.Key;
                var model = item.Value;
                if (editor.LodIndex == lodIndex)
                    model.Visible = state;
                else
                    model.Visible = false;
            }
        }

        private void PopulateUi(RigidModelEditorView view, RigidModelElement element)
        {
            bool firstSub = true;
            bool first = true;
            int currentLodIndex = 0;
            foreach (var lod in element.Model.LodInformations)
            {
                var lodContent = new CollapsableButtonControl($"Lod - {lod.LodLevel}");

                var lodStackPanel = new StackPanel();
                lodContent.Content = lodStackPanel;

                var currentModelIndex = 0;
                foreach (var mesh in lod.LodModels)
                {
                    var meshContnet = new CollapsableButtonControl($"{mesh.modelName}");
                    var meshStackPanel = new StackPanel();
                    meshContnet.Content = meshStackPanel;

                    var meshView = new RigidModelMeshEditorView
                    {
                        ModelIndex = currentModelIndex,
                        LodIndex = currentLodIndex
                    };
                    _modelEditors.Add(meshView, null);

                    meshView.ModelType.Text = mesh.GroupType.ToString();
                    meshView.VisibleCheckBox.Click += (sender, arg) => VisibleCheckBox_Click(meshView);
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
                    AddUnknowData(meshView, mesh);

                    meshStackPanel.Children.Add(meshView);
                    lodStackPanel.Children.Add(meshContnet);

                    if (firstSub)
                        meshContnet.OnClick();
                    firstSub = false;

                    currentModelIndex++;
                }

                currentLodIndex++;


                view.LodStackPanel.Children.Add(lodContent);
                if(first)
                    lodContent.OnClick();
                first = false;
            }
        }

        private void VisibleCheckBox_Click(RigidModelMeshEditorView editorView)
        {
            var model = _modelEditors[editorView];
            model.Visible = editorView.VisibleCheckBox.IsChecked == true;
        }

        void DisplayTexture(TexureType type, TextBox pathContainer )
        {
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

        void AddUnknowData(RigidModelMeshEditorView view, LodModel model)
        { 
        
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
    }
}
