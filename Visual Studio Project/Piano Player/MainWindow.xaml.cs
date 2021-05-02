using System;
using System.Windows;
using System.ComponentModel;

namespace Piano_Player
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        // =======================================================
        public  Player.TimelinePlayer PianoPlayer { get; private set; }
        // =======================================================
        public MainWindow(string[] Args)
        {
            InitializeComponent();
            PianoPlayer = new Player.TimelinePlayer(this);
        }
        // =======================================================
        private void Window_Closed(object sender, EventArgs e)
        {
            if (PianoPlayer != null) PianoPlayer.Stop();
            Environment.Exit(0);
        }
        // -------------------------------------------------------
        
        // =======================================================
    }
}
