using CommunityToolkit.Mvvm.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace WpfApp1.Common
{
    public class CustomLoggerProvider : ILoggerProvider
    {
        public Microsoft.Extensions.Logging.ILogger CreateLogger(string categoryName)
        {
            return new CustomLogger(categoryName, "This is prefix: ");
        }

        public void Dispose()
        {
        }
    }

    public class CustomLogger : Microsoft.Extensions.Logging.ILogger
    {
        private readonly string CategoryName;

        public CustomLogger(string categoryName, string logPrefix)
        {
            CategoryName = categoryName;
        }

        public IDisposable BeginScope<TState>(TState state)
        {
            return new NoopDisposable();
        }

        public bool IsEnabled(LogLevel logLevel)
        {
            return true;
        }

        public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception exception, Func<TState, Exception, string> formatter)
        {
            var message = formatter(state, exception);
            Serilog.Events.LogEventLevel level = Serilog.Events.LogEventLevel.Debug;
            switch (logLevel)
            {
                case LogLevel.Trace:
                    level = Serilog.Events.LogEventLevel.Verbose;
                    break;
                case LogLevel.Debug:
                    level = Serilog.Events.LogEventLevel.Debug;
                    break;
                case LogLevel.Information:
                    level = Serilog.Events.LogEventLevel.Information;
                    break;
                case LogLevel.Warning:
                    level = Serilog.Events.LogEventLevel.Warning;
                    break;
                case LogLevel.Error:
                    level = Serilog.Events.LogEventLevel.Error;
                    break;
                case LogLevel.Critical:
                    break;
                case LogLevel.None:
                    break;
                default:
                    break;
            }

            // Serilog.Log.Logger.Write(level, exception, message);
            var logger = Serilog.Log.Logger.ForContext(SourceContextEnricher.LogKey, this.CategoryName);
            logger.Write(level, exception, message);


            //NLog.LogManager.GetLogger(CategoryName).Log(new NLog.LogEventInfo()
            //{
            //    Exception = exception,
            //    LoggerName = CategoryName,
            //    Message = message,
            //    Level = level
            //});


            //var logViewModel = Ioc.Default.GetRequiredService<LogViewModel>();
            //logViewModel.AddLog(logLevel, CategoryName, message);
        }

        private class NoopDisposable : IDisposable
        {
            public void Dispose()
            {
            }
        }
    }
}
