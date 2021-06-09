using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.IO;
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
using ZXing;
using ZXing.Common;

namespace WpfApp1
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            var content = App.Address + "index.html";
            Dictionary<EncodeHintType, object> hints = new Dictionary<EncodeHintType, object>();
            hints.Add(EncodeHintType.CHARACTER_SET, "UTF-8");

            var bitMatrix = new MultiFormatWriter().encode(content, BarcodeFormat.QR_CODE, 200, 200);
            this.img.Stretch = Stretch.Fill;
            this.img.Source = toImage(bitMatrix);

            this.imgToolTip.Source = this.img.Source;
        }

        private BitmapImage toImage(BitMatrix matrix)
        {
            try
            {
                int width = matrix.Width;
                int height = matrix.Height;

                Bitmap bmp = new Bitmap(width, height);
                for (int x = 0; x < height; x++)
                {
                    for (int y = 0; y < width; y++)
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

                return ConvertBitmapToBitmapImage(bmp);
            }
            catch (Exception ex)
            {
                throw ex;
            }

        }
        private static BitmapImage ConvertBitmapToBitmapImage(Bitmap wbm)
        {
            BitmapImage bimg = new BitmapImage();

            using (MemoryStream stream = new MemoryStream())
            {
                wbm.Save(stream, System.Drawing.Imaging.ImageFormat.Png);
                bimg.BeginInit();
                stream.Seek(0, SeekOrigin.Begin);
                bimg.StreamSource = stream;
                bimg.CacheOption = BitmapCacheOption.OnLoad;
                bimg.EndInit();
            }
            return bimg;
        }
            
        public async Task Show(string message)
        {
            myNotifyIcon.ShowBalloonTip("Info", message, Hardcodet.Wpf.TaskbarNotification.BalloonIcon.Info);
            await Task.Delay(2000);
            myNotifyIcon.HideBalloonTip();
        }

        private void CommandBinding_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            Process.Start(new ProcessStartInfo()
            {
                FileName = Startup.UploadPath,
                UseShellExecute = true,
                Verb = "open",
            });
        }
    }
}
