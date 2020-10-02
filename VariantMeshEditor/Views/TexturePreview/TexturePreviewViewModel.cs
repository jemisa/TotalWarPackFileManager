using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Media;
using VariantMeshEditor.Util;

namespace VariantMeshEditor.Views.TexturePreview
{
    class TexturePreviewViewModel : NotifyPropertyChangedImpl
    {
        ImageSource _image;
        public ImageSource Image
        {
            get { return _image; }
            set 
            { 
                _image = value;
                NotifyPropertyChanged();
            }
        }

        string _imageName;
        public string Name
        {
            get { return _imageName; }
            set
            {
                _imageName = value;
                NotifyPropertyChanged();
            }
        }

        string _imageFormat;
        public string Format
        {
            get { return _imageFormat; }
            set
            {
                _imageFormat = value;
                NotifyPropertyChanged();
            }
        }

        int _width;
        public int Width
        {
            get { return _width; }
            set
            {
                _width = value;
                NotifyPropertyChanged();
            }
        }

        int _height;
        public int Height
        {
            get { return _height; }
            set
            {
                _height = value;
                NotifyPropertyChanged();
            }
        }
    }
}
