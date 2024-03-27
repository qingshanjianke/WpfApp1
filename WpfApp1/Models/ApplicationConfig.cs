using System.IO;

namespace WpfApp1.Models
{

    public class ApplicationConfig
    {
        public string LogPath => $"{ProgramDataPath}\\Logs";
        public Type MainWindow { get; set; } = typeof(MainWindow);
        public string? ProductName { get; set; }

        public string ProgramDataPath
        {
            get
            {
                var commonPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData), "VIV零售");
                return commonPath;
            }
        }

        public string ScrcpyNetPath => "ScrcpyNet";
        public string? ServiceName { get; set; }
        public string Title { get; set; } = "Test";
        public string TitleVersion => Title + " v" + Version;
        public string? Version { get; set; }
    }
}
