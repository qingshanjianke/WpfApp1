using System.IO;
using System.Reflection;
using System.Text;
using System.Windows;
using CommunityToolkit.Mvvm.DependencyInjection;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Serilog;
using WpfApp1.Common;
using WpfApp1.Models;

namespace WpfApp1
{

    public class Startup
    {
        private readonly List<Assembly> _assemblyList;
        private readonly ApplicationConfig _appConfig;

        public Startup(ApplicationConfig appConfig)
        {
            _appConfig = appConfig;
            _assemblyList = Directory.GetFiles(AppDomain.CurrentDomain.BaseDirectory, "WpfApp1.dll", SearchOption.TopDirectoryOnly)
                .Select(Assembly.LoadFrom)
                .ToList();
        }

        public virtual async Task Run()
        {
            // 加入对 gb2312 的支持
            //注册Nuget包System.Text.Encoding.CodePages中的编码到.NET Core
            Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);

            //单实例启动
            SingleInstanceHelper.Check(_appConfig.TitleVersion);

            //全局异常捕获
            ExceptionHelper.Handle();

            //配置
            var host = Host.CreateDefaultBuilder()
                .ConfigureAppConfiguration((context, builder) =>
                {
                    builder.AddJsonFile("appsettings.Development.json", true, true);
                })
                .ConfigureServices((context, services) => { ConfigureServices(services); })
                .ConfigureLogging(ConfigureLogSystem)
                .Build();


            Ioc.Default.ConfigureServices(host.Services);

            var logger = host.Services.GetRequiredService<ILogger<Startup>>();

            try
            {
                logger.LogInformation("应用启动");

                //检查更新
                //await host.Services.GetRequiredService<UpdateWindowViewModel>().Check();

                //同步数据库结构
                //host.Services.GetRequiredService<DbContext>().InitTables(this._assemblyList);

                //登录界面
                //await host.Services.GetRequiredService<LoginWindowViewModel>().Login();

                await PreRunActionAsync(host, logger);

                try
                {
                    // 执行主窗口程序
                    await RunMainWindowAsync(host, logger);
                }
                finally
                {
                    await PostRunActionAsync(host, logger);
                }

                //不加这个退出后进程不会退出
                Environment.Exit(0);
            }
            catch (Exception exception)
            {
                logger.LogError(exception, "程序执行过程中发生错误");

                Environment.Exit(1);
            }
        }


        /// <summary>
        ///     主窗口关闭后调用
        /// </summary>
        protected virtual async Task PostRunActionAsync(IHost host, ILogger<Startup> logger, CancellationToken stopingToken = default)
        {
            await Task.CompletedTask;
        }

        /// <summary>
        ///     主窗口显示前调用
        /// </summary>
        protected virtual async Task PreRunActionAsync(IHost host, ILogger<Startup> logger, CancellationToken stoppingToken = default)
        {
            await Task.CompletedTask;
        }

        protected virtual void ConfigureServices(IServiceCollection services)
        {
            //注册通过Export注解的类
            ConfigureExportServices(services);

            services.AddSingleton(_appConfig.MainWindow);

            //注册其他类
            services.AddSingleton(_appConfig);
        }

        /// <summary>
        ///     配置日志系统
        /// </summary>
        private void ConfigureLogSystem(HostBuilderContext builder, ILoggingBuilder logging)
        {
            {
                Log.Logger = new LoggerConfiguration()
                    .ReadFrom.Configuration(builder.Configuration)
                    .Enrich.FromLogContext()
                    .Enrich.WithThreadId()
                    .Enrich.WithSourceContext()
                    .WriteTo.File($"{_appConfig.LogPath}\\log.log",
                        rollOnFileSizeLimit: true,
                        rollingInterval: RollingInterval.Day,
                        retainedFileCountLimit: 15,
                        outputTemplate: "{Timestamp:yyyy-MM-dd HH:mm:ss.fff} [{Level:u3}] [{ThreadId}] [{SourceContext}] {Message:lj}{NewLine}{Exception}"
                    )
                    .CreateLogger();


                logging.SetMinimumLevel(LogLevel.Trace);
                logging.ClearProviders();
                logging.AddProvider(new CustomLoggerProvider());
            }
        }

        /// <summary>
        ///     执行主窗口
        /// </summary>
        private async Task RunMainWindowAsync(IHost host, Microsoft.Extensions.Logging.ILogger logger, CancellationToken stoppingToken = default)
        {
            //启动主界面
            if (host.Services.GetService(_appConfig.MainWindow) is Window mainWindow)
            {
                //退出时确认框
                mainWindow.Closing += (s, e) => { };

                //退出后的处理
                mainWindow.Closed += (s, e) => logger.LogInformation("应用退出");

                mainWindow.ShowDialog();
            }
            else
            {
                throw new InvalidOperationException("尚未注册主窗口");
            }

            await Task.CompletedTask;
        }

        private void ConfigureExportServices(IServiceCollection services)
        {
            //获取程序集中需要注入的类
            _assemblyList.Select(q => q.GetTypes()).SelectMany(q => q).ForEach(type =>
            {
                //获取类注解
                var attribute = type.GetCustomAttribute<ExportAttribute>();
                if (attribute == null || type.IsAbstract || type.IsInterface)
                {
                    return;
                }

                var registerType = attribute.RegisterType ?? type;
                if (attribute.Type == DependencyType.Singleton)
                {
                    services.AddSingleton(registerType, type);
                }

                if (attribute.Type == DependencyType.Scoped)
                {
                    services.AddScoped(registerType, type);
                }

                if (attribute.Type == DependencyType.Transient)
                {
                    services.AddTransient(registerType, type);
                }
            });
        }
    }
}
