using System;
using System.IO;
using System.Text;
using System.Text.Json;
using System.Diagnostics;
using Syroot.Windows.IO;

namespace Piano_Player.Update
{
    public class PianoPlayerUpdater
    {
        // =======================================================
        public const string LatestVersionInfoURL = "https://raw.githubusercontent.com/TheCSDev/piano-player/main/AppData/latestVersionInfo.dat";
        public const string UpdaterBatScriptURL  = "https://raw.githubusercontent.com/TheCSDev/piano-player/main/AppData/updaterBatScript.dat";
        // -------------------------------------------------------
        // =======================================================
        public static LatestVersionInfo GetLatestVersionInfo()
        {
            LatestVersionInfo r = null;
            var wc = new System.Net.WebClient();
            try
            {
                string s = wc.DownloadString(LatestVersionInfoURL);
                r = JsonSerializer.Deserialize<LatestVersionInfo>(s);
            }
            catch (Exception) { }
            wc.Dispose();
            return r;
        }
        // -------------------------------------------------------
        public static bool CheckForUpdates(bool askToUpdate = false)
        {
            LatestVersionInfo lvi = GetLatestVersionInfo();
            if (lvi == null) return false;

            int v = lvi.LatestVersion.CompareTo(App.AppVersion);

            if (v > 0 && askToUpdate)
                if (QM.YesNoDialog("An update is available, would you like " +
                    "to update the application?", "Piano Player"))
                {
                    PerformUpdate(lvi);
                    Environment.Exit(0);
                }

            return v > 0;
        }
        // -------------------------------------------------------
        public static void PerformUpdate(LatestVersionInfo lvi)
        {
            if (lvi.LatestVersion.CompareTo(App.AppVersion) <= 0) return;

            try
            {
                var wc = new System.Net.WebClient();
                string downloaded = wc.DownloadString(UpdaterBatScriptURL);
                wc.Dispose();

                StringBuilder batScript = new StringBuilder();
                batScript.AppendLine("@echo off");
                batScript.AppendLine("");
                batScript.AppendLine("set PianoPlayer_APP_DIRECTORY=" + AppDomain.CurrentDomain.BaseDirectory);
                batScript.AppendLine("set PianoPlayer_APP_UNINSTALLER_NAME=" + "unins000.exe");
                batScript.AppendLine("set PianoPlayer_SETUP_FILE_URL=" + lvi.UpdateInstallerURL);
                batScript.AppendLine("set PianoPlayer_SETUP_FILE_NAME=" + "PianoPlayer_Setup.exe");
                batScript.AppendLine("");
                batScript.AppendLine(downloaded);

                string batPath = Path.GetTempPath() + "\\TheCSDev_PianoPlayerUpdater.bat";
                if (File.Exists(batPath)) File.Delete(batPath);
                File.WriteAllText(batPath, batScript.ToString());

                //og. source: https://stackoverflow.com/questions/38321981/how-to-use-c-sharp-run-batch-file-as-administrator-to-install-windows-services
                var psi = new ProcessStartInfo();
                //i got rid of window hiding to let the user know what is going on
                //in the background
                //psi.CreateNoWindow = true; //This hides the dos-style black window that the command prompt usually shows
                //psi.UseShellExecute = false; //and this line was added by TheCSDev
                psi.FileName = @"cmd.exe";
                //no need to run as admin
                //psi.Verb = "runas"; //This is what actually runs the command as administrator
                psi.Arguments = "/C \"" + batPath + "\" run_update";

                var process = new Process();
                process.StartInfo = psi;
                process.Start();
            }
            catch (Exception e)
            {
                ErrorWindow.ShowExceptionWindow("Unable to perform update.", e);
            }
            
            Environment.Exit(0);
        }
        // =======================================================
    }
}
