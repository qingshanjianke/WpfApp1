using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Extensions.Logging;
using WpfApp1.Common;

namespace WpfApp1
{
    [Export]
    public partial class MainWindowViewModel(ILogger<MainWindowViewModel> logger, WebApiService webServices) : ObservableObject
    {
        private readonly WebApiService webServices = webServices;
        private readonly ILogger<MainWindowViewModel> logger = logger;

        [RelayCommand]
        private async Task OpenServices()
        {
            this.logger.LogInformation("实打实打算");
            CancellationTokenSource cts = new CancellationTokenSource();

            var args = Environment.GetCommandLineArgs();

            await webServices.StartAsync( args, cts.Token);
        }
    }
}