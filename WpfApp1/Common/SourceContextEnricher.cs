using Serilog;
using Serilog.Configuration;
using Serilog.Core;
using Serilog.Events;

namespace WpfApp1.Common
{
    /// <summary>
    /// 日志输出当前类名扩展
    /// </summary>
    public static class SourceContextEnricherExtensions
    {
        /// <summary>
        /// Withes the source context.
        /// </summary>
        /// <param name="enrichmentConfiguration">The enrichment configuration.</param>
        public static LoggerConfiguration WithSourceContext(this LoggerEnrichmentConfiguration enrichmentConfiguration)
        {
            return enrichmentConfiguration.With<SourceContextEnricher>();
        }
    }

    /// <summary>
    /// 日志输出当前类名 
    /// </summary>
    /// <seealso cref="Serilog.Core.ILogEventEnricher" />
    public class SourceContextEnricher : ILogEventEnricher
    {
        /// <summary>
        /// The log key
        /// </summary>
        public const string LogKey = "SourceContext";

        /// <summary>
        ///     Enrich the log event.
        /// </summary>
        /// <param name="logEvent">The log event to enrich.</param>
        /// <param name="propertyFactory">Factory for creating new properties to add to the event.</param>
        public void Enrich(LogEvent logEvent, ILogEventPropertyFactory propertyFactory)
        {
            /*const string Key = "SourceContext";*/

            if (!logEvent.Properties.TryGetValue(LogKey, out var property))
            {
                return;
            }

            var scalarValue = property as ScalarValue;
            var value = scalarValue?.Value as string;

            if (string.IsNullOrWhiteSpace(value))
            {
                return;
            }

            var lastElement = value.Split(".").LastOrDefault();
            if (!string.IsNullOrWhiteSpace(lastElement))
            {
                logEvent.AddOrUpdateProperty(new LogEventProperty(LogKey, new ScalarValue(lastElement)));
            }
        }
    }
}
