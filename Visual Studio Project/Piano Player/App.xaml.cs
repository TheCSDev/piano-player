using System;
using System.IO;
using System.Net;
using System.Windows;
using System.Reflection;
using System.Text.Json;
using Piano_Player.Scripts;

namespace Piano_Player
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        // =======================================================
        [Serializable]
        public class AppSettings
        {
            // ----------------------------
            [Serializable]
            public class PVersion
            {
                public int Major { get; set; }
                public int Minor { get; set; }
                public int Build { get; set; }
                public int Revision { get; set; }

                public PVersion() { }
                public PVersion(int maj, int min, int bui, int rev)
                {
                    Major = maj; Minor = min; Build = bui; Revision = rev;
                }

                public int CompareTo(Version v)
                {
                    return new Version(Major, Minor, Build, Revision)
                        .CompareTo(v);
                }
                public int CompareTo(PVersion v)
                {
                    return new Version(Major, Minor, Build, Revision)
                        .CompareTo(new Version(v.Major, v.Minor, v.Build, v.Revision));
                }
            }
            // ----------------------------
            /// <summary>Latest available version (4 digits separated by full stops).</summary>
            public PVersion LatestVersion { get; set; }
            public string UpdateInstallerURL { get; set; }
            // ----------------------------
        }
        // =======================================================
        public static Version AppVersion { get; } = Assembly.GetExecutingAssembly().GetName().Version;
        public const int FileVersion = 1;
        public const string FileExtension = "ppsf";
        public const string CAS_URL = "https://raw.githubusercontent.com/TheCSDev/piano-player/main/appSettings.dat";
        public static string[] StartupArgs { get; private set; }
        // -------------------------------------------------------
        //CurrentAppSettings from URL
        public static AppSettings CAS { get; private set; }
        public static string JavaHelperPath { get; } = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location) + "/PianoPlayerHelper.jar";
        public static string UninstallerPath { get; } = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location) + "/unins000.exe";
        // =======================================================
        protected override void OnStartup(StartupEventArgs e)
        {
            //startup setup
            base.OnStartup(e);
            CAS = null;
            StartupArgs = e.Args;

            //grab current app settings and check for updates
            GetCAS();

            if (CAS != null && CAS.LatestVersion.CompareTo(AppVersion) > 0)
            {
                MessageBoxResult mbr = MessageBox.Show
                    ("An update for Piano Player is available. " +
                    "Would you like to download and install it? " +
                    "Please make sure you have uninstalled the " +
                    "current version of Piano Player before " +
                    "installing an update.",
                    "Piano Player - Update", MessageBoxButton.YesNo);

                if (mbr == MessageBoxResult.Yes) Updater.RunUpdaterBAT();
            }

            //set up window
            MainWindow wnd = new MainWindow(e.Args);
            wnd.Show();
        }
        // -------------------------------------------------------
        /// <summary>
        /// Gets the current app settings from the current app settings URL.
        /// If grabbing is unsuccesful, CAS.GrabSuccessful will be set to false.
        /// </summary>
        public static void GetCAS()
        {
            WebClient client = new WebClient();
            try
            {
                string str = client.DownloadString(CAS_URL);
                AppSettings _cas = JsonSerializer.Deserialize<AppSettings>(str);
                CAS = _cas; //grab successful
            }
            catch (Exception) { /*grab unsuccessful*/ }
            client.Dispose();
        }
        // =======================================================
        public static bool IsStringANumber(string text)
        {
            if (text.Length == 0) return false;
            foreach (char ch in text) if (!char.IsDigit(ch)) return false;
            return true;
        }
        // -------------------------------------------------------
        public static string NumberOnlyString(string input)
        {
            string result = "";
            foreach (char ch in input) if (char.IsDigit(ch)) result += ch;
            if (result.Length == 0) result = "0";
            return result;
        }
        // -------------------------------------------------------
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
        // -------------------------------------------------------
        public static int ClampInt(int input, int min, int max)
        {
            if (input < min) input = min;
            else if (input > max) input = max;
            return input;
        }
        // =======================================================
    }
}