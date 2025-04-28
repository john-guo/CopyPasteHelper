using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media.Imaging;
using ZXing;
using ZXing.Common;

namespace CopyPasteHelper
{
    public static class BarcodeExt
    {
        public static BitmapImage ToBarcode(this string content, int width, int height)
        {
            var hints = new Dictionary<EncodeHintType, object>
            {
                { EncodeHintType.CHARACTER_SET, "UTF-8" }
            };

            var matrix = new MultiFormatWriter().encode(content, BarcodeFormat.QR_CODE, width, height, hints);

            int w = matrix.Width;
            int h = matrix.Height;
            Bitmap bmp = new Bitmap(w, h);
            for (int x = 0; x < h; x++)
            {
                for (int y = 0; y < w; y++)
                {
                    if (matrix[x, y])
                    {
                        bmp.SetPixel(x, y, System.Drawing.Color.Black);
                    }
                    else
                    {
                        bmp.SetPixel(x, y, System.Drawing.Color.White);
                    }
                }
            }
            var bitmapImage = new BitmapImage();
            using (MemoryStream stream = new MemoryStream())
            {
                bmp.Save(stream, System.Drawing.Imaging.ImageFormat.Png);
                stream.Position = 0;
                bitmapImage.BeginInit();
                bitmapImage.StreamSource = stream;
                bitmapImage.CacheOption = BitmapCacheOption.OnLoad;
                bitmapImage.EndInit();
                bitmapImage.Freeze();
            }
            return bitmapImage;
        }
    }
}
