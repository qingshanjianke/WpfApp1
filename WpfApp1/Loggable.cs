using Microsoft.Extensions.Logging;

namespace RpaClient.DouYin.WebServices
{
    public abstract class Loggable
    {
        private readonly ILogger logger;

        protected ILogger Logger => logger;

        protected Loggable(ILogger logger)
        {
            this.logger = logger;
        }

        protected void LogInfo(string message, Exception? exception = null)
        {
            if (exception == null)
            {
                logger.LogInformation(message);
            }
            else
            {
                logger.LogError(exception, message);
            }
        }

        protected void LogDebug(string message, Exception? exception = null)
        {
            logger.LogDebug(exception, message);
        }
    }
}