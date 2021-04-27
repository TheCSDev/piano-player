using System;
using System.IO;
using System.Diagnostics;
using Syroot.Windows.IO;

namespace Piano_Player.Scripts
{
    public static class Updater
    {
        public static void RunUpdaterBAT()
        {
            try
            {
                if (!Environment.OSVersion.ToString().ToLower().Contains("microsoft windows"))
                    throw new Exception("Unsupported operating system.");

                string APP_DIRECTORY = AppDomain.CurrentDomain.BaseDirectory;
                string APP_UNINSTALLER_NAME = "unins000.exe";
                string SETUP_FILE_URL = App.CAS.UpdateInstallerURL;

                string SETUP_FILE_NAME = "PianoPlayer_Setup.exe";

                if (APP_DIRECTORY.EndsWith("\\") || APP_DIRECTORY.EndsWith("/"))
                    APP_DIRECTORY = APP_DIRECTORY.Remove(APP_DIRECTORY.Length - 1);

                string bat_script = "@echo off\r\n\r\nrem | It is important to run from downloads because\r\nrem | the batch script could run as admin (C://System32)\r\ncd %userprofile%\\Downloads\r\n\r\nrem | Make sure the script has been given a \"key\" argument\r\nrem | to prevent the user from running this script\r\nif not \"%*\"==\"run_update\" (\r\nstart /b \"\" cmd /c del \"%~f0\"&exit /b\r\nexit\r\n)\r\n\r\nrem >>> Define system variables\r\nrem 1. Veriables controlled by the app itself\r\nset APP_DIRECTORY=" + APP_DIRECTORY + "\r\nset APP_UNINSTALLER_NAME=" + APP_UNINSTALLER_NAME + "\r\nset SETUP_FILE_URL=" + SETUP_FILE_URL + "\n\r\nrem 2. Constant variables\r\nset SETUP_FILE_NAME=" + SETUP_FILE_NAME + "\r\n\r\nrem Wait a little bit before running the update\r\ntimeout 1\r\n\r\nrem Run the uninstaller and wait for it to finish\r\nstart /w \"\" \"%APP_DIRECTORY%\\%APP_UNINSTALLER_NAME%\"\r\n\r\nrem Check if the app is uninstalling. If not, quit the updater and delete this script.\r\nif not %errorlevel%==0 (\r\ngoto exit\r\n)\r\n\r\nrem Wait for the app to finish uninstalling\r\n:loop1\r\nif not exist \"%APP_DIRECTORY%/\" ( goto loop2 )\r\ntimeout 1\r\ngoto loop1\r\n:loop2\r\n\r\nrem Execuute Windows Powershell and tell it to download the update file\r\nif exist \"%SETUP_FILE_NAME%\" ( goto exit )\r\npowershell -Command \"(New-Object Net.WebClient).DownloadFile('%SETUP_FILE_URL%', '%SETUP_FILE_NAME%')\"\r\ntimeout 1 /nobreak\r\n\r\nrem Execute the downloaded update file\r\nstart PianoPlayer_Setup.exe\r\n\r\nrem | Exit the process, delete the installer if it was\r\nrem | downloaded and used and make the batch file delete itself\r\nrem | Also do not allow the loop to run indefinitely\r\n:exit\r\n\r\nset /A counter=0\r\n:loop3\r\nset /A counter=%counter%+1\r\nif %counter% gtr 420 ( exit )\r\n\r\ntimeout 1\r\nif exist \"%SETUP_FILE_NAME%\" (\r\ndel \"%SETUP_FILE_NAME%\" /q\r\nif exist \"%SETUP_FILE_NAME%\" goto loop3\r\n)\r\n\r\nstart /b \"\" cmd /c del \"%~f0\"&exit /b\r\nexit";

                string bat_path = new KnownFolder(KnownFolderType.Downloads).Path + "/PianoPlayer_Updater.bat";
                File.WriteAllText(bat_path, bat_script);

                //og. source: https://stackoverflow.com/questions/38321981/how-to-use-c-sharp-run-batch-file-as-administrator-to-install-windows-services
                var psi = new ProcessStartInfo();
                psi.CreateNoWindow = true; //This hides the dos-style black window that the command prompt usually shows
                psi.UseShellExecute = false; //and this line was added by TheCSDev
                psi.FileName = @"cmd.exe";
                psi.Verb = "runas"; //This is what actually runs the command as administrator
                psi.Arguments = "/C \"" + bat_path + "\" run_update";

                var process = new Process();
                process.StartInfo = psi;
                process.Start();
            }
            catch (Exception e)
            {
                ErrorWindow.ShowExceptionWindow(
                    "Unable to update the app.\n" +
                    "Please try updaing the app manually by going " +
                    "on it's official webpage and downloading an installer.", e);
                return;
            }

            Environment.Exit(0);
        }
    }
}
