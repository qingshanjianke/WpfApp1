namespace WpfApp1.Common
{
    public class ExportAttribute : Attribute
    {
        /// <summary>
        /// 注册实例的生命周期
        /// </summary>
        public DependencyType Type;
        /// <summary>
        /// 注册的类型
        /// </summary>
        public Type? RegisterType;
    }

    public enum DependencyType
    {
        /// <summary>
        /// 全局唯一
        /// </summary>
        Singleton,
        /// <summary>
        /// 同一次请求或同一个线程
        /// </summary>
        Scoped,
        /// <summary>
        /// 每次都创建新的实例
        /// </summary>
        Transient,
    }
}
