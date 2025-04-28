using System;
using System.Collections.Generic;
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
using System.Windows.Shapes;

namespace CopyPasteHelper
{
    /// <summary>
    /// Interaction logic for WifiWindow.xaml
    /// </summary>
    public partial class WifiWindow : Window
    {
        public WifiWindow()
        {
            InitializeComponent();
        }

        private void ok_Click(object sender, RoutedEventArgs e)
        {
            var sid = tb_sid.Text;
            var password = tb_passwd.Text;
            var type = cb_type.Text;

            var str = $"WIFI:S:{sid};T:{type};P:{password};;";
            if (type == "nopass")
            {
                str = $"WIFI:S:{sid};T:{type};;";
            }
            img.Stretch = Stretch.Fill;
            img.Source = str.ToBarcode(200, 200);
        }
    }
}
