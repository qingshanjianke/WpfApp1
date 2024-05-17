using System.Windows;

namespace WpfApp1.Common
{
    public static class AppConstants
    {
        public const string LF_DLL = "dll\\lf_pos_dll.dll";
        public const string OPO_DLL = "dll\\opo_pos.dll";
        public const string EHE_DLL = "dll\\EHEScale.dll";
        public const string POS_DLL = "dll\\pos_ad_dll.dll";
        public const string ZQEB_DLL = "dll\\ZQEBSDK.dll";
        public const string HT618_DLL = "dll\\ht_618.dll";
        public const string SUNMI_DLL = "dll\\sunmi_pos.dll";
        public const string SENSOR_Dll = "dll\\SensorDll.dll";
        public const string AURORA_DLL = "dll\\aurora_pos.dll";
        public const string TOLEDO_DLL = "dll\\pos_ad_dll.dll";
        public const string ZHONGKE_DLL = "dll\\zhongke_pos.dll";
        public const string TOLEDOFBP_DLL = "dll\\pos_ad_dllFPB.dll";
        public const string HAISHINEW_DLL = "dll\\HS\\pos_ad_dll.dll";
        public const string SG_pos_ad_dll = "dll\\SG\\pos_ad_dll.dll";
        public const string MtArivaComm_Dll = "dll\\MtArivaComm.dll";
        public const string MtArivaProtocol_DLL = "dll\\MtArivaProtocol.dll";
        public const string SG_pos_ad_dll_stdcall = "dll\\SG\\pos_ad_dll_stdcall.dll";
        public const string HD = "dll\\QiHuaWeightProtocalDll.dll";
        public const string HDPCOMM = "dll\\HD\\PCOMM.dll";


        public static string ENCRYPT_KEY => "h1VXZrF3FQozVKgSgEc2W4SyotNlbE7J"[..16];
        public static string WorkPath => AppDomain.CurrentDomain.BaseDirectory;
        public static string ApplicationData => Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);


        public const string ICON_DIR = "icons";
        public const string Screen_DIR = "Screen";
        public const string Video_DIR = "video";


        public const int HTTP_PORT = 16890;
        public const string SearchStr = "录入商品名称,代码搜索商品";
        public static double POS_MIN_WIDTH => 740 + ((SystemParameters.PrimaryScreenWidth / 1920) - 1) * 200;
        public static double POS_MIN_Height => 450 + ((SystemParameters.PrimaryScreenHeight / 1080) - 1) * 150;

        public const string QiNiuDomain = "https://static.yious.cn";
    }
}
