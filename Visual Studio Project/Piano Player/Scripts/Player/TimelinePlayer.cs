using System.Threading;
using WindowsInput.Native;

namespace Piano_Player.Player
{
    public class TimelinePlayer
    {
        // =======================================================
        public const char PlayerCommandPrefix = '_';

        public delegate void PlayStateChangedHandler();
        public delegate void ProgressUpdateHandler(); 
        // =======================================================
        public  readonly MainWindow         ParentWindow;
        private readonly PlayerInputHandler InputHandler;
        private readonly Thread             PlayerThread;

        public PlayStateChangedHandler PlayStateChanged;
        public ProgressUpdateHandler   ProgressUpdate;
        // -------------------------------------------------------
        private Timeline _currentTimeline = new Timeline();
        public  Timeline CurrentTimeline
        {
            get { return _currentTimeline; }
            set
            {
                if (value != null)
                {
                    _currentTimeline = value;
                    ParentWindow.UpdateUI_slider_progress();
                    ParentWindow.UpdateUI_label_time();
                }
            }
        }
        // -------------------------------------------------------
        private bool _playing = false;
        public  bool Playing
        {
            get { return _playing; }
            private set
            {
                bool trigger = false;
                if (_playing != value) trigger = true;
                _playing = value;
                if (trigger) PlayStateChanged();
            }
        }
        public  int  Time { get; set; } //ms
        // =======================================================
        public TimelinePlayer(MainWindow parentWindow)
        {
            Playing         = false;
            Time            = 0;
            
            ParentWindow    = parentWindow;
            InputHandler    = new PlayerInputHandler(this);
            PlayerThread = new Thread(() => { PlayerThreadM(this); });
            PlayerThread.IsBackground = true;
            PlayerThread.Start();

            //CurrentTimeline = new Timeline();
        }
        // =======================================================
        private static void PlayerThreadM(TimelinePlayer player)
        {
            try
            {
                bool continuedLastFrame = false;
                int progressUpdateCooldown = 1000;

                while (true)
                {
                    if (!player.Playing || QM.ApplicationIsActivated())
                    {
                        Thread.Sleep(500);
                        continuedLastFrame = true;
                        continue;
                    }

                    if (continuedLastFrame)
                    {
                        Thread.Sleep(1500);
                        continuedLastFrame = false;
                    }

                    if (progressUpdateCooldown <= 0)
                    {
                        player.ProgressUpdate();
                        progressUpdateCooldown = 800;
                    }

                    Timeline.PlayerAction action;
                    string[] args;
                    player.CurrentTimeline.GetPlayerAction(player.Time, out action, out args);

                    //has to be at least 1
                    int sleptFor = 1;

                    if (action == Timeline.PlayerAction.KeyPress)
                    {
                        foreach (char ch in args[0].ToCharArray())
                            player.KeyPress(ch);
                        Thread.Sleep(1);
                    }

                    else if (action == Timeline.PlayerAction.Sleep)
                    {
                        int t = int.Parse(args[0]);

                        if (t >= 1000)
                        {
                            Thread.Sleep(1000);
                            sleptFor = 1000;
                        }
                        else if (t >= 400)
                        {
                            Thread.Sleep(400);
                            sleptFor = 400;
                        }
                        else if (t >= 100)
                        {
                            Thread.Sleep(100);
                            sleptFor = 100;
                        }
                        else if (t >= 50)
                        {
                            Thread.Sleep(50);
                            sleptFor = 50;
                        }
                        else if (t >= 10)
                        {
                            Thread.Sleep(10);
                            sleptFor = 10;
                        }
                        else if (t >= 1)
                        {
                            Thread.Sleep(1);
                            sleptFor = 1;
                        }
                    }

                    else if (action == Timeline.PlayerAction.Stop)
                    {
                        player.ProgressUpdate();
                        player.Stop();
                    }

                    player.Time += sleptFor;
                    progressUpdateCooldown -= sleptFor;
                }
            }
            catch (ThreadAbortException) { player.Playing = false; }
        }

        private void KeyPress(char ch)
        {
            if (!Timeline.ChValid(ch) || InputHandler.KeysPressedBeforePing
                >= PlayerInputHandler.KeyLimitPerPing) return;

            if (char.IsLetter(ch))
            {
                if (char.IsUpper(ch)) InputHandler.KeyDown(VirtualKeyCode.LSHIFT);
                InputHandler.KeyPress((VirtualKeyCode)char.ToUpper(ch));
                if (char.IsUpper(ch)) InputHandler.KeyUp(VirtualKeyCode.LSHIFT);
            }
            else if(Timeline.NumberShiftCharacters.Contains(ch.ToString()))
            {
                InputHandler.KeyDown(VirtualKeyCode.LSHIFT);
                //and this is why NumberShiftCharacters character order is important
                InputHandler.KeyPress((VirtualKeyCode)
                    Timeline.NumberShiftCharacters.IndexOf(ch.ToString()));
                InputHandler.KeyUp(VirtualKeyCode.LSHIFT);
            }
        }
        // -------------------------------------------------------
        public void Play()
        {
            if (Playing) return;

            InputHandler.StartPlayerHelper();

            Playing = true;
        }
        public void Pause()
        {
            if (!Playing) return;

            if (InputHandler.javaHelperProcess != null &&
                !InputHandler.javaHelperProcess.HasExited)
                InputHandler.javaHelperProcess.Kill();

            Playing = false;
        }
        public void Stop()
        {
            Pause();
            Time = -1;
        }
        public void ToglePlayPause() { if (Playing) Pause(); else Play(); }
        public void AbortPlayer()
        {
            try { Stop(); } catch (System.Exception) { }
            try
            {
                //first the thread
                if(PlayerThread != null) PlayerThread.Abort();
                //and then the process because process killing
                //is more likely to throw an exception
                if(InputHandler.javaHelperProcess != null)
                    InputHandler.javaHelperProcess.Kill();
            }
            catch (System.Exception) { }
            try
            {
                ParentWindow.btn_playPause.IsEnabled = false;
                ParentWindow.btn_Stop.IsEnabled = false;
            }
            catch (System.Exception) { }
        }
        // =======================================================
    }
}
