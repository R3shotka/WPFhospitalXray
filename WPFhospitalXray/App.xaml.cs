using System;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.AspNetCore.Identity;

using DAL.DBContext;
using DAL.Entity;
using WPFhospitalXray;

namespace WPFhospitalXray
{
    public partial class App : Application
    {
        private readonly IHost _host;

        public App()
        {
            _host = Host.CreateDefaultBuilder()
                .ConfigureServices((ctx, services) =>
                {
                    services.AddUiServices(); // <- твій ConfigureUIService
                })
                .Build();
        }

        protected override async void OnStartup(StartupEventArgs e)
        {
            try
            {
                await _host.StartAsync();

                var init = _host.Services.GetRequiredService<IInitializerService>();
                await init.InitializeAsync();

                var main = _host.Services.GetRequiredService<MainWindow>();
                MainWindow = main;
                main.Show();

                base.OnStartup(e);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString(), "Startup error");
                Shutdown();
            }
        }

        protected override async void OnExit(ExitEventArgs e)
        {
            await _host.StopAsync();
            _host.Dispose();
            base.OnExit(e);
        }
    }
}