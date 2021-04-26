using System;
using System.ComponentModel;
using System.IO;
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
        // =======================================================
        public Player PianoPlayer { get; private set; }
        public SaveLoadSystem SheetSaveLoad { get; private set; }
        // =======================================================
        public MainWindow(string[] Args)
        {
            InitializeComponent();
            PianoPlayer = new Player(this);
            SheetSaveLoad = new SaveLoadSystem(this);

            PianoPlayer.PlayStateChanged += UpdateUI;
            PianoPlayer.PlayerProgressChanged += UpdateUI;

            edit_timePerNote.Text = "" + PianoPlayer.NoteTime;
            edit_timePerSpace.Text = "" + PianoPlayer.SpaceTime;
            edit_timePerBreak.Text = "" + PianoPlayer.BreakTime;
            edit_sheets.Text = PianoPlayer.CurrentSheet.RawSheet;

            SheetSaveLoad.ChangesSaved = true;
            if (Args.Length > 0 && Args[0].EndsWith(App.FileExtension) && File.Exists(Args[0]))
                SheetSaveLoad.LoadFile(Args[0]);
        }
        // =======================================================
        private void edit_sheets_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (PianoPlayer == null) return;
            PianoPlayer.CurrentSheet = new Player.PianoSheet(edit_sheets.Text);
            SheetSaveLoad.OnChangesMade();
        }

        private void edit_timePerNote_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (!App.IsStringANumber(edit_timePerNote.Text) || PianoPlayer == null)
            {
                edit_timePerNote.Text = App.NumberOnlyString(edit_timePerNote.Text);
                return;
            }

            try
            {
                PianoPlayer.NoteTime = int.Parse(edit_timePerNote.Text);
                SheetSaveLoad.OnChangesMade();
            }
            catch (Exception) { Console.WriteLine("> Parsing error while setting Player.NoteTime."); }

            if (PianoPlayer.NoteTime < 10)
            {
                PianoPlayer.NoteTime = 10;
                edit_timePerNote.Text = "10";
            }
            else if (PianoPlayer.NoteTime > 10000)
            {
                PianoPlayer.NoteTime = 10000;
                edit_timePerNote.Text = "10000";
            }
        }

        private void edit_timePerSpace_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (!App.IsStringANumber(edit_timePerSpace.Text) || PianoPlayer == null)
            {
                edit_timePerSpace.Text = App.NumberOnlyString(edit_timePerSpace.Text);
                return;
            }

            try
            {
                PianoPlayer.SpaceTime = int.Parse(edit_timePerSpace.Text);
                SheetSaveLoad.OnChangesMade();
            }
            catch (Exception) { Console.WriteLine("> Parsing error while setting Player.SpaceTime."); }

            if (PianoPlayer.SpaceTime < 10)
            {
                PianoPlayer.SpaceTime = 10;
                edit_timePerSpace.Text = "10";
            }
            else if (PianoPlayer.SpaceTime > 10000)
            {
                PianoPlayer.SpaceTime = 10000;
                edit_timePerSpace.Text = "10000";
            }
        }

        private void edit_timePerBreak_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (!App.IsStringANumber(edit_timePerBreak.Text) || PianoPlayer == null)
            {
                edit_timePerBreak.Text = App.NumberOnlyString(edit_timePerBreak.Text);
                return;
            }

            try
            {
                PianoPlayer.BreakTime = int.Parse(edit_timePerBreak.Text);
                SheetSaveLoad.OnChangesMade();
            }
            catch (Exception) { Console.WriteLine("> Parsing error while setting Player.BreakTime."); }

            if (PianoPlayer.BreakTime < 10)
            {
                PianoPlayer.BreakTime = 10;
                edit_timePerBreak.Text = "10";
            }
            else if (PianoPlayer.BreakTime > 10000)
            {
                PianoPlayer.BreakTime = 10000;
                edit_timePerBreak.Text = "10000";
            }
        }

        private void btn_playpause_Click(object sender, RoutedEventArgs e) { PianoPlayer.Player_Toggle(); }

        private void btn_stop_Click(object sender, RoutedEventArgs e) { PianoPlayer.Player_Stop(); }

        private void btn_reset_Click(object sender, RoutedEventArgs e)
        {
            PianoPlayer.Player_Stop();

            edit_timePerNote.Text = "150";
            edit_timePerSpace.Text = "150";
            edit_timePerBreak.Text = "400";
            edit_sheets.Text = "";

            PianoPlayer.NoteTime = 150;
            PianoPlayer.SpaceTime = 150;
            PianoPlayer.BreakTime = 400;
            PianoPlayer.CurrentSheet = new Player.PianoSheet("");
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

        protected override void OnClosing(CancelEventArgs e)
        {
            base.OnClosing(e);
            e.Cancel = !CloseWithSaveDialog();
        }

        private void Window_Closed(object sender, EventArgs e)
        {
            if (PianoPlayer == null) return;

            PianoPlayer.PlayerAlive = false;
        }

        private void menu_file_new_Click(object sender, RoutedEventArgs e) { SheetSaveLoad.NewFile(); }

        private void menu_file_open_Click(object sender, RoutedEventArgs e) { SheetSaveLoad.OpenFile(); }

        private void menu_file_save_Click(object sender, RoutedEventArgs e) { SheetSaveLoad.SaveFile(); }

        private void menu_file_saveas_Click(object sender, RoutedEventArgs e) { SheetSaveLoad.SaveFileAs(); }

        private void menu_file_exit_Click(object sender, RoutedEventArgs e) { Close(); }

        private void menu_tools_mergesheets_Click(object sender, RoutedEventArgs e)
        {
            new SheetMergerWindow(this).Show();
            menu_tools_mergesheets.IsEnabled = false;
        }
        // =======================================================
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
        // =======================================================
        /// <summary>
        /// Returns true if the window is allowed to close or if the user
        /// chooses not to save their changes.
        /// </summary>
        /// <returns></returns>
        public bool CloseWithSaveDialog()
        {
            System.Windows.Forms.DialogResult dr = SheetSaveLoad.ShowConfirmationDialog();
            if (dr == System.Windows.Forms.DialogResult.Cancel) return false;
            else if (dr == System.Windows.Forms.DialogResult.Yes)
                if (!SheetSaveLoad.SaveFile()) return false;

            return true;
        }
        // =======================================================
    }
}
