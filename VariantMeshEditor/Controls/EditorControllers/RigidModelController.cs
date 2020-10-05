﻿using Common;
using Filetypes.RigidModel;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Pfim;
using SharpDX.DirectWrite;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using VariantMeshEditor.ViewModels;
using VariantMeshEditor.Views.EditorViews;
using VariantMeshEditor.Views.EditorViews.Util;
using VariantMeshEditor.Views.TexturePreview;
using Viewer.GraphicModels;
using Viewer.Scene;
using WpfTest.Scenes;

namespace VariantMeshEditor.Controls.EditorControllers
{
    public class RigidModelController
    {
        RigidModelEditorView _view;
        RigidModelElement _element;
        ResourceLibary _resourceLibary;
        Scene3d _world;
        Dictionary<RigidModelMeshEditorView, MeshRenderItem> _modelEditors = new Dictionary<RigidModelMeshEditorView, MeshRenderItem>();
         
        public RigidModelController(RigidModelEditorView view, RigidModelElement element, ResourceLibary resourceLibary, Scene3d world)
        {
            _view = view;
            _element = element;
            _resourceLibary = resourceLibary;
            _world = world;
            PopulateUi(_view, _element);
        }

        public void AssignModel(MeshRenderItem meshInstance, int lodIndex, int modelIndex)
        {
            var item = _modelEditors.Where(x => x.Key.ModelIndex == modelIndex && x.Key.LodIndex == lodIndex).First();
            _modelEditors[item.Key] = meshInstance;
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

                LodEditorView lodEditorView = new LodEditorView();
                lodEditorView.Scale.Text = $"{lod.Scale}";
                lodEditorView.MeshCount.Text = $"{lod.GroupsCount}";
                lodEditorView.Debug.Text = $"{GetUnknownString(lod.Unknown0)}, {GetUnknownString(lod.Unknown1)}, {GetUnknownString(lod.Unknown2)}";

                lodStackPanel.Children.Add(lodEditorView);
                lodContent.Content = lodStackPanel;

                var currentModelIndex = 0;
                foreach (var mesh in lod.LodModels)
                {
                    var meshContnet = new CollapsableButtonControl($"{mesh.ModelName}");
                    var meshStackPanel = new StackPanel();
                    meshContnet.Content = meshStackPanel;

                    // Create the model

                    var meshView = new RigidModelMeshEditorView
                    {
                        ModelIndex = currentModelIndex,
                        LodIndex = currentLodIndex
                    };
                    _modelEditors.Add(meshView, null);

                    meshView.ModelType.Text = mesh.GroupType.ToString();
                    meshView.VisibleCheckBox.Click += (sender, arg) => VisibleCheckBox_Click(meshView);

                    DisplayTransforms(mesh, meshView);

                    meshView.VertexType.Text = mesh.VertexFormat.ToString();
                    meshView.VertexCount.Text = mesh.VertexCount.ToString();
                    meshView.FaceCount.Text = mesh.FaceCount.ToString();
                    
                    meshView.AlphaMode.Items.Add(mesh.AlphaMode);
                    meshView.AlphaMode.SelectedIndex = 0;

                    foreach (var bone in mesh.Bones)
                        meshView.BoneList.Items.Add(bone.Name);

                    meshView.TextureDir.LabelName.Width = 100;
                    meshView.TextureDir.LabelName.Content = "Texture Directory";
                    meshView.TextureDir.PathTextBox.Text = mesh.TextureDirectory;
                    meshView.TextureDir.RemoveButton.Visibility = System.Windows.Visibility.Hidden;
                    meshView.TextureDir.PreviewButton.Visibility = System.Windows.Visibility.Hidden;

                    CreateTextureDisplayItem(mesh, meshView.Diffuse, TexureType.Diffuse);
                    CreateTextureDisplayItem(mesh, meshView.Specular, TexureType.Specular);
                    CreateTextureDisplayItem(mesh, meshView.Normal, TexureType.Normal);
                    CreateTextureDisplayItem(mesh, meshView.Mask, TexureType.Mask);
                    CreateTextureDisplayItem(mesh, meshView.Gloss, TexureType.Gloss);

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

        void DisplayTransforms(LodModel mesh, RigidModelMeshEditorView view)
        {
            view.PivotView.GroupBox.Header = "Pivot";
            view.PivotView.Row0_0.Text = mesh.Transformation.Pivot.X.ToString();
            view.PivotView.Row0_1.Text = mesh.Transformation.Pivot.Y.ToString();
            view.PivotView.Row0_2.Text = mesh.Transformation.Pivot.Z.ToString();
            DisplayMatrix("Unknown0", view.MatrixView0, mesh.Transformation.Matrices[0]);
            DisplayMatrix("Unknown1", view.MatrixView1, mesh.Transformation.Matrices[1]);
            DisplayMatrix("Unknown2", view.MatrixView2, mesh.Transformation.Matrices[2]);
        }

        void DisplayMatrix(string name, Matrix3x4View view, FileMatrix3x4 matrix)
        {
            view.GroupBox.Header = name;
            view.Row0_0.Text = matrix.Matrix[0].X.ToString();
            view.Row0_1.Text = matrix.Matrix[0].Y.ToString();
            view.Row0_2.Text = matrix.Matrix[0].Z.ToString();
            view.Row0_3.Text = matrix.Matrix[0].W.ToString();

            view.Row1_0.Text = matrix.Matrix[1].X.ToString();
            view.Row1_1.Text = matrix.Matrix[1].Y.ToString();
            view.Row1_2.Text = matrix.Matrix[1].Z.ToString();
            view.Row1_3.Text = matrix.Matrix[1].W.ToString();

            view.Row2_0.Text = matrix.Matrix[2].X.ToString();
            view.Row2_1.Text = matrix.Matrix[2].Y.ToString();
            view.Row2_2.Text = matrix.Matrix[2].Z.ToString();
            view.Row2_3.Text = matrix.Matrix[2].W.ToString();
        }

        void CreateTextureDisplayItem(LodModel mesh, BrowsableItemView view, TexureType textureType)
        {
            view.LabelName.Width = 100;
            view.LabelName.Content = textureType.ToString();
            view.PathTextBox.Text = GetTextureName(mesh, textureType);
            view.CheckBox.Visibility = System.Windows.Visibility.Visible;
            if (view.PathTextBox.Text.Length != 0)
                view.CheckBox.IsChecked = true;

            view.PreviewButton.Click += (sender, file) => DisplayTexture(textureType, view.PathTextBox.Text);
            view.RemoveButton.Click += (sender, file) => DisplayTexture(textureType, view.PathTextBox.Text);
            view.BrowseButton.Click += (sender, file) => DisplayTexture(textureType, view.PathTextBox.Text);
            view.CheckBox.Click += (sender, file) => DisplayTexture(textureType, view.PathTextBox.Text);
        }

        private void VisibleCheckBox_Click(RigidModelMeshEditorView editorView)
        {
            var model = _modelEditors[editorView];
            model.Visible = editorView.VisibleCheckBox.IsChecked == true;
        }


   

        void DisplayTexture(TexureType type, string path)
        {
            TexturePreviewController.Create(path, _world.TextureToTextureRenderer, _resourceLibary);
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
            view.UnkownDataView0.SetData("Unknown 0", model.Unknown0);
            view.UnkownDataView1.SetData("Unknown 1", model.Unknown1);
            view.UnkownDataView2.SetData("Unknown 2", model.Unknown2);
            view.UnkownDataView3.SetData("Unknown 3", model.Unknown3);
            view.UnkownDataView4.SetData("Unknown 4", model.Unknown4);
            view.UnkownDataView5.SetData("Unknown 5", model.Unknown5);
        }

        string GetTextureName(LodModel model, TexureType type)
        {
            foreach (var material in model.Materials)
            {
                if (material.Type == type)
                    return material.Name;
            }

            return "";
        }

        string GetUnknownString(byte[] bytes)
        {
            var bytesAsString = bytes.Select(x => x.ToString());
            return "[" + String.Join(", ", bytesAsString) + "]";
        }
    }
}
