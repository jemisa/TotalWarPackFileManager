using Filetypes;
using FreeImageAPI;
using System;
using System.Drawing;
using System.IO;
using System.Windows.Forms;

namespace Filetypes.Codecs
{
    public class BitmapCodec : ICodec<Bitmap>
    {
        public static readonly BitmapCodec Instance = new BitmapCodec();

        FREE_IMAGE_FORMAT format = FREE_IMAGE_FORMAT.FIF_BMP;
        public string Format
        {
            set
            {
                switch (value)
                {
                    case "tga":
                        format = FREE_IMAGE_FORMAT.FIF_TARGA;
                        break;
                    case "dds":
                        format = FREE_IMAGE_FORMAT.FIF_DDS;
                        break;
                    case "png":
                        format = FREE_IMAGE_FORMAT.FIF_PNG;
                        break;
                    case "jpg":
                        format = FREE_IMAGE_FORMAT.FIF_JPEG;
                        break;
                    case "bmp":
                        format = FREE_IMAGE_FORMAT.FIF_BMP;
                        break;
                    case "psd":
                        format = FREE_IMAGE_FORMAT.FIF_PSD;
                        break;
                    default:
                        format = FREE_IMAGE_FORMAT.FIF_BMP;
                        break;
                }
            }
        }
        public Bitmap Decode(Stream stream)
        {
            try
            {
                FreeImageBitmap bitmap = new FreeImageBitmap(stream, format);
                bitmap.ConvertType(FREE_IMAGE_TYPE.FIT_BITMAP, true);
                return (Bitmap)bitmap;
            }
            catch (DllNotFoundException)
            {
                MessageBox.Show("Unable to located the FreeImageBitmap dll. Please install it.", "Dll load error");
            }
            catch (BadImageFormatException)
            {
                MessageBox.Show("Unable to located the FreeImageBitmap dll. Probably 32v64 bit mixup", "Dll load error");
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
            return null;
        }

        public void Encode(Stream stream, Bitmap toEncode)
        {
            throw new NotImplementedException();
        }
    }
}
