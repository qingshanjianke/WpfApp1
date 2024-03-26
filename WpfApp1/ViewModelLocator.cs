using CommunityToolkit.Mvvm.DependencyInjection;

namespace WpfApp1
{
    public class ViewModelLocator
    {
        public static MainWindowViewModel MainWindowViewModel => Ioc.Default.GetRequiredService<MainWindowViewModel>();
    }
}
