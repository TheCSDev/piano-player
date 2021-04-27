using System;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Forms;
using System.Collections.Generic;
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

        private void btn_merged_merge_Click(object sender, RoutedEventArgs e)
        {
            PianoPlayerSheetFile ppsfA = new PianoPlayerSheetFile()
            {
                FileVersion = -1, //ignore this one
                TimePerNote = int.Parse(App.NumberOnlyString(edit_a_tpn.Text)),
                TimePerSpace = int.Parse(App.NumberOnlyString(edit_a_tps.Text)),
                TimePerBreak = int.Parse(App.NumberOnlyString(edit_a_tpb.Text)),
                Sheets = new string[] { edit_a_sheet.Text }
            };

            PianoPlayerSheetFile ppsfB = new PianoPlayerSheetFile()
            {
                FileVersion = -1, //ignore this one
                TimePerNote = int.Parse(App.NumberOnlyString(edit_b_tpn.Text)),
                TimePerSpace = int.Parse(App.NumberOnlyString(edit_b_tps.Text)),
                TimePerBreak = int.Parse(App.NumberOnlyString(edit_b_tpb.Text)),
                Sheets = new string[] { edit_b_sheet.Text }
            };

            PianoPlayerSheetFile ppsfM = MergePPSFs(ppsfA, ppsfB);
            if (ppsfM != null)
            {
                edit_merged_tpn.Text = ppsfM.TimePerNote.ToString();
                edit_merged_tps.Text = ppsfM.TimePerSpace.ToString();
                edit_merged_tpb.Text = ppsfM.TimePerBreak.ToString();
                edit_merged_sheet.Text = ppsfM.Sheets[0];
            }
        }

        private void btn_merged_save_Click(object sender, RoutedEventArgs e)
        {

        }
        // =======================================================
        [Serializable]
        private class MergerTimelineKey
        {
            public int Timestamp { get; private set; } //ms
            public char NoteKey { get; private set; }

            public MergerTimelineKey(int t, char note)
            {
                Timestamp = t;
                NoteKey = note;
            }
        }

        public static PianoPlayerSheetFile MergePPSFs
        (PianoPlayerSheetFile ppsfA, PianoPlayerSheetFile ppsfB)
        {
            //get sets of Player.Piano sheet instructions from both PPSFs
            List<string> ppsA = ((Player.PianoSheet)ppsfA).RemainingKeys;
            List<string> ppsB = ((Player.PianoSheet)ppsfB).RemainingKeys;

            //get time delays for each PPSF
            int ppsA_tpn = ppsfA.TimePerNote;  int ppsB_tpn = ppsfB.TimePerNote;
            int ppsA_tps = ppsfA.TimePerSpace; int ppsB_tps = ppsfB.TimePerSpace;
            int ppsA_tpb = ppsfA.TimePerBreak; int ppsB_tpb = ppsfB.TimePerBreak;

            //create a new timeline where the notes will be aligned
            List<MergerTimelineKey> timeline = new List<MergerTimelineKey>();

            //align notes from each PPSF to their timelines
            int iTimeA = 0;
            foreach (string iKeysA in ppsA)
            {
                foreach (char ch in iKeysA.ToCharArray())
                {
                    if (iKeysA.Length == 1 && char.IsWhiteSpace(ch))
                    {
                        iTimeA += ppsA_tps;
                        continue;
                    }
                    else if (iKeysA.Length == 1 && ch == '|')
                    {
                        iTimeA += ppsA_tpb;
                        continue;
                    }
                    else if(char.IsLetterOrDigit(ch)) 
                        timeline.Add(new MergerTimelineKey(iTimeA, ch));
                }
                iTimeA += ppsA_tpn;
            }

            int iTimeB = 0;
            foreach (string iKeysB in ppsB)
            {
                foreach (char ch in iKeysB.ToCharArray())
                {
                    if (iKeysB.Length == 1 && char.IsWhiteSpace(ch))
                    {
                        iTimeB += ppsB_tps;
                        continue;
                    }
                    else if (iKeysB.Length == 1 && ch == '|')
                    {
                        iTimeB += ppsB_tpb;
                        continue;
                    }
                    else if (char.IsLetterOrDigit(ch))
                        timeline.Add(new MergerTimelineKey(iTimeB, ch));
                }
                iTimeB += ppsB_tpn;
            }

            //now all notes are aligned in a timeline
            //next up: sort the timeline and convert it back to PPSF
            timeline = timeline.OrderBy(i => i.Timestamp).ToList();
            Console.WriteLine(JsonSerializer.Serialize(ppsA));

            //and finally, prepare the result
            PianoPlayerSheetFile ppsfR = new PianoPlayerSheetFile()
            {
                FileVersion = 1,
                TimePerNote = 150,
                TimePerSpace = 150,
                TimePerBreak = 400,
                Sheets = new string[] { "" }
            };

            //and return the result
            return ppsfR;
        }
        // =======================================================
    }
}
