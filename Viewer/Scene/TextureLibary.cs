using Common;
using Filetypes.RigidModel;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace Viewer.Scene
{
    public class TextureLibary
    {
        Dictionary<string, Texture2D> _textureMap = new Dictionary<string, Texture2D>();
        List<PackFile> _loadedContent;



        public TextureLibary(List<PackFile> loadedContent)
        {
            _loadedContent = loadedContent;
        }

        public Texture2D LoadTexture(string fileName, GraphicsDevice device)
        {
            if (_textureMap.ContainsKey(fileName))
                return _textureMap[fileName];

            var texture = LoadTextureAsTexture2d(fileName, device);
            if(texture != null)
                _textureMap[fileName] = texture;
            return texture;
        }

        public void SaveTexture(Texture2D texture, string path)
        {
            using (FileStream stream = new FileStream(path, FileMode.OpenOrCreate))
            {
                texture.SaveAsPng(stream, texture.Width, texture.Height);
            }
        }

        Texture2D LoadTextureAsTexture2d(string fileName, GraphicsDevice device)
        {
            var file = PackFileLoadHelper.FindFile(_loadedContent, fileName);
            if (file == null)
                return null;

            var content = file.Data;
            using (MemoryStream stream = new MemoryStream(content))
            {
                var image = Pfim.Dds.Create(stream, new Pfim.PfimConfig(32768, Pfim.TargetFormat.Native, false));
                if (image as Pfim.Dxt1Dds != null)
                {
                    var texture = new Texture2D(device, image.Width, image.Height, false, SurfaceFormat.Dxt1);
                    texture.SetData(image.Data, 0, (int)image.Header.PitchOrLinearSize);
                    return texture;
                }
                else if (image as Pfim.Dxt5Dds != null)
                {
                    var texture = new Texture2D(device, image.Width, image.Height, false, SurfaceFormat.Dxt5);
                    texture.SetData(image.Data, 0, (int)image.Header.PitchOrLinearSize);
                    return texture;
                }
                else
                {
                    throw new Exception("Unknow texture format: " + image.ToString());
                }
            }
        }


    }
}
