using System;
using System.IO;
using System.Threading;
using System.Reflection;
using System.Diagnostics;
using System.Windows.Forms;
using WindowsInput;
using WindowsInput.Native;

namespace Piano_Player
{
    public class PlayerInputHandler
    {
        // ================================================
        public static string JavaHelperPath { get { return Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location) + "/PianoPlayerHelper.jar"; } }

        public Player parentPlayer { get; private set; }
        public InputSimulator inputSimulator { get; private set; }
        public bool isJavaInstalled { get; private set; }
        public bool canUseJavaHelper { get; private set; }

        //Java helper variables
        public Process javaHelperProcess { get; private set; }
        // ================================================
        public PlayerInputHandler(Player parentPlayer)
        {
            this.parentPlayer = parentPlayer;
            inputSimulator = new InputSimulator();
            RefreshData();

            if (!isJavaInstalled)
            {
                MessageBox.Show("Please note that most applications block automated " +
                    "input, which is why this application may not work on some " +
                    "applications. To work around this and make automated input " +
                    "be accepted by more applications, I have implemented a Java " +
                    "(jar) file that acts as a helper to automating input. If you " +
                    "wish the application to use this Java helper, please install " +
                    "the latest version of Java on your device. If you choose not " +
                    "to install Java, this application will still run as normal, but " +
                    "it's automated input will work on less applications.", "Piano Player");
            }

            //this thread will make sure to deal with the server stuff
            //should the Java helper be used
            Thread t = new Thread(() =>
            {
                while (true)
                {
                    Thread.Sleep(1500);
                    if (javaHelperProcess != null && !javaHelperProcess.HasExited)
                        SendCommandToHelper("ping");
                }
            });
            t.IsBackground = true;
            t.Start();
        }
        //
        public void RefreshData()
        {
            isJavaInstalled = App.Where("java.exe") != null;
            canUseJavaHelper = isJavaInstalled && File.Exists(JavaHelperPath);
        }
        // ================================================
        public void KeyPress(VirtualKeyCode keyCode)
        {
            if (!canUseJavaHelper) inputSimulator.Keyboard.KeyPress(keyCode);
            else SendCommandToHelper("key-press " + (int)keyCode);
        }
        public void KeyDown(VirtualKeyCode keyCode)
        {
            if (!canUseJavaHelper) inputSimulator.Keyboard.KeyDown(keyCode);
            else SendCommandToHelper("key-down " + (int)keyCode);
        }
        public void KeyUp(VirtualKeyCode keyCode)
        {
            if (!canUseJavaHelper) inputSimulator.Keyboard.KeyUp(keyCode);
            else SendCommandToHelper("key-up " + (int)keyCode);
        }
        // ================================================
        public void SendCommandToHelper(string args)
        {
            //return if helper cannot be used
            if (!canUseJavaHelper || javaHelperProcess == null) return;
            if (javaHelperProcess.HasExited && !parentPlayer.IsPlaying) return;

            //catch and handle an error should it ever occur
            if (javaHelperProcess.HasExited && parentPlayer.IsPlaying)
            {
                parentPlayer.Player_Pause();
                MessageBox.Show("The PianoPlayerHelper.jar Process " +
                    "has unexpectedly crashed.\n\n" +
                    GetProcessLogOuput(javaHelperProcess), "Piano Player");
            }

            javaHelperProcess.StandardInput.WriteLine("/" + args);
        }

        /// <summary>
        /// This method is executed every time the player is played.
        /// This method is there to prevent any Java errors such as
        /// invalid Java version being installed and so on...
        /// 
        /// Upon launching the jar, if everything is alright, the jar will just close
        /// itself without doing anything and the player will play.
        /// However, if something is wrong, an error pup-up will occur, thus preventing
        /// the player from running and once the pop-up is closed, the player app
        /// exits as well.
        /// </summary>
        public void DebugPlayerHelper()
        {
            //if the Java helper cannot be used, ignre this method
            if (!canUseJavaHelper) return;

            //first off, execute the jar file to test it and make sure it works
            Process proc = new Process();
            proc.StartInfo.UseShellExecute = false;
            proc.StartInfo.FileName = "javaw";
            proc.StartInfo.Arguments = "-jar \"" + JavaHelperPath + "\" \"" + "null" + "\"";
            proc.StartInfo.RedirectStandardOutput = true;
            proc.StartInfo.RedirectStandardError = true;
            proc.Start();
            proc.WaitForExit();

            //now get the exit code, and if it's not 0, something went wrong
            //in which case an error window will show up and Java will most
            //likely need to be reconfigured
            if (proc.ExitCode != 0)
            {
                //show the error window
                ShowHelperErrorWindow("There was an error while trying to use the " +
                    "PianoPlayerHelper (Java).\nMake sure your Java is up to date " +
                    "and that there are no other issues with Java.", proc);

                //close the player window
                parentPlayer.parentWindow.Close();
                //kill the player thread
                parentPlayer.PlayerAlive = false;
            }

            //if everything is alright, start the helper (kill the previous one first)
            if (javaHelperProcess != null && !javaHelperProcess.HasExited) javaHelperProcess.Kill();

            javaHelperProcess = new Process();
            javaHelperProcess.StartInfo.UseShellExecute = false;
            javaHelperProcess.StartInfo.FileName = "javaw";
            javaHelperProcess.StartInfo.Arguments = "-jar \"" + JavaHelperPath + "\" \"" + "start-helper" + "\"";
            javaHelperProcess.StartInfo.RedirectStandardInput = true;
            javaHelperProcess.StartInfo.RedirectStandardOutput = true;
            javaHelperProcess.StartInfo.RedirectStandardError = true;
            javaHelperProcess.Start();
        }

        public void ShowHelperErrorWindow(string message, Process proc)
        {
            if (proc == null) return;
            try { proc.Kill(); } catch (Exception) { }

            message += "\n\n" + GetProcessLogOuput(proc);
            message += "\n\n" + "The Process has exited with code " + proc.ExitCode + ".";

            //show the error window
            ErrorWindow errorWindow = new ErrorWindow();
            errorWindow.edit_text.Text = message;
            errorWindow.Show();
        }
        public static String GetProcessLogOuput(Process proc)
        {
            string message = "";
            string s_out = "", e_out = "";
            if (proc.StartInfo.RedirectStandardOutput)
                while (!proc.StandardOutput.EndOfStream) { s_out += "\n" + proc.StandardOutput.ReadLine(); }
            if (proc.StartInfo.RedirectStandardError)
                while (!proc.StandardError.EndOfStream) { e_out += "\n" + proc.StandardError.ReadLine(); }

            if (s_out.Length > 0)
            {
                if (s_out.StartsWith("\n")) s_out = s_out.Substring(1);
                message += "PianoPlayerHelper standard output:\n\n" + s_out;
            }
            if (e_out.Length > 0)
            {
                if (e_out.StartsWith("\n")) e_out = e_out.Substring(1);
                message += "\n\nPianoPlayerHelper error output:\n\n" + e_out;
            }
            return message;
        }
        // ================================================
    }
}
