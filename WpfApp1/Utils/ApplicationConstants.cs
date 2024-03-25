using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WpfApp1.Utils
{
    class ApplicationConstants
    {
        public const string KOL = "kol";
        public const string KolGroup = "_KOL_";
        public const string WebApiGroup = "_Public_";
        public const string AppName = "scrcpy";
        public const string NoLimit = "不限";

        public static readonly string[] RegionOptions = new string[] { "不限", "北京", "天津", "河北", "山西", "内蒙古", "辽宁", "吉林", "黑龙江",
                                                                   "上海", "江苏", "浙江", "安徽", "福建", "江西", "山东", "河南", "湖北", "湖南", "广东", "广西",
                                                                   "海南", "重庆", "四川", "贵州", "云南", "西藏", "陕西", "甘肃", "青海", "宁夏", "新疆", "台湾", "香港", "澳门" };
        public static readonly string[] SexOptions = new string[] { "不限", "男", "女" };
        public static readonly string[] OpenShopOptions = new string[] { "不限", "开通", "未开通" };

        public const string NameOfRpaDouYin = "RpaClient.DouYin";

        public const string DeviceDownloadFolder = "/storage/emulated/0/Download";

        public const string PermissionOwner = "未分配";
        public const string DeviceWorkMode = "USB模式";
        public const string DeviceNetWorkMode = "网络模式";
    }
}
