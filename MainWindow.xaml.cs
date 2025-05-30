using CopyPasteHelper;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Net.NetworkInformation;
using System.Net;
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
using System.Windows.Threading;

namespace WpfApp1
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private DispatcherTimer timer;
        private string[] Addresses;

        public MainWindow()
        {
            InitializeComponent();
            timer = new DispatcherTimer()
            {
                Interval = TimeSpan.FromSeconds(1),
            };
            timer.Tick += Timer_Tick;
        }

        private void Timer_Tick(object sender, EventArgs e)
        {
            if (Addresses == null || Addresses.Length == 0)
            {
                Addresses = AllUpExternalIPv4Addresses().Select(ip => $"http://{ip}:6688/").ToArray();
                if (Addresses.Length > 0)
                {
                    addresses.ItemsSource = Addresses;
                    addresses.SelectedIndex = 0;
                }
                return;
            }

            var currentAddresses = AllUpExternalIPv4Addresses().Select(ip => $"http://{ip}:6688/").ToArray();
            if (Addresses.SequenceEqual(currentAddresses))
            {
                return; // No change in addresses
            }
            Addresses = currentAddresses;
            addresses.ItemsSource = Addresses;
            addresses.SelectedIndex = 0;
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            Timer_Tick(null, null);
            timer.IsEnabled = true;
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
            
        public void Show(string message)
        {
            myNotifyIcon.ShowNotification("Info", message, H.NotifyIcon.Core.NotificationIcon.Info);
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
        private IPAddress[] AllUpExternalIPv4Addresses()
        {
            return NetworkInterface.GetAllNetworkInterfaces()
                .Where(n => n.NetworkInterfaceType != NetworkInterfaceType.Loopback && n.OperationalStatus == OperationalStatus.Up)
                .SelectMany(n => n.GetIPProperties().UnicastAddresses.Select(u => u.Address))
                .Where(addr => addr.AddressFamily == System.Net.Sockets.AddressFamily.InterNetwork)
                .ToArray();
        }
        private void ComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var content = addresses.SelectedItem + "index.html";
            img.Stretch = Stretch.Fill;
            img.Source = content.ToBarcode(200, 200);

            imgToolTip.Source = img.Source;
        }

        private WifiWindow _wifi = null;
        private void wifi_Click(object sender, RoutedEventArgs e)
        {
            if (_wifi == null || !_wifi.IsVisible)
            {
                _wifi = new WifiWindow();
                _wifi.Owner = this;
                _wifi.Show();
            }
            else
            {
                _wifi.Activate();
            }
        }

        private void hotspot_Click(object sender, RoutedEventArgs e)
        {
            Process.Start(new ProcessStartInfo("ms-settings:network-mobilehotspot")
            {
                UseShellExecute = true,
                Verb = "open",
            });
        }
    }
}
