using System;
using System.IO;
using System.Windows;
using System.Threading;
using System.Reflection;
using System.Runtime.InteropServices;
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
        //og. src: https://stackoverflow.com/questions/502303/how-do-i-programmatically-get-the-guid-of-an-application-in-net-2-0
        public static string AppGUID { get { return ((GuidAttribute)Assembly.GetExecutingAssembly().GetCustomAttributes(typeof(GuidAttribute), true)[0]).Value; } }
        public const  int    FileVersion = 1;
        public const  string FileExtension = "ppsf";
        // -------------------------------------------------------
        public static readonly Mutex AppMutex = new Mutex(true, "{" + AppGUID + "}");
        // -------------------------------------------------------
        //CurrentAppSettings from URL
        public static string AppDirPath { get; } = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
        public static string JavaHelperPath { get; } = AppDirPath + "/PianoPlayerHelper.jar";
        public static string UninstallerPath { get; } = AppDirPath + "/unins000.exe";
        // =======================================================
        protected override void OnStartup(StartupEventArgs e)
        {
            Test.AudioTest();

            //setup a mutex
            if (!AppMutex.WaitOne(TimeSpan.Zero))
            {
                MessageBox.Show("Piano Player is already running.");

                AppMutex.Dispose();
                Environment.Exit(1);
            }

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