using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Threading;
using Newtonsoft.Json;
using System.Drawing;
using System.Text.RegularExpressions;
using System.IO;
using Microsoft.AspNetCore.Http.Features;
using System.Windows.Interop;
using System.Windows.Media.Imaging;
using Microsoft.Extensions.FileProviders;

namespace WpfApp1
{
    public class Startup
    {
        public static readonly string UploadPath;
        static Startup()
        {
            UploadPath = Path.Combine(Directory.GetCurrentDirectory(), @"wwwroot\files");
            if (!Directory.Exists(UploadPath))
            {
                Directory.CreateDirectory(UploadPath);
            }
        }

        // This method gets called by the runtime. Use this method to add services to the container.
        // For more information on how to configure your application, visit https://go.microsoft.com/fwlink/?LinkID=398940
        public void ConfigureServices(IServiceCollection services)
        {
            services.Configure<FormOptions>(options =>
            {
                // Set the limit to 500 MB
                options.MultipartBodyLengthLimit = 524288000;
            });

            services.AddSingleton<MainWindow>();
            //services.AddControllers();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            app.UseStaticFiles();
            app.UseDirectoryBrowser(new DirectoryBrowserOptions()
            {
                FileProvider = new PhysicalFileProvider(UploadPath),
                RequestPath = new PathString("/files")
            });
            app.UseRouting();
            app.UseEndpoints(endpoints =>
            {
                //endpoints.MapControllers();
                endpoints.MapPost("/api/copy", Copy);
                endpoints.MapPost("/api/copy2", Copy2);
                endpoints.MapPost("/api/save", Save);
                endpoints.MapPost("/api/paste", Paste);
            });
        }

        internal static Task<T> STTask<T>(Func<T> func)
        {
            var dispatcher = Application.Current.Dispatcher;
            return dispatcher.InvokeAsync(func, DispatcherPriority.Normal).Task;
        }

        internal static void RunAction(Action action)
        {
            var dispatcher = Application.Current.Dispatcher;
            dispatcher.BeginInvoke(action);
        }

        public class RequestParameter
        {
            [JsonProperty("type")]
            public int Type { get; set; }

            [JsonProperty("data")]
            public string Data { get; set; }

            private static Regex regex = new Regex(@"data:(?<mime>[\w/\-\.]+);(?<encoding>\w+),(?<data>.*)", RegexOptions.Compiled);
            public Bitmap GetImage()
            {
                if (Type == 0)
                    return null;

                var match = regex.Match(Data);

                var mime = match.Groups["mime"].Value;
                var encoding = match.Groups["encoding"].Value;
                var data = match.Groups["data"].Value;

                using (var stream = new MemoryStream(Convert.FromBase64String(data)))
                {
                    return new Bitmap(stream);
                }
            }

            public void PutImage(InteropBitmap image)
            {
                using (var stream = new MemoryStream())
                {
                    BitmapEncoder encoder = new PngBitmapEncoder();
                    encoder.Frames.Add(BitmapFrame.Create(image));
                    encoder.Save(stream);
                    var data = stream.ToArray();
                    Data = $"data:image/png;base64,{Convert.ToBase64String(data)}";
                }
            }
        }

        private async Task Copy(HttpContext context)
        {
            byte[] buffer = new byte[context.Request.ContentLength.Value];
            await context.Request.Body.ReadAsync(buffer, 0, buffer.Length);
            var data = Encoding.UTF8.GetString(buffer);
            var parameters = JsonConvert.DeserializeObject<RequestParameter>(data);

            await STTask(() =>
            {
                var result = "";
                switch (parameters.Type)
                {
                    case 0://text
                        Clipboard.SetData(DataFormats.UnicodeText, parameters.Data);
                        result = "Text";
                        break;
                    case 1://image
                        
                        using (var image = parameters.GetImage())
                        {
                            Clipboard.SetData(DataFormats.Bitmap, image);
                        }
                        result = "Image";
                        break;
                }


                RunAction(async () =>
                {
                    await (Application.Current.MainWindow as MainWindow).Show($"{result} Copy To PC OK!");
                });

                return data;
            });
        }

        private async Task Copy2(HttpContext context)
        {
            var form = await context.Request.ReadFormAsync();
            var file = form.Files[0];
            var image = new Bitmap(file.OpenReadStream());

            await STTask(() =>
            {
                Clipboard.SetData(DataFormats.Bitmap, image);

                RunAction(async () =>
                {
                    await (Application.Current.MainWindow as MainWindow).Show("Image Copy To PC OK!");
                });

                image.Dispose();
                return 0;
            });
        }


        private async Task Save(HttpContext context)
        {
            var form = await context.Request.ReadFormAsync();
            var file = form.Files[0];
            var filename = Path.Combine(UploadPath, file.FileName);
            using (var stream = file.OpenReadStream())
            using (var fs = new FileStream(filename, FileMode.Create))
            {
                byte[] buffer = new byte[file.Length];
                int n = 0;
                do
                {
                    n = await stream.ReadAsync(buffer, 0, buffer.Length);
                    if (n > 0)
                    {
                        await fs.WriteAsync(buffer, 0, n);
                    }
                } while (n != 0);
            }

            RunAction(async () =>
            {
                await (Application.Current.MainWindow as MainWindow).Show("Upload To PC OK!");
            });
        }

        private async Task Paste(HttpContext context)
        {
            var data = await STTask(() =>
            {
                RequestParameter parameter = new RequestParameter();
                IDataObject ido = Clipboard.GetDataObject();

                string result = "";
                if (ido.GetDataPresent(DataFormats.Bitmap))
                {
                    result = "Image";
                    parameter.Type = 1;
                    parameter.PutImage((InteropBitmap)ido.GetData(DataFormats.Bitmap));
                }
                else if (ido.GetDataPresent(DataFormats.Text))
                {
                    result = "Text";
                    parameter.Type = 0;
                    parameter.Data = (string)ido.GetData(DataFormats.UnicodeText);
                }

                RunAction(async () =>
                {
                    await (Application.Current.MainWindow as MainWindow).Show($"{result} Paste From PC OK!");
                });

                return parameter;
            });

            byte[] buffer = Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(data));
            context.Response.ContentLength = buffer.Length;
            context.Response.ContentType = "application/json";
            await context.Response.Body.WriteAsync(buffer, 0, buffer.Length);
        }
    }
}
