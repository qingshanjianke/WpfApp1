using System.IO;
using System.Reflection;
using CommunityToolkit.Mvvm.DependencyInjection;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using WpfApp1.Common;
using WpfApp1.Models;

namespace WpfApp1;

[Export]
public class WebApiService : IAsyncDisposable, IDisposable
{
    private WebApplication? lastWebApplication;
    private readonly ILogger<WebApiService> _logger;
    private readonly IServiceProvider _parentServiceProvider;

    public WebApiService(ILogger<WebApiService> logger, IServiceProvider parentServiceProvider)
    {
        _logger = logger;
        _parentServiceProvider = parentServiceProvider;
    }

    public async Task StartAsync(string[] args, CancellationToken cancellationToken)
    {
        _logger.LogInformation($@"准备启动 web 服务");

        var builder = WebApplication.CreateBuilder(args);

        builder.Services.AddEndpointsApiExplorer();
        builder.Services.AddSwaggerGen();
        builder.Logging.SetMinimumLevel(LogLevel.Trace);
        builder.Logging.ClearProviders();
        builder.Logging.AddProvider(new CustomLoggerProvider());

        builder.Services.AddControllers();
        RegisterService(builder.Services);

        var app = builder.Build();

        if (app.Environment.IsDevelopment())
        {
            app.UseSwagger();
            app.UseSwaggerUI(c =>
            {
                c.SwaggerEndpoint("/swagger/v1/swagger.json", "My API V1");
            });
        }

        app.UseHttpsRedirection();
        app.UseFileServer();

        app.MapControllers();
        
        var lastApp = Interlocked.Exchange(ref lastWebApplication, app);

        await StopCoreAsync(lastApp);

        var url = "http://127.0.0.1:18000";

        var taskFactory = new TaskFactory(TaskCreationOptions.LongRunning, TaskContinuationOptions.LongRunning);
        var task = taskFactory.StartNew(() => RunAsync(app, url, cancellationToken), cancellationToken);

        try
        {
            var appTask = await task;
            await WaitWebAppTaskRunningAsync(app, appTask, cancellationToken);
        }
        catch (Exception ex)
        {
            await app.DisposeAsync();

            if (ex is not ObjectDisposedException)
                throw;
        }
    }

    private void RegisterService(IServiceCollection services)
    {
        try
        {
            var assemblyList = Directory.GetFiles(AppDomain.CurrentDomain.BaseDirectory, "WpfApp1.dll", SearchOption.TopDirectoryOnly)
                .Select(Assembly.LoadFrom)
                .ToList();

            //获取程序集中需要注入的类
            assemblyList.Select(q => q.GetTypes()).SelectMany(q => q).ForEach(type =>
            {
                //获取类注解
                var attribute = type.GetCustomAttribute<ExportAttribute>();
                if (attribute == null || type.IsAbstract || type.IsInterface)
                    return;
                var registerType = attribute.RegisterType ?? type;
                if (attribute.Type == DependencyType.Singleton)
                    services.AddSingleton(registerType, Ioc.Default.GetRequiredService(registerType));
                if (attribute.Type == DependencyType.Scoped)
                    services.AddScoped(registerType, type);
                if (attribute.Type == DependencyType.Transient)
                    services.AddTransient(registerType, type);
            });

            services.AddSingleton(Ioc.Default.GetRequiredService<ApplicationConfig>());
        }
        catch (Exception ex)
        {
            _logger.LogError("{}", ex);
        }
    }

    /// <summary>
    /// 开始停止 
    /// </summary>
    public async Task StopAsync()
    {
        var lastApp = Interlocked.Exchange(ref lastWebApplication, null);

        await StopCoreAsync(lastApp);
    }

    public async ValueTask DisposeAsync()
    {
        await StopAsync();
    }

    /// <summary>
    /// 等待 webapi 应用启动完成
    /// </summary>
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
                _logger.LogInformation("web 服务运行中");
                return;
            }

            var taskStatus = task.Status;

            if (lastStatus != taskStatus)
            {
                lastStatus = taskStatus;
                _logger.LogInformation("web 服务状态 {}", lastStatus);
            }

            if (task.IsCompleted)
            {
                _logger.LogInformation("web 服务中止, {}", taskStatus);

                // 有异常就会直接抛出了
                await task;

                throw new InvalidOperationException($"web 服务提前中止 {taskStatus}");
            }

            await Task.Delay(TimeSpan.FromSeconds(0.5), cancellationToken);
        }

        token.ThrowIfCancellationRequested();
    }

    private async Task<Task> RunAsync(WebApplication app, string url, CancellationToken cancellationToken)
    {
        app.Urls.Add(url);
        var task = app.RunAsync(cancellationToken);

        return await Task.FromResult(task);
    }

    /// <summary>
    /// 占用资源 
    /// </summary>
    private async Task StopCoreAsync(WebApplication? lastApp)
    {
        try
        {
            if (lastApp == null)
            {
                return;
            }

            _logger.LogInformation("释放 web 应用");

            await lastApp.DisposeAsync();

            _logger.LogInformation("释放 web 应用完成");
        }
        catch (Exception exception)
        {
            _logger.LogInformation("释放 web 应用发生错误{}", exception);
        }
    }

    public void Dispose()
    {
        throw new NotImplementedException();
    }
}