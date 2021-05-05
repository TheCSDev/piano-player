using System;
using System.IO;
using System.Windows;
using System.Reflection;
using Piano_Player.Update;

namespace Piano_Player
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        // =======================================================
        public static Version AppVersion { get; } = Assembly.GetExecutingAssembly().GetName().Version;
        public const int FileVersion = 1;
        public const string FileExtension = "ppsf";
        // -------------------------------------------------------
        //CurrentAppSettings from URL
        public static string JavaHelperPath { get; } = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location) + "/PianoPlayerHelper.jar";
        public static string UninstallerPath { get; } = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location) + "/unins000.exe";
        // =======================================================
        protected override void OnStartup(StartupEventArgs e)
        {
            //startup setup
            base.OnStartup(e);

            //check for updates
            PianoPlayerUpdater.CheckForUpdates(true);

            //set up window
            MainWindow wnd = new MainWindow(e.Args);
            wnd.Show();
        }
        // =======================================================
    }
}