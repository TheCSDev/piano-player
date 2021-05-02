using System.Threading;
using WindowsInput.Native;

namespace Piano_Player.Player
{
    public class TimelinePlayer
    {
        // =======================================================
        public const char PlayerCommandPrefix = '_';
        // =======================================================
        public  readonly MainWindow         ParentWindow;
        private readonly Thread             PlayerThread;
        private readonly PlayerInputHandler InputHandler;
        // -------------------------------------------------------
        public Timeline CurrentTimeline { get; private set; }
        // -------------------------------------------------------
        public bool Playing { get; private set; }
        public int  Time    { get; set; } //ms
        // =======================================================
        public TimelinePlayer(MainWindow parentWindow)
        {
            ParentWindow    = parentWindow;
            CurrentTimeline = new Timeline();
            InputHandler    = new PlayerInputHandler(this);

            PlayerThread = new Thread(() => { PlayerThreadM(this); });
            PlayerThread.IsBackground = true;
            
            Playing         = false;
            Time            = 0;
        }
        // =======================================================
        private static void PlayerThreadM(TimelinePlayer player)
        {
            while (true)
            {
                Thread.Sleep(1);
                if (!player.Playing) break;
                player.Time++;

                Timeline.PlayerAction action;
                string[] args;
                player.CurrentTimeline.GetPlayerAction(player.Time, out action, out args);

                switch (action)
                {
                    case Timeline.PlayerAction.KeyPress:
                        foreach (char ch in args[0].ToCharArray()) player.KeyPress(ch);
                        break;
                    case Timeline.PlayerAction.Sleep: continue;
                    case Timeline.PlayerAction.Stop: player.Stop(); break;
                }
            }
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
            InputHandler.RefreshData();
            InputHandler.DebugPlayerHelper();
            if (!PlayerThread.IsAlive) PlayerThread.Start();
            
            Playing = true;
        }
        public void Pause()
        {
            if (InputHandler.javaHelperProcess != null &&
                !InputHandler.javaHelperProcess.HasExited)
                InputHandler.javaHelperProcess.Kill();
            
            Playing = false;
        }
        public void Stop()
        {
            Pause();
            Time = -1;
            PlayerThread.Abort();
        }
        public void ToglePlayPause() { if (Playing) Pause(); else Play(); }
        // =======================================================
    }
}
