using System;
using System.IO;
using System.Windows;
using System.Windows.Forms;
using System.Text.Json;

namespace Piano_Player
{
    /// <summary>
    /// Interaction logic for SheetMergerWindow.xaml
    /// </summary>
    public partial class SheetMergerWindow : Window
    {
        // =======================================================
        public readonly MainWindow ParentWindow;
        // =======================================================
        public SheetMergerWindow(MainWindow parent)
        {
            ParentWindow = parent;
            InitializeComponent();
        }

        private PianoPlayerSheetFile ShowLoadDialog()
        {
            OpenFileDialog ofd = new OpenFileDialog();
            ofd.Filter = "Piano Player Sheet Files|*.ppsf";
            ofd.RestoreDirectory = true;

            if (ofd.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                try
                {
                    return JsonSerializer.Deserialize<PianoPlayerSheetFile>
                        (File.ReadAllText(ofd.FileName));
                }
                catch (Exception e)
                {
                    ErrorWindow.ShowExceptionWindow
                        ("Failed to open file: \"" + ofd.FileName + "\"", e);
                }
            }

            return null;
        }
        // =======================================================
        private void btn_a_load_Click(object sender, RoutedEventArgs e)
        {
            PianoPlayerSheetFile ppsf = ShowLoadDialog();
            if (ppsf != null)
            {
                edit_a_tpn.Text = ppsf.TimePerNote.ToString();
                edit_a_tps.Text = ppsf.TimePerSpace.ToString();
                edit_a_tpb.Text = ppsf.TimePerBreak.ToString();
                try { edit_a_sheet.Text = ppsf.Sheets[0]; }
                catch (Exception) { edit_a_sheet.Text = ""; }
            }
        }

        private void btn_b_load_Click(object sender, RoutedEventArgs e)
        {
            PianoPlayerSheetFile ppsf = ShowLoadDialog();
            if (ppsf != null)
            {
                edit_b_tpn.Text = ppsf.TimePerNote.ToString();
                edit_b_tps.Text = ppsf.TimePerSpace.ToString();
                edit_b_tpb.Text = ppsf.TimePerBreak.ToString();
                try { edit_b_sheet.Text = ppsf.Sheets[0]; }
                catch (Exception) { edit_b_sheet.Text = ""; }
            }
        }

        private void btn_merged_save_Click(object sender, RoutedEventArgs e)
        {

        }
        // =======================================================
    }
}
