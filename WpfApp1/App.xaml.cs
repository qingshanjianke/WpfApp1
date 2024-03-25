using System.IO;
using System.Reflection;
using System.Windows;
using WpfApp1.Models;
using WpfApp1.Utils;

namespace WpfApp1
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        protected override async void OnStartup(StartupEventArgs e)
        {
            await new Startup(new ApplicationConfig
            {
                Title = "光年KOL挖掘沟通系统(RPA版)",
                Version = Assembly.GetExecutingAssembly().GetName().Version?.ToString(),
                ProductName = Path.GetFileNameWithoutExtension(Assembly.GetExecutingAssembly().Location),
                ServiceName = ApplicationConstants.KOL,
                MainWindow = typeof(MainWindow)
            }).Run();
        }
    }

}
