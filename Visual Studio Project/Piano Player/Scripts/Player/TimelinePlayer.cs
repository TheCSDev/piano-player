using System.Threading;
using WindowsInput.Native;

namespace Piano_Player.Player
{
    public class TimelinePlayer
    {
        // =======================================================
        public const char PlayerCommandPrefix = '_';

        public delegate void PlayStateChangedHandler();
        // =======================================================
        public  readonly MainWindow         ParentWindow;
        private readonly PlayerInputHandler InputHandler;
        private readonly Thread             PlayerThread;

        public PlayStateChangedHandler PlayStateChanged;
        // -------------------------------------------------------
        private Timeline _currentTimeline = new Timeline();
        public Timeline CurrentTimeline
        {
            get { return _currentTimeline; }
            set { if (value != null) _currentTimeline = value; }
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
            ParentWindow    = parentWindow;
            InputHandler    = new PlayerInputHandler(this);
            PlayerThread = new Thread(() => { PlayerThreadM(this); });
            PlayerThread.IsBackground = false;
            PlayerThread.Start();

            //CurrentTimeline = new Timeline();
            
            Playing         = false;
            Time            = 0;
        }
        // =======================================================
        private static void PlayerThreadM(TimelinePlayer player)
        {
            try
            {
                bool continuedLastFrame = false;

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
                        Thread.Sleep(1000);
                        continuedLastFrame = false;
                    }

                    Thread.Sleep(1);
                    player.Time++;

                    Timeline.PlayerAction action;
                    string[] args;
                    player.CurrentTimeline.GetPlayerAction(player.Time, out action, out args);

                    if (action == Timeline.PlayerAction.KeyPress)
                        foreach (char ch in args[0].ToCharArray())
                            player.KeyPress(ch);
                    else if (action == Timeline.PlayerAction.Sleep)
                        continue;
                    else if (action == Timeline.PlayerAction.Stop)
                        player.Stop();
                }
            }
            catch (ThreadAbortException) { player.Playing = false; }
        }

        private void KeyPress(char ch)
        {
            if (!Timeline.ChValid(ch)) return;

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
            try { PlayerThread.Abort(); } catch (System.Exception) { }
            try { ParentWindow.IsEnabled = false; } catch (System.Exception) { }
        }
        // =======================================================
    }
}
