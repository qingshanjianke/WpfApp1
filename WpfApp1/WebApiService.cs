using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Reflection;
using CommunityToolkit.Mvvm.DependencyInjection;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.VisualBasic.Logging;
using RpaClient.DouYin.WebServices;
using WpfApp1.Common;
using WpfApp1.Models;

namespace WpfApp1;

/// <summary>
/// web 服务管理器
/// </summary>
/// <seealso cref="Loggable" />
/// <seealso cref="IAsyncDisposable" />
[Export]
public class WebApiService : Loggable, IAsyncDisposable, IDisposable
{

    private WebApplication? lastWebApplication;

    public WebApiService(ILogger<WebApiService> logger) : base(logger) { }

    public int Port { get; private set; }

    public async Task StartAsync(int port, string[] args, CancellationToken cancellationToken)
    {
        this.LogInfo($@"准备启动 web 服务参数，端口：{port}");

        var builder = WebApplication.CreateBuilder(args);

        builder.Services.AddEndpointsApiExplorer();

        builder.Logging.SetMinimumLevel(LogLevel.Trace);
        builder.Logging.ClearProviders();
        builder.Logging.AddProvider(new CustomLoggerProvider());

        builder.Services.AddControllers();
        RegisterService(builder.Services);

        var app = builder.Build();

        app.MapControllers();
        app.MapGet("/test", () => $@"test {DateTime.Now}");
        var lastApp = Interlocked.Exchange(ref this.lastWebApplication, app);

        await this.StopCoreAsync(lastApp);

        var url = $@"http://127.0.0.1:{port}";

        var taskFactory = new TaskFactory(TaskCreationOptions.LongRunning, TaskContinuationOptions.LongRunning);
        var task = taskFactory.StartNew(() => this.RunAsync(app, url, cancellationToken), cancellationToken);

        try
        {
            var appTask = await task;

            await this.WaitWebAppTaskRunningAsync(app, appTask, cancellationToken);

            this.Port = port;
        }
        catch (Exception ex)
        {
            await app.DisposeAsync();

            if (ex is not System.ObjectDisposedException)
                throw;
        }
    }

    private void RegisterService(IServiceCollection services)
    {
        try
        {
            var assemblyList = Directory.GetFiles(AppDomain.CurrentDomain.BaseDirectory, "WpfApp1.dll", SearchOption.TopDirectoryOnly).Select(Assembly.LoadFrom).ToList();
            //获取程序集中需要注入的类
            assemblyList.Select(q => q.GetTypes()).SelectMany(q => q).ForEach(type =>
            {
                //获取类注解
                var attribute = type.GetCustomAttribute<ExportAttribute>();
                if (attribute == null || type.IsAbstract || type.IsInterface)
                    return;
                var registerType = attribute.RegisterType ?? type;
                if (attribute.Type == DependencyType.Singleton)
                    services.AddSingleton(registerType, Ioc.Default.GetService(registerType));
                if (attribute.Type == DependencyType.Scoped)
                    services.AddScoped(registerType, type);
                if (attribute.Type == DependencyType.Transient)
                    services.AddTransient(registerType, type);
            });

            services.AddSingleton(Ioc.Default.GetService<ApplicationConfig>());
        }
        catch (Exception ex)
        {
            Logger.LogError(ex.ToString());
        }

    }

    /// <summary>
    /// 开始停止 
    /// </summary>
    public async Task StopAsync()
    {
        var lastApp = Interlocked.Exchange(ref this.lastWebApplication, null);

        await this.StopCoreAsync(lastApp);
    }

    /// <summary>
    /// Gets the recommand port.
    /// </summary>
    public int GetRecommandPort()
    {
#if DEBUG
        // 测试时，将本地端口也测试为 2601
        var port = 2601;
        this.LogDebug($@"设置模式下，本地 api 端口被修改为：{port}");

        return port;
#endif
        return GetAvailablePort();
    }

    public async ValueTask DisposeAsync()
    {
        await this.StopAsync();
    }

    public void Dispose()
    {
        throw new NotImplementedException();
    }

    /// <summary>
    /// 返回一个当前未被使用的端口
    /// </summary>
    /// <returns></returns>
    private static int GetAvailablePort()
    {
        using var socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
        socket.Bind(new IPEndPoint(IPAddress.Loopback, 0));

        if (socket.LocalEndPoint is IPEndPoint ipEndPoint)
        {
            return ipEndPoint.Port;
        }

        throw new InvalidOperationException($@"无法获取当前可用端口，类型 {socket.LocalEndPoint?.GetType()}");
    }

    /// <summary>
    /// 等待 webapi 应用启动完成
    /// </summary>
    /// <param name="app">The application.</param>
    /// <param name="task">The task.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <exception cref="System.InvalidOperationException">@"web 服务提前中止 {taskStatus}")</exception>
    private async Task WaitWebAppTaskRunningAsync(WebApplication app, Task task, CancellationToken cancellationToken)
    {
        using var timeoutTokenSource = new CancellationTokenSource(TimeSpan.FromSeconds(5));
        using var cancellationTokenSource = CancellationTokenSource.CreateLinkedTokenSource(timeoutTokenSource.Token, cancellationToken);


        var appIsStarted = false;
        app.Lifetime.ApplicationStarted.Register(() => appIsStarted = true);
        TaskStatus? lastStatus = null;

        var token = cancellationTokenSource.Token;
        while (!token.IsCancellationRequested)
        {
            if (appIsStarted)
            {
                this.LogInfo("web 服务运行中");
                return;
            }

            var taskStatus = task.Status;

            if (lastStatus != taskStatus)
            {
                lastStatus = taskStatus;
                this.LogInfo($@"web 服务状态 {lastStatus}");
            }

            if (task.IsCompleted)
            {
                this.LogInfo($"web 服务中止, {taskStatus}");

                // 有异常就会直接抛出了
                await task;

                throw new InvalidOperationException($@"web 服务提前中止 {taskStatus}");
            }

            await Task.Delay(TimeSpan.FromSeconds(0.5), cancellationToken);
        }

        token.ThrowIfCancellationRequested();
    }

    /// <summary>
    /// Runs the asynchronous.
    /// </summary>
    /// <param name="app">The application.</param>
    /// <param name="url">The URL.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns></returns>
    private async Task<Task> RunAsync(WebApplication app, string url, CancellationToken cancellationToken)
    {
        app.Urls.Add(url);
        var task = app.RunAsync(cancellationToken);

        return await Task.FromResult(task);
    }

    /// <summary>
    /// 占用资源 
    /// </summary>
    /// <param name="lastApp">The last application.</param>
    private async Task StopCoreAsync(WebApplication? lastApp)
    {
        try
        {
            if (lastApp == null)
            {
                return;
            }

            this.LogInfo("释放 web 应用");

            await lastApp.DisposeAsync();

            this.LogInfo("释放 web 应用完成");
        }
        catch (Exception exception)
        {
            this.LogInfo("释放 web 应用发生错误", exception);
        }
    }
}