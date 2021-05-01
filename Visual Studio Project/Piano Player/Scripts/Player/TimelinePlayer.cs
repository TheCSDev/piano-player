using System.Threading;

namespace Piano_Player.Player
{
    public class TimelinePlayer
    {
        // =======================================================
        private readonly Thread PlayerThread;
        // -------------------------------------------------------
        public readonly MainWindow ParentWindow;
        public Timeline CurrentTimeline { get; private set; }
        // -------------------------------------------------------
        public bool Playing { get; private set; }
        public int  Time    { get; set; } //ms
        // =======================================================
        public TimelinePlayer(MainWindow parentWindow)
        {
            ParentWindow    = parentWindow;
            CurrentTimeline = new Timeline();
            Playing         = false;
            Time            = 0;

            PlayerThread = new Thread(() => { PlayerThreadM(this); })
            { IsBackground = true };
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

        }
        // -------------------------------------------------------
        public void Play() { Playing = true; if (!PlayerThread.IsAlive) PlayerThread.Start(); }
        public void Pause() { Playing = false; }
        public void Stop() {  Playing = false; Time = -1; PlayerThread.Abort(); }
        public void ToglePlayPause() { if (Playing) Pause(); else Play(); }
        // =======================================================
    }
}
