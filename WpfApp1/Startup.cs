using System.Diagnostics;
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
using WpfApp1.Database;
using WpfApp1.Models;

namespace WpfApp1
{

    public class Startup
    {
        private readonly List<Assembly> _assemblyList;
        private readonly ApplicationConfig _appConfig;

        public Startup(ApplicationConfig appConfig)
        {
            this._appConfig = appConfig;
            this._assemblyList = Directory.GetFiles(AppDomain.CurrentDomain.BaseDirectory, "RpaClient.*.dll", SearchOption.TopDirectoryOnly)
                .Select(Assembly.LoadFrom)
                .ToList();
        }

        public virtual async Task Run()
        {
            // 加入对 gb2312 的支持
            Encoding.RegisterProvider(CodePagesEncodingProvider.Instance); //注册Nuget包System.Text.Encoding.CodePages中的编码到.NET Core


            //单实例启动
            SingleInstanceHelper.Check(this._appConfig.TitleVersion);

            //全局异常捕获
            ExceptionHelper.Handle();

            //配置
            var host = Host.CreateDefaultBuilder()
                .ConfigureAppConfiguration((context, builder) =>
                {
                    // Add other configuration files...
                    builder.AddJsonFile("appsettings.Development.json", true, true);
                })
                .ConfigureServices((context, services) => { this.ConfigureServices(services); })
                //.UseSerilog()
                .ConfigureLogging(this.ConfigureLogSystem)
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

                await this.PreRunActionAsync(host, logger);

                try
                {
                    // 执行主窗口程序
                    await this.RunMainWindowAsync(host, logger);
                }
                finally
                {
                    await this.PostRunActionAsync(host, logger);
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
        /// <param name="host">The host.</param>
        /// <param name="logger">The logger.</param>
        /// <param name="stopingToken">The stoping token.</param>
        protected virtual async Task PostRunActionAsync(IHost host, ILogger<Startup> logger, CancellationToken stopingToken = default)
        {
            await Task.CompletedTask;
        }

        /// <summary>
        ///     主窗口显示前调用
        /// </summary>
        /// <param name="host">The host.</param>
        /// <param name="logger">The logger.</param>
        /// <param name="stoppingToken">The stopping token.</param>
        protected virtual async Task PreRunActionAsync(IHost host, ILogger<Startup> logger, CancellationToken stoppingToken = default)
        {
            await Task.CompletedTask;
        }

        protected virtual void ConfigureServices(IServiceCollection services)
        {
            //注册通过Export注解的类
            this.ConfigureExportServices(services);

            //services.AddSingleton(this._appConfig.MainWindow);

            //注册其他类
            //services.AddSingleton<IAdbServer>(_ => AdbServer.Instance);
            //services.AddTransient<AdbClient>();
            //services.AddSingleton(this._appConfig);
            //services.AddSingleton<IMemoryCache>(new MemoryCache(new MemoryCacheOptions()));

        }

        /// <summary>
        ///     配置日志系统
        /// </summary>
        /// <param name="builder">The builder.</param>
        /// <param name="logging">The logging.</param>
        private void ConfigureLogSystem(HostBuilderContext builder, ILoggingBuilder logging)
        {
            {
                Log.Logger = new LoggerConfiguration()
                    .ReadFrom.Configuration(builder.Configuration)
                    .Enrich.FromLogContext()
                    .Enrich.WithThreadId()
                    .Enrich.WithSourceContext()
                    .WriteTo.File($"{this._appConfig.LogPath}\\log.log",
                        rollOnFileSizeLimit: true,
                        rollingInterval: RollingInterval.Day,
                        retainedFileCountLimit: 15,
                        outputTemplate: "{Timestamp:yyyy-MM-dd HH:mm:ss.fff zzz} [{Level:u3}] [{ThreadId}] [{SourceContext}] {Message:lj}{NewLine}{Exception}"
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
        /// <param name="host">The host.</param>
        /// <param name="logger">The logger.</param>
        /// <param name="stoppingToken">The stopping token.</param>
        /// <exception cref="System.InvalidOperationException">尚未注册主窗口</exception>
        private async Task RunMainWindowAsync(IHost host, Microsoft.Extensions.Logging.ILogger logger, CancellationToken stoppingToken = default)
        {
            //启动主界面
            /*var mainWindow = (Window)Activator.CreateInstance(_appConfig.MainWindow)!;*/
            if (host.Services.GetService(this._appConfig.MainWindow) is not Window mainWindow)
            {
                throw new InvalidOperationException("尚未注册主窗口");
            }

            //退出时确认框
            mainWindow.Closing += (s, e) =>
            {
                //if (!DialogHelper.Confirm("确定要关闭程序并退出吗？"))
                //{
                //    e.Cancel = true;
                //}
            };

            //退出后的处理
            mainWindow.Closed += (s, e) =>
            {
                logger.LogInformation("应用退出");
                //关闭adb进程，否则更新程序时会提示程序被占用
                var adb = Process.GetProcessesByName("adb").FirstOrDefault();
                if (adb != null)
                {
                    adb.Kill();
                    adb.WaitForExit(1000);
                }
            };

            mainWindow.ShowDialog();

            await Task.CompletedTask;
        }

        private void ConfigureExportServices(IServiceCollection services)
        {
            //获取程序集中需要注入的类
            this._assemblyList.Select(q => q.GetTypes()).SelectMany(q => q).ForEach(type =>
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

                //获取方法注解
                //[Export(Type = DependencyType.Transient)]
                //public AdbClient CreateAdbClient()
                //{
                //    return new AdbClient();
                //}
                foreach (var method in type.GetMethods())
                {
                    var methodAttribute = method.GetCustomAttribute<ExportAttribute>();
                    if (methodAttribute == null)
                    {
                        continue;
                    }

                    if (methodAttribute.Type == DependencyType.Singleton)
                    {
                        services.AddSingleton(method.ReturnType, serviceProvider =>
                        {
                            var entity = serviceProvider.GetService(type);
                            return method.Invoke(entity, null);
                        });
                    }

                    if (methodAttribute.Type == DependencyType.Transient)
                    {
                        services.AddTransient(method.ReturnType, serviceProvider =>
                        {
                            var entity = serviceProvider.GetService(type);
                            return method.Invoke(entity, null);
                        });
                    }

                    if (methodAttribute.Type == DependencyType.Scoped)
                    {
                        services.AddScoped(method.ReturnType, serviceProvider =>
                        {
                            var entity = serviceProvider.GetService(type);
                            return method.Invoke(entity, null);
                        });
                    }
                }
            });
        }
    }
}
