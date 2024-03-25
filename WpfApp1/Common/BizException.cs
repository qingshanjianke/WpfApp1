using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace WpfApp1.Common
{
    /// <summary>
    ///     支持传错误码的Exception类(在调试-窗口-异常设置中取消勾选，或者等抛出异常时展开选项后取消勾选，否则程序会中断)
    /// </summary>
    public class BizException : Exception
    {
        //如果需要标识错误类型让前端好进行特殊处理，则加到BizCode枚举里面，然后throw new BizException(BizCode.XXXXX);
        public BizException(BizCode errorCode, string errorMessage = null) : base(errorMessage)
        {
            ErrorCode = errorCode;
            ErrorMessage = errorMessage ?? GetEnumDescription(errorCode);
        }

        public BizException(string errorMessage) : this(errorMessage, null)
        {

        }

        /// <summary>
        /// Initializes a new instance of the <see cref="BizException"/> class.
        /// </summary>
        /// <param name="errorMessage">The error message.</param>
        /// <param name="innerException">The inner exception.</param>
        public BizException(string errorMessage, Exception? innerException) : base(errorMessage, innerException)
        {
            ErrorCode = BizCode.Error;
            ErrorMessage = errorMessage;
        }

        public BizCode ErrorCode { get; set; }
        public string ErrorMessage { get; set; }

        public string GetEnumDescription(Enum enumValue)
        {
            string value = enumValue.ToString();
            FieldInfo field = enumValue.GetType().GetField(value);
            var descriptionAttribute = field.GetCustomAttribute<DescriptionAttribute>();
            if (descriptionAttribute == null)
                return value;
            return descriptionAttribute.Description;
        }
    }

    public enum BizCode
    {
        [Description("系统异常")] Error = -1,
    }
}
