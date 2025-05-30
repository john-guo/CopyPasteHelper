using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Net;
using System.Net.NetworkInformation;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;

namespace WpfApp1
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        private IHost _host;
        //public static string Address;
        //public static string[] Addresses { get; set; }

        public App()
        {
            _host = Host.CreateDefaultBuilder()
                .ConfigureWebHostDefaults(webBuilder =>
                {
                    webBuilder.ConfigureKestrel((context, options) =>
                    {
                        options.ListenAnyIP(6688); // Listen on port 6688 for any IP address
                        // Handle requests up to 500 MB
                        options.Limits.MaxRequestBodySize = 524288000;
                    });
                    webBuilder.UseKestrel();

                    //Addresses = AllUpExternalIPv4Addresses().Select(ip => $"http://{ip}:6688/").ToArray();
                    //Address = Addresses[0];
                    //webBuilder.UseUrls(Addresses);
                    webBuilder.UseStartup<Startup>();
                }).Build();
        }

        private async void Application_Startup(object sender, StartupEventArgs e)
        {
            await _host.StartAsync();

            var mainWindow = _host.Services.GetService<MainWindow>();
            mainWindow.Show();
        }

        private void Application_Exit(object sender, ExitEventArgs e)
        {
            _host.Dispose();
            //using (_host)
            //{
            //    _host.StopAsync();
            //}
        }
    }
}
