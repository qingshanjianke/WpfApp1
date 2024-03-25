using System.Windows;
using System.Windows.Threading;
using CommunityToolkit.Mvvm.DependencyInjection;
using Microsoft.Data;
using Microsoft.Extensions.Logging;

namespace WpfApp1.Common
{
    /// <summary>
    /// 全局异常捕获处理
    /// </summary>
    public class ExceptionHelper
    {
        /*****************************************WeChat****************************************************/
        private static readonly HashSet<Type> PassHashSet;

        //
        // 摘要:
        //     Initializes static members of the X.CommLib.Miscellaneous.ExceptionHelper class.
        static ExceptionHelper()
        {
            PassHashSet = new HashSet<Type>();
            PassHashSet.Add(typeof(OutOfMemoryException));
            PassHashSet.Add(typeof(OperationAbortedException));
            PassHashSet.Add(typeof(ThreadAbortException));
            PassHashSet.Add(typeof(OperationCanceledException));
            PassHashSet.Add(typeof(TaskCanceledException));
        }

        public static void Handle()
        {
            // UI线程未捕获异常处理事件
            Application.Current.DispatcherUnhandledException += new DispatcherUnhandledExceptionEventHandler(App_DispatcherUnhandledException);
            // 非UI线程未捕获异常处理事件
            AppDomain.CurrentDomain.UnhandledException += CurrentDomain_UnhandledException;
            // Task线程内未捕获异常处理事件
            TaskScheduler.UnobservedTaskException += TaskScheduler_UnobservedTaskException;

#if DEBUG
            // 在 xaml 中的绑定错误 
            XamlBindingErrorListener.Instance.OnXamlBindingError += XamlBinding_OnXamlBindingError;
#endif
        }

        /// <summary>
        /// 创建一个可以显示错误提示的异常
        /// </summary>
        /// <param name="exception">The exception.</param>
        /// <param name="message">The message.</param>
        /// <returns></returns>
        public static BizException ConvertToBizException(Exception exception, string? message = null)
        {
            if (exception is BizException bizException)
            {
                return bizException;
            }

            message = string.IsNullOrWhiteSpace(message) ? exception.Message : message;
            return new BizException(message, exception);
        }

        public static void GetExceptionMsg(Exception ex, string errorType, bool isTerminating = false)
        {
            if (ex is BizException)
            {
                //业务上的错误提示
                Application.Current.Dispatcher.BeginInvoke(() =>
                {
                    //DialogHelper.Alert(ex.Message);
                });
            }
            else
            {
                //其他未捕获异常
                var logger = Ioc.Default.GetRequiredService<ILogger<ExceptionHelper>>();
                logger.LogError(ex, errorType + "，" + ex.Message + (isTerminating ? "，程序崩溃" : ""));
                //程序即将崩溃时弹窗提示
                if (isTerminating)
                {
                    Application.Current.Dispatcher.BeginInvoke(() =>
                    {
                        //DialogHelper.Error(ex);
                    });
                }
            }
        }

        //
        // 摘要:
        //     指定一个异常要不要捕获
        //
        // 参数:
        //   exception:
        //     The exception.
        //
        // 返回结果:
        //     The System.Boolean.
        public static bool CannotCatchException(Exception exception)
        {
            if (exception != null)
            {
                return PassHashSet.Contains(exception.GetType());
            }

            return false;
        }

        /// <summary>
        /// xaml 绑定时发生了错误
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The <see cref="XamlBindingErrorListener.XamlBindingErrorEventArgs"/> instance containing the event data.</param>
        private static void XamlBinding_OnXamlBindingError(object? sender, XamlBindingErrorListener.XamlBindingErrorEventArgs e)
        {
            GetExceptionMsg(new Exception(e.Message), "OnXamlBindingError", false);
        }

        private static void App_DispatcherUnhandledException(object sender, DispatcherUnhandledExceptionEventArgs e)
        {
            e.Handled = true;
            GetExceptionMsg(e.Exception, "DispatcherUnhandledException");
        }

        private static void TaskScheduler_UnobservedTaskException(object sender, UnobservedTaskExceptionEventArgs e)
        {
            e.SetObserved();
            GetExceptionMsg(e.Exception, "UnobservedTaskException");
        }

        private static void CurrentDomain_UnhandledException(object sender, UnhandledExceptionEventArgs e)
        {
            GetExceptionMsg((Exception)e.ExceptionObject, "UnhandledException", e.IsTerminating);
        }
    }
}
