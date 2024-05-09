using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Extensions.Logging;
using WpfApp1.Common;
using MyRes = WpfApp1.Resource.Language;

namespace WpfApp1
{
    [Export]
    public partial class MainWindowViewModel : ObservableObject
    {
        private readonly WebApiService webServices;
        private readonly ILogger<MainWindowViewModel> logger;



        [RelayCommand]
        private async Task OpenServices()
        {
            this.logger.LogInformation("实打实打算");
            CancellationTokenSource cts = new CancellationTokenSource();

            var args = Environment.GetCommandLineArgs();

            await webServices.StartAsync(args, cts.Token);
        }

        [ObservableProperty]
        public string testName;

        public MainWindowViewModel(ILogger<MainWindowViewModel> logger, WebApiService webServices)
        {
            this.webServices = webServices;
            this.logger = logger;
            TestName = string.Format(MyRes.Open, MyRes.Swich);
        }
    }
}