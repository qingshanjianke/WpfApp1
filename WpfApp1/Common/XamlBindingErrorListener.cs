using System.Diagnostics;

namespace WpfApp1.Common
{
    /// <summary>
    /// xaml 数据绑定错误的捕获工具类
    /// </summary>
    public class XamlBindingErrorListener : TraceListener
    {
        /// <summary>
        /// The listerner lazy
        /// </summary>
        private static readonly Lazy<XamlBindingErrorListener> ListernerLazy = new(() => new XamlBindingErrorListener());


        /// <summary>
        /// Prevents a default instance of the <see cref="XamlBindingErrorListener"/> class from being created.
        /// </summary>
        private XamlBindingErrorListener()
        {
            PresentationTraceSources.DataBindingSource.Listeners.Add(this);
        }

        /// <summary>
        /// 发现错误时的事件
        /// </summary>
        public event EventHandler<XamlBindingErrorEventArgs>? OnXamlBindingError;


        /// <summary>
        /// Gets the instance.
        /// </summary>
        /// <value>
        /// The instance.
        /// </value>
        public static XamlBindingErrorListener Instance => ListernerLazy.Value;


        /// <summary>
        /// Writes the specified message.
        /// </summary>
        /// <param name="message">The message.</param>
        /// <returns></returns>
        public override void Write(string? message)
        {
            WriteErrorMessage(message);
        }

        /// <summary>
        /// When overridden in a derived class, writes a message to the listener you create in the derived class, followed by a line terminator.
        /// </summary>
        /// <param name="message">A message to write.</param>
        public override void WriteLine(string? message)
        {
            WriteErrorMessage(message);
        }

        /// <summary>
        /// Writes the error message.
        /// </summary>
        /// <param name="message">The message.</param>
        private void WriteErrorMessage(string? message)
        {
            OnXamlBindingError?.Invoke(this, new XamlBindingErrorEventArgs { Message = message });
        }


        /// <summary>
        /// 解析 xaml 时发生错误，对应的错误内容 
        /// </summary>
        /// <seealso cref="System.EventArgs" />
        public class XamlBindingErrorEventArgs : EventArgs
        {
            /// <summary>
            /// 对应的错误内容 
            /// </summary>
            /// <value>
            /// The message.
            /// </value>
            public string? Message { get; set; }
        }
    }
}
