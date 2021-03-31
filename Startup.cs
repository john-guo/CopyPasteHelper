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

namespace WpfApp1
{
    public class Startup
    {
        // This method gets called by the runtime. Use this method to add services to the container.
        // For more information on how to configure your application, visit https://go.microsoft.com/fwlink/?LinkID=398940
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddSingleton<MainWindow>();

        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseStaticFiles();
            app.UseRouting();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapPost("/api/copy", Copy);
                endpoints.MapPost("/api/paste", Paste);
            });
        }

        private Task<T> STTask<T>(Func<T> func)
        {
            var dispatcher = Application.Current.Dispatcher;
            return dispatcher.InvokeAsync(func, DispatcherPriority.Normal).Task;
        }

        private void RunAction(Action action)
        {
            var dispatcher = Application.Current.Dispatcher;
            dispatcher.BeginInvoke(action);
        }

        private async Task Copy(HttpContext context)
        {
            byte[] buffer = new byte[context.Request.ContentLength.Value];
            await context.Request.Body.ReadAsync(buffer, 0, buffer.Length);
            var data = Encoding.UTF8.GetString(buffer);
            await STTask(() =>
            {
                Clipboard.SetData(DataFormats.Text, data);

                RunAction(async () =>
                {
                    await (Application.Current.MainWindow as MainWindow).Show("Copy to PC OK!");
                });

                return data;
            });
        }

        private async Task Paste(HttpContext context)
        {
            var data = await STTask(() =>
            {
                var str = string.Empty;
                IDataObject ido = Clipboard.GetDataObject();
                if (ido.GetDataPresent(DataFormats.Text))
                    str = (string)ido.GetData(DataFormats.Text);

                RunAction(async () =>
                {
                    await (Application.Current.MainWindow as MainWindow).Show("Paste from PC OK!");
                });

                return str;
            });

            byte[] buffer = Encoding.UTF8.GetBytes(data);
            context.Response.ContentLength = buffer.Length;
            context.Response.ContentType = "application/json";
            await context.Response.Body.WriteAsync(buffer, 0, buffer.Length);
        }
    }
}
