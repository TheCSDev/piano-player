using System;
using System.IO;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Runtime.Serialization.Formatters.Binary;

namespace Piano_Player
{
    public static class QM
    {
        // =======================================================
        public static string NumberOnlyString(string input)
        {
            string result = "";
            foreach (char ch in input)
                if (char.IsDigit(ch) || (ch == '-' && result.Length == 0))
                    result += ch;
            if (result.Replace("-", "").Length == 0) result = "0";
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
        // -------------------------------------------------------
        // =======================================================
        //og. src: https://stackoverflow.com/questions/7162834/determine-if-current-application-is-activated-has-focus
        /// <summary>Returns true if the current application has focus, false otherwise</summary>
        public static bool ApplicationIsActivated()
        {
            var activatedHandle = GetForegroundWindow();
            if (activatedHandle == IntPtr.Zero)
            {
                return false; // No window is currently activated
            }

            var procId = Process.GetCurrentProcess().Id;
            int activeProcId;
            GetWindowThreadProcessId(activatedHandle, out activeProcId);

            return activeProcId == procId;
        }

        [DllImport("user32.dll", CharSet = CharSet.Auto, ExactSpelling = true)]
        private static extern IntPtr GetForegroundWindow();

        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        private static extern int GetWindowThreadProcessId(IntPtr handle, out int processId);
        // =======================================================
    }
}
