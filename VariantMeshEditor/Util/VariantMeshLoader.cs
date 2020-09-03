using Common;
using Filetypes.ByteParsing;
using Filetypes.RigidModel;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static Filetypes.RigidModel.VariantMeshDefinition;

namespace VariantMeshEditor.Util
{


    public class FileSceneElement
    {
        public List<FileSceneElement> Children { get; set; } = new List<FileSceneElement>();
        public string FileName { get; set; }

        public override string ToString()
        {
            return "Root";
        }

    }



    public class VariantMeshElement : FileSceneElement
    {
        public override string ToString()
        {
            return $"VariantMesh - {FileName}";
        }
    }


    public class SlotElement : FileSceneElement
    {
        public string SlotName { get; set; }
        public string AttachmentPoint { get; set; }

        public override string ToString()
        {
            return $"Slot - {SlotName} - {AttachmentPoint}";
        }
    }

    public class RigidModelElement : FileSceneElement
    {
        public RigidModel Model { get; set; }

        public override string ToString()
        {
            return $"RigidModel - {FileName}";
        }
    }

    public class WsModelElement : FileSceneElement
    {
        public override string ToString()
        {
            return $"WsModel - {FileName}";
        }
    }


    class SceneLoader
    {
        public SceneLoader(List<PackFile> loadedContent)
        {
            _loadedContent = loadedContent;
        }
        List<PackFile> _loadedContent;
        public FileSceneElement Load(string filePath, FileSceneElement parent = null)
        {
            if(parent == null)
                parent = new FileSceneElement();

            var file = FindFile(filePath);
            switch (file.FileExtention)
            {
                case "variantmeshdefinition":
                    LoadVariantMesh(file, parent);
                    break;

                case "rigid_model_v2":
                    LoadRigidMesh(file, parent);
                    break;

                case "wsmodel":
                    LoadVariantMesh(file, parent);
                    break;
            }

            return parent;
        }

        void LoadVariantMesh(PackedFile file, FileSceneElement parent)
        {
            var variantMeshElement = new VariantMeshElement();
            variantMeshElement.FileName = file.FullPath;
            parent.Children.Add(variantMeshElement);

            var content = file.Data;
            var fileContent = Encoding.Default.GetString(content);
            VariantMeshFile meshFile = VariantMeshDefinition.Create(fileContent);

            foreach (var slot in meshFile.VARIANT_MESH.SLOT)
            {
                var slotElement = new SlotElement();
                slotElement.AttachmentPoint = slot.AttachPoint;
                slotElement.SlotName = slot.Name;
                variantMeshElement.Children.Add(slotElement);

                foreach (var mesh in slot.VariantMeshes)
                    Load(mesh.Name, slotElement);

                foreach (var meshReference in slot.VariantMeshReferences)
                    Load(meshReference.definition, slotElement);
            }
        }

        void LoadRigidMesh(PackedFile file, FileSceneElement parent)
        {
            ByteChunk chunk = new ByteChunk(file.Data);
            var model = RigidModel.Create(chunk, out string errorMessage);
            
            

            var rigidModelElement = new RigidModelElement();
            rigidModelElement.Model = model;
            rigidModelElement.FileName = file.FullPath;
            parent.Children.Add(rigidModelElement);
        }


        void LoadWsModel(PackedFile file, FileSceneElement parent)
        {
            var wsModelElement = new WsModelElement();
            wsModelElement.FileName = file.FullPath;
            parent.Children.Add(wsModelElement);
        }

        PackedFile FindFile(string filename)
        {
            filename = filename.ToLower();
            filename = filename.Replace(@"/", @"\");

            var p = Path.GetFullPath(filename);
            foreach (var directory in _loadedContent)
            {
                foreach (var file in directory)
                {
                    if (file.FullPath == filename )
                        return file;
                }
            }
            return null;
        }
    }
}
