using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Extensions.Logging;
using RpaClient.DouYin.WebServices;
using WpfApp1.Common;

namespace WpfApp1
{
    [Export]
    public partial class MainWindowViewModel : ObservableObject
    {
        private readonly WebApiService webServices;
        private ILogger<MainWindowViewModel> logger;

        public MainWindowViewModel(ILogger<MainWindowViewModel> logger, WebApiService webServices)
        {
            this.logger = logger;
            this.webServices = webServices;
        }

        [RelayCommand]
        private async Task OpenServices()
        {
            this.logger.LogInformation("实打实打算");
            CancellationTokenSource cts = new CancellationTokenSource();

            var args = System.Environment.GetCommandLineArgs();
            var port = 18001;

            await webServices.StartAsync(port, args, cts.Token);
        }
    }
}