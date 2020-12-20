using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media.Imaging;

namespace Piano_Player
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private Player PianoPlayer = null;

        public MainWindow()
        {
            InitializeComponent();
            PianoPlayer = new Player(this);

            PianoPlayer.PlayStateChanged += UpdateUI;
            PianoPlayer.PlayerProgressChanged += UpdateUI;

            edit_timePerNote.Text = "" + PianoPlayer.NoteTime;
            edit_timePerSpace.Text = "" + PianoPlayer.SpaceTime;
            edit_timePerBreak.Text = "" + PianoPlayer.BreakTime;
            edit_sheets.Text = PianoPlayer.CurrentSheet.RawSheet;
        }
        
        private void edit_sheets_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (PianoPlayer == null) return;
            PianoPlayer.CurrentSheet = new Player.PianoSheet(edit_sheets.Text);
        }

        private void btn_playpause_Click(object sender, RoutedEventArgs e)
        {
            PianoPlayer.Player_Toggle();
        }

        private void btn_stop_Click(object sender, RoutedEventArgs e)
        {
            PianoPlayer.Player_Stop();
        }

        public void UpdateUI()
        {
            if (PianoPlayer == null) return;

            if (Application.Current != null)
            Application.Current.Dispatcher.Invoke(() =>
            {
                bool playing = PianoPlayer.IsPlaying;
                if (!playing)
                    img_btn_playpause.Source = new BitmapImage(new Uri(@"pack://application:,,,/Images/btn_play.png"));
                else
                    img_btn_playpause.Source = new BitmapImage(new Uri(@"pack://application:,,,/Images/btn_pause.png"));

                progress_bar.Value = PianoPlayer.PlayerProgress;
            });
        }

        private void Window_GotKeyboardFocus(object sender, KeyboardFocusChangedEventArgs e)
        {
            if (PianoPlayer == null) return;
            //PianoPlayer.IsMainWindowKeyboardFocused = true;
            PianoPlayer.IsMainWindowKeyboardFocused = Keyboard.FocusedElement != null;
        }

        private void Window_LostKeyboardFocus(object sender, KeyboardFocusChangedEventArgs e)
        {
            if (PianoPlayer == null) return;
            //PianoPlayer.IsMainWindowKeyboardFocused = false;
            PianoPlayer.IsMainWindowKeyboardFocused = Keyboard.FocusedElement != null;
        }

        private void Window_Closed(object sender, EventArgs e)
        {
            if (PianoPlayer == null) return;

            PianoPlayer.PlayerAlive = false;
        }

        private void edit_timePerNote_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (!App.IsStringANumber(edit_timePerNote.Text) || PianoPlayer == null)
            {
                edit_timePerNote.Text = App.NumberOnlyString(edit_timePerNote.Text);
                return;
            }

            try { PianoPlayer.NoteTime = int.Parse(edit_timePerNote.Text); }
            catch (Exception) { Console.WriteLine("> Parsing error while setting Player.NoteTime."); }
        }

        private void edit_timePerSpace_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (!App.IsStringANumber(edit_timePerSpace.Text) || PianoPlayer == null)
            {
                edit_timePerSpace.Text = App.NumberOnlyString(edit_timePerSpace.Text);
                return;
            }

            try { PianoPlayer.SpaceTime = int.Parse(edit_timePerSpace.Text); }
            catch (Exception) { Console.WriteLine("> Parsing error while setting Player.SpaceTime."); }
        }

        private void edit_timePerBreak_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (!App.IsStringANumber(edit_timePerBreak.Text) || PianoPlayer == null)
            {
                edit_timePerBreak.Text = App.NumberOnlyString(edit_timePerBreak.Text);
                return;
            }

            try { PianoPlayer.BreakTime = int.Parse(edit_timePerBreak.Text); }
            catch (Exception) { Console.WriteLine("> Parsing error while setting Player.BreakTime."); }
        }

        private void btn_reset_Click(object sender, RoutedEventArgs e)
        {
            edit_timePerNote.Text = "150";
            edit_timePerSpace.Text = "150";
            edit_timePerBreak.Text = "400";
            edit_sheets.Text = "";

            PianoPlayer.NoteTime = 150;
            PianoPlayer.SpaceTime = 150;
            PianoPlayer.BreakTime = 400;
            PianoPlayer.CurrentSheet = new Player.PianoSheet("");
        }
    }
}
