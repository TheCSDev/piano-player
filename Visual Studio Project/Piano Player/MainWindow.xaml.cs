using System;
using System.Windows;
using System.Windows.Media;
using System.Windows.Controls;
using System.Collections.Generic;
using System.Windows.Media.Imaging;
using Piano_Player.Player;

namespace Piano_Player
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        // =======================================================
        public TimelinePlayer PianoPlayer { get; private set; }
        // =======================================================
        public MainWindow(string[] Args)
        {
            //initialize components
            InitializeComponent();
            PianoPlayer = new TimelinePlayer(this);

            //handle events
            PianoPlayer.PlayStateChanged += UpdateUI_btn_PlayPause;
            PianoPlayer.PlayStateChanged += () =>
            {
                if(Application.Current != null)
                Application.Current.Dispatcher.Invoke(() =>
                {
                    if (PianoPlayer.Playing)
                    {
                        tabs.SelectedItem = tabs.Items[0];
                        ((TabItem)tabs.Items[1]).IsEnabled = false;
                    }
                    else ((TabItem)tabs.Items[1]).IsEnabled = true;
                });
            };
            PianoPlayer.ProgressUpdate += UpdateUI_slider_progress;
            //commented because label already updates via the event above
            //PianoPlayer.ProgressUpdate += UpdateUI_label_time;

            //apply default ui settings to the timeline
            UpdatePlayerTimeline();
        }
        // =======================================================
        private void Window_Closed(object sender, EventArgs e)
        {
            if (PianoPlayer != null) PianoPlayer.AbortPlayer();
            Environment.Exit(0);
        }

        private void Window_GotKeyboardFocus(object sender, System.Windows.Input.KeyboardFocusChangedEventArgs e)
        {
            UpdateUI_slider_progress();
            UpdateUI_label_time();
            e.Handled = true;
        }
        // -------------------------------------------------------
        private void btn_playPause_Click(object sender, RoutedEventArgs e)
        {
            PianoPlayer.ToglePlayPause();
            e.Handled = true;
        }

        private void btn_Stop_Click(object sender, RoutedEventArgs e)
        {
            PianoPlayer.Stop();
            e.Handled = true;
        }

        private void tabs_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (!PianoPlayer.Playing) UpdatePlayerTimeline();
            e.Handled = true;
        }

        private void btn_addSheet_Click(object sender, RoutedEventArgs e)
        {
            Editor_AddSheet("");
            e.Handled = true;
        }

        private void btn_removeSelSheet_Click(object sender, RoutedEventArgs e)
        {
            Editor_RemoveSelectedSheet();
            e.Handled = true;
        }

        private void slider_progress_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            UpdateUI_label_time();
            e.Handled = true;
        }

        private void slider_progress_DragDelta(object sender, System.Windows.Controls.Primitives.DragDeltaEventArgs e)
        {
            PianoPlayer.Pause();
            PianoPlayer.Time = (int)slider_progress.Value;
            e.Handled = true;
        }

        private void slider_progress_DragCompleted(object sender, System.Windows.Controls.Primitives.DragCompletedEventArgs e)
        {
            PianoPlayer.Pause();
            PianoPlayer.Time = (int)slider_progress.Value;
            e.Handled = true;
        }
        // -------------------------------------------------------
        public IO.PianoPlayerSheetFile UIInputToPPSF()
        {
            IO.PianoPlayerSheetFile ppsf = new IO.PianoPlayerSheetFile()
            {
                FileVersion = App.FileVersion,
                TimePerNote = int.Parse(QM.NumberOnlyString(edit_startTimePerNote.Text)),
                TimePerSpace = int.Parse(QM.NumberOnlyString(edit_startTimePerSpace.Text)),
                TimePerBreak = int.Parse(QM.NumberOnlyString(edit_startTimePerBreak.Text)),
                Sheets = null
            };

            List<string> sheets = new List<string>();
            foreach (TabItem sheetTab in tabs_sheets.Items)
            {
                TextBox edit_sheet = (TextBox)sheetTab.Content;
                sheets.Add(edit_sheet.Text);
            }

            ppsf.Sheets = sheets.ToArray();
            return ppsf;
        }

        public void PPSFToUIInput(IO.PianoPlayerSheetFile ppsf)
        {
            edit_startTimePerNote.Text = ppsf.TimePerNote.ToString();
            edit_startTimePerSpace.Text = ppsf.TimePerSpace.ToString();
            edit_startTimePerBreak.Text = ppsf.TimePerBreak.ToString();

            tabs_sheets.Items.Clear();
            foreach (string sheet in ppsf.Sheets) Editor_AddSheet(sheet);

            UpdatePlayerTimeline();
        }

        public void UpdatePlayerTimeline()
        {
            PianoPlayer.CurrentTimeline = (Timeline)UIInputToPPSF();
        }
        // -------------------------------------------------------
        public void Editor_AddSheet(string sheet)
        {
            TabItem tabItem = new TabItem();
            tabItem.Header = "Sheet " + (tabs_sheets.Items.Count + 1);

            TextBox edit_sheet = new TextBox();
            edit_sheet.Padding = new Thickness(5, 5, 5, 5);
            edit_sheet.AcceptsReturn = true;
            edit_sheet.AcceptsTab = true;
            edit_sheet.IsUndoEnabled = false;
            edit_sheet.FontFamily = new FontFamily("Courier New");
            edit_sheet.HorizontalScrollBarVisibility = ScrollBarVisibility.Auto;
            edit_sheet.VerticalScrollBarVisibility = ScrollBarVisibility.Auto;
            tabItem.Content = edit_sheet;

            tabs_sheets.Items.Add(tabItem);
            tabs_sheets.SelectedItem = tabItem;
        }

        public void Editor_RemoveSelectedSheet()
        {
            tabs_sheets.Items.Remove(tabs_sheets.SelectedItem);
        }
        // -------------------------------------------------------
        public void UpdateUI_btn_PlayPause()
        {
            if (Application.Current != null)
                Application.Current.Dispatcher.Invoke(() =>
                {
                    switch (PianoPlayer.Playing)
                    {
                        case true:
                            img_btn_playPause.Source = new BitmapImage
                                (new Uri(@"pack://application:,,,/Images/btn_pause.png"));
                            break;
                        case false:
                            img_btn_playPause.Source = new BitmapImage
                                (new Uri(@"pack://application:,,,/Images/btn_play.png"));
                            break;
                    }
                });
        }

        public void UpdateUI_slider_progress()
        {
            if (Application.Current != null)
                Application.Current.Dispatcher.Invoke(() =>
                {
                    slider_progress.Minimum = 0;
                    slider_progress.Maximum = PianoPlayer.CurrentTimeline.TimeLength;
                    slider_progress.Value = PianoPlayer.Time;
                });
        }

        //og. src: https://stackoverflow.com/questions/9993883/convert-milliseconds-to-human-readable-time-lapse
        public void UpdateUI_label_time()
        {
            if (Application.Current != null)
                Application.Current.Dispatcher.Invoke(() =>
                {
                    TimeSpan ct = TimeSpan.FromMilliseconds
                        (QM.ClampInt(PianoPlayer.Time, 0, PianoPlayer.CurrentTimeline.TimeLength));
                    TimeSpan t  = TimeSpan.FromMilliseconds
                        (PianoPlayer.CurrentTimeline.TimeLength);

                    label_time.Text =
                        string.Format("{0:D1}:{1:D1}:{2:D1}.{3:D3}",
                        ct.Hours, ct.Minutes, ct.Seconds, ct.Milliseconds)
                        + " / " +
                        string.Format("{0:D1}:{1:D1}:{2:D1}.{3:D3}",
                        t.Hours, t.Minutes, t.Seconds, t.Milliseconds);
                });
        }
        // =======================================================
    }
}
