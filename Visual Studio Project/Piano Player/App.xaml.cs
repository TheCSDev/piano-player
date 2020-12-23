using System;
using System.IO;
using System.Windows;

namespace Piano_Player
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        public const string FileExtension = "ppsf";
        public const int FileVersion = 1;
        public static string[] StartupArgs { get; private set; }

        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);
            StartupArgs = e.Args;

            MainWindow wnd = new MainWindow(e.Args);
            wnd.Show();
        }

        /*
        //bVk         - A virtual-key code. (0-254)
        //bScan       - A hardware scan code for the key. (0x45)
        //dwFlags     - Controls various aspects of function operation.
        //dwExtraInfo - An additional value associated with the key stroke.
        [DllImport("User32.dll")]
        public static extern void keybd_event(byte bVk, byte bScan, uint dwFlags, int dwExtraInfo);
        */
        public static bool IsStringANumber(string text)
        {
            foreach (char ch in text) if (!char.IsDigit(ch)) return false;
            return true;
        }

        public static string NumberOnlyString(string input)
        {
            string result = "";
            foreach (char ch in input) if (char.IsDigit(ch)) result += ch;
            return result;
        }

        //og. src: https://stackoverflow.com/questions/22210758/equivalent-of-where-command-prompt-command-in-c-sharp
        public static string Where(string filename)
        {
            var path = Environment.GetEnvironmentVariable("PATH");
            var directories = path.Split(';');

            foreach (var dir in directories)
            {
                var fullpath = Path.Combine(dir, filename);
                if (File.Exists(fullpath)) return fullpath;
            }

            // filename does not exist in path
            return null;
        }
    }
}