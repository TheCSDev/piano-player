using System;
using System.IO;
using System.Threading;
using System.Reflection;
using System.Diagnostics;
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
        // ================================================
        public PlayerInputHandler(Player parentPlayer)
        {
            this.parentPlayer = parentPlayer;
            inputSimulator = new InputSimulator();
            RefreshData();
        }
        // ================================================
        public void KeyPress(VirtualKeyCode keyCode)
        {
            if (!canUseJavaHelper)
                inputSimulator.Keyboard.KeyPress(keyCode);
            else ExecHelper("key-press " + (int)keyCode);
        }
        public void KeyDown(VirtualKeyCode keyCode)
        {
            if (!canUseJavaHelper)
                inputSimulator.Keyboard.KeyDown(keyCode);
            else ExecHelper("key-down " + (int)keyCode);
        }
        public void KeyUp(VirtualKeyCode keyCode)
        {
            if (!canUseJavaHelper)
                inputSimulator.Keyboard.KeyUp(keyCode);
            else ExecHelper("key-up " + (int)keyCode);
        }
        // ================================================
        public void RefreshData()
        {
            isJavaInstalled = App.Where("java.exe") != null;
            canUseJavaHelper = isJavaInstalled && File.Exists(JavaHelperPath);
        }
        private void ExecHelper(string args)
        {
            if (!canUseJavaHelper) return;

            Thread th_output_reader = new Thread(() =>
            {
                Process proc = new Process();
                proc.StartInfo.UseShellExecute = false;
                proc.StartInfo.FileName = "javaw";
                proc.StartInfo.Arguments = "-jar \"" + JavaHelperPath + "\" \"" + args + "\"";
                proc.StartInfo.RedirectStandardOutput = true;

                //this security thread will be used to prevent the processes
                //from running for longer than a second or so. this is here to prevent
                //processes from running into errors and spamming error pop-ups
                Thread security = new Thread(() =>
                {
                    bool started = false;
                    while (!started) //first wait for the process to start
                    {
                        Thread.Sleep(50);
                        try { Process.GetProcessById(proc.Id); started = true; }
                        catch (Exception) { }
                    }
                    Thread.Sleep(2000); //then wait 2 seconds
                    try
                    {
                        Process.GetProcessById(proc.Id);//check if process still running

                        parentPlayer.Player_Pause(); //prevent further processes
                        proc.Kill(); //kill the process
                    }
                    catch (Exception)
                    {
                        //if finished, log the output
                        proc.WaitForExit(); //just in case
                        while (!proc.StandardOutput.EndOfStream)
                            Console.WriteLine("> " + proc.StandardOutput.ReadLine());
                    }
                });
                security.IsBackground = false;
                security.Start();

                proc.Start();
            });
            th_output_reader.IsBackground = false;
            th_output_reader.Start();
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
            if (!canUseJavaHelper) return;

            Process proc = new Process();
            proc.StartInfo.UseShellExecute = false;
            proc.StartInfo.FileName = "javaw";
            proc.StartInfo.Arguments = "-jar \"" + JavaHelperPath + "\" \"" + "null" + "\"";
            proc.StartInfo.RedirectStandardOutput = true;
            proc.StartInfo.RedirectStandardError = true;
            proc.Start();

            proc.WaitForExit();

            if (proc.ExitCode != 0)
            {
                string output = "There was an error while trying to use the " +
                    "PianoPlayerHelper (Java).\nMake sure your Java is up to date " +
                    "and that there are no other issues with Java.\n" +
                    "The PianoPlayerHelper has exited with code " +
                    proc.ExitCode + ".";

                string s_out = "", e_out = "";
                while (!proc.StandardOutput.EndOfStream) { s_out += "\n" + proc.StandardOutput.ReadLine(); }
                while (!proc.StandardError.EndOfStream) { e_out += "\n" + proc.StandardError.ReadLine(); }

                if (s_out.Length > 0)
                {
                    if(s_out.StartsWith("\n")) s_out = s_out.Substring(1);
                    output += "\n\nPianoPlayerHelper standard output:\n\n" + s_out;
                }
                if (e_out.Length > 0)
                {
                    if (e_out.StartsWith("\n")) e_out = e_out.Substring(1);
                    output += "\n\nPianoPlayerHelper error output:\n\n" + e_out;
                }

                //show the error window
                ErrorWindow errorWindow = new ErrorWindow();
                errorWindow.edit_text.Text = output;
                errorWindow.Show();

                //close the player window
                parentPlayer.parentWindow.Close();
                //kill the player thread
                parentPlayer.PlayerAlive = false;
            }
        }
        // ================================================
    }
}
