using System;
using System.IO;
using System.Net;
using System.Windows;
using System.Reflection;
using System.Text.Json;

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
                public static explicit operator Version(PVersion a)
                {
                    return new Version(a.Major, a.Minor, a.Build, a.Revision);
                }

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
                    return ((Version)this).CompareTo(v);
                }
                public int CompareTo(PVersion v)
                {
                    return ((Version)this).CompareTo(((Version)v));
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

            //TODO - IF ARGS HAS A PATH TO A PPSF FILE, LOAD IT
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
    }
}