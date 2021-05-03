using System;
using System.IO;
using System.Threading;
using System.Diagnostics;
using System.Windows.Forms;
using WindowsInput;
using WindowsInput.Native;

namespace Piano_Player.Player
{
    public class PlayerInputHandler
    {
        // ================================================
        public TimelinePlayer parentPlayer { get; private set; }
        public InputSimulator inputSimulator { get; private set; }
        public bool isJavaInstalled { get; private set; }
        public bool canUseJavaHelper { get; private set; }

        //Java helper variables
        public Process javaHelperProcess { get; private set; }
        // ================================================
        public PlayerInputHandler(TimelinePlayer parentPlayer)
        {
            this.parentPlayer = parentPlayer;
            inputSimulator = new InputSimulator();
            RefreshData();

            if (!isJavaInstalled && File.Exists(App.JavaHelperPath))
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

            if (!File.Exists(App.JavaHelperPath))
            {
                MessageBox.Show(App.JavaHelperPath + "\n\ncould not be found.", "Piano Player");
            }

            //this thread will make sure to deal with the pinging stuff
            //should the Java helper be used if the java helper doesn't recieve a
            //ping within every 3 seconds, it automatically closes
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
            isJavaInstalled = QM.Where("java.exe") != null;
            canUseJavaHelper = isJavaInstalled && File.Exists(App.JavaHelperPath);
        }
        // ================================================
        public void KeyPress(VirtualKeyCode keyCode)
        {
            if (!canUseJavaHelper) inputSimulator.Keyboard.KeyPress(keyCode);
            else
            {
                if(keyCode == VirtualKeyCode.LSHIFT) SendCommandToHelper("key-press " + 16);
                else SendCommandToHelper("key-press " + (int)keyCode);
            }
        }
        public void KeyDown(VirtualKeyCode keyCode)
        {
            if (!canUseJavaHelper) inputSimulator.Keyboard.KeyDown(keyCode);
            else
            {
                if (keyCode == VirtualKeyCode.LSHIFT) SendCommandToHelper("key-down " + 16);
                else SendCommandToHelper("key-down " + (int)keyCode);
            }
        }
        public void KeyUp(VirtualKeyCode keyCode)
        {
            if (!canUseJavaHelper) inputSimulator.Keyboard.KeyUp(keyCode);
            else
            {
                if (keyCode == VirtualKeyCode.LSHIFT) SendCommandToHelper("key-up " + 16);
                else SendCommandToHelper("key-up " + (int)keyCode);
            }
        }
        // ================================================
        public void SendCommandToHelper(string args)
        {
            //return if helper cannot be used
            if (!canUseJavaHelper || javaHelperProcess == null) return;
            if (javaHelperProcess.HasExited && !parentPlayer.Playing) return;

            //catch and handle an error should it ever occur
            if (javaHelperProcess.HasExited && parentPlayer.Playing)
            {
                parentPlayer.Pause();
                MessageBox.Show("The PianoPlayerHelper.jar Process " +
                    "has unexpectedly crashed.\n\n" +
                    GetProcessLogOuput(javaHelperProcess), "Piano Player",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning,
                    MessageBoxDefaultButton.Button1, MessageBoxOptions.DefaultDesktopOnly);
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
        public void StartPlayerHelper()
        {
            RefreshData();

            //if the Java helper cannot be used, ignre this method
            if (!canUseJavaHelper) return;

            //first off, execute the jar file to test it and make sure it works
            Process proc = new Process();
            proc.StartInfo.UseShellExecute = false;
            proc.StartInfo.FileName = "javaw";
            proc.StartInfo.Arguments = "-jar \"" + App.JavaHelperPath + "\" \"" + "null" + "\"";
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

                //kill the player thread (by stopping the player)
                parentPlayer.AbortPlayer();

                //close the player window
                parentPlayer.ParentWindow.Close();
                Environment.Exit(-1);
            }

            //if everything is alright, start the helper (kill the previous one first)
            if (javaHelperProcess != null && !javaHelperProcess.HasExited) javaHelperProcess.Kill();

            Process proc2 = new Process();
            proc2.StartInfo.UseShellExecute = false;
            proc2.StartInfo.FileName = "javaw";
            proc2.StartInfo.Arguments = "-jar \"" + App.JavaHelperPath + "\" \"" + "start-helper" + "\"";
            proc2.StartInfo.RedirectStandardInput = true;
            proc2.StartInfo.RedirectStandardOutput = true;
            proc2.StartInfo.RedirectStandardError = true;
            proc2.Start();
            //avoid concurrent modification by assigning it after running it
            javaHelperProcess = proc2;
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
            errorWindow.ShowDialog();
        }
        
        public static string GetProcessLogOuput(Process proc)
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
