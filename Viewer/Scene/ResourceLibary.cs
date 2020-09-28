﻿using Common;
using Filetypes.RigidModel;
using Microsoft.Xna.Framework.Content;
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
    public enum ShaderTypes
    { 
        Line,
        Mesh,
    }

    public class ResourceLibary
    {
        Dictionary<string, Texture2D> _textureMap = new Dictionary<string, Texture2D>();
        Dictionary<ShaderTypes, Effect> _shaders = new Dictionary<ShaderTypes, Effect>();

        List<PackFile> _loadedContent;
        public ContentManager XnaContentManager { get; set; }


        public ResourceLibary(List<PackFile> loadedContent)
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

        public Effect LoadEffect(string fileName, ShaderTypes type)
        {
            if (_shaders.ContainsKey(type))
                return _shaders[type];
            var effect = XnaContentManager.Load<Effect>(fileName);
            _shaders[type] = effect;
            return effect;
        }

        public Effect GetEffect(ShaderTypes type)
        {
            if (_shaders.ContainsKey(type))
                return _shaders[type];
            return null; 
        }

        public List<PackFile> PackfileContent { get { return _loadedContent; } }
    }
}
