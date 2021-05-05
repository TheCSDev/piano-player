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
        public static Version AppVersion { get; } = Assembly.GetExecutingAssembly().GetName().Version;
        public const int FileVersion = 1;
        public const string FileExtension = "ppsf";
        
        public static string[] StartupArgs { get; private set; }
        // -------------------------------------------------------
        //CurrentAppSettings from URL
        public static string JavaHelperPath { get; } = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location) + "/PianoPlayerHelper.jar";
        public static string UninstallerPath { get; } = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location) + "/unins000.exe";
        // =======================================================
        protected override void OnStartup(StartupEventArgs e)
        {
            //startup setup
            base.OnStartup(e);
            StartupArgs = e.Args;

            //set up window
            MainWindow wnd = new MainWindow(e.Args);
            wnd.Show();
        }
        // =======================================================
    }
}