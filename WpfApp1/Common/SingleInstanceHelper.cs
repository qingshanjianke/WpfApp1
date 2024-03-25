using System;
using System.Diagnostics;
using System.IO;
using System.Runtime.InteropServices;
using System.Threading;
using System.Windows;

namespace WpfApp1.Common
{
    public class SingleInstanceHelper
    {
        private static Mutex mutex = null;
        static string filePath = Process.GetCurrentProcess().MainModule.FileName;

        public static void Restart()
        {
            mutex.Close();
            Process.Start(filePath);
            Application.Current.Shutdown();
        }

        public static void Check(string title)
        {
            var fileName = Path.GetFileName(filePath);
            bool isNew = false;
            if (null == mutex)
                mutex = new Mutex(true, fileName, out isNew);
            if (!isNew)
            {
                ActiveAndShowToFront(title);
                Application.Current.Shutdown();
            }
        }

        private static void ActiveAndShowToFront(string titleName)
        {
            //s1:通过WAPi:FindWindow获取运行实例的句柄
            //或者事先保存实例，传递过来           
            IntPtr hwnd = FindWindowW(null, titleName);
            if (hwnd != IntPtr.Zero)
            {
                SetForegroundWindow(hwnd);
                if (IsIconic(hwnd))
                    OpenIcon(hwnd);
            }
        }

        [DllImport("User32", CharSet = CharSet.Unicode)]
        static extern IntPtr FindWindowW(string lpClassName, string lpWindowName);

        [DllImport("User32", CharSet = CharSet.Unicode)]
        static extern bool SetForegroundWindow(IntPtr hWnd);

        [DllImport("User32", CharSet = CharSet.Unicode)]
        static extern bool IsIconic(IntPtr hWnd);

        [DllImport("User32", CharSet = CharSet.Unicode)]
        static extern bool OpenIcon(IntPtr hWnd);
    }
}
