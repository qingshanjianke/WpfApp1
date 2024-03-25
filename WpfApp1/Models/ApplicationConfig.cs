using System.IO;

namespace WpfApp1.Models
{

    public class ApplicationConfig
    {
        public string AdbPath => AppDomain.CurrentDomain.BaseDirectory + "ScrcpyNet\\adb.exe";
        public string ApkPath => AppDomain.CurrentDomain.BaseDirectory + "Resources\\Apks\\";
        public string ExePath => AppDomain.CurrentDomain.BaseDirectory + "Resources\\Exes\\";
        public string LogPath => $"{ProgramDataPath}\\Logs";

        public Type MainWindow { get; set; }
        public string ProductName { get; set; }

        public string ProgramDataPath
        {
            get
            {
                var commonPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData), "光年实验室");

                var oldAppPath = Path.Combine(commonPath, Title);
                var newAppPath = Path.Combine(commonPath, ProductName);

                //补丁,当所有客户升级到新版本后可以删除
                var rpaCommonPath = Path.Combine(commonPath, "RpaClient.Common");
                if (Directory.Exists(rpaCommonPath))
                {
                    var files = Directory.GetFiles(rpaCommonPath);
                    // 遍历所有文件，逐个移动到目标文件夹
                    foreach (var file in files)
                    {
                        var fileName = Path.GetFileName(file);
                        var targetPath = Path.Combine(newAppPath, fileName);
                        File.Move(file, targetPath, true);
                    }

                    Directory.Delete(rpaCommonPath, true);
                }

                if (!Directory.Exists(newAppPath))
                {
                    Directory.CreateDirectory(newAppPath);

                    if (Directory.Exists(oldAppPath))
                    {
                        var files = Directory.GetFiles(oldAppPath);
                        // 遍历所有文件，逐个移动到目标文件夹
                        foreach (var file in files)
                        {
                            var fileName = Path.GetFileName(file);
                            var targetPath = Path.Combine(newAppPath, fileName);
                            File.Copy(file, targetPath);
                        }
                    }
                }

                return newAppPath;
            }
        }

        public string ScrcpyNetPath => "ScrcpyNet";
        public string ServiceName { get; set; }
        public string Title { get; set; } = "Test";
        public string TitleVersion => Title + " v" + Version;
        public string? Version { get; set; }
    }
}
