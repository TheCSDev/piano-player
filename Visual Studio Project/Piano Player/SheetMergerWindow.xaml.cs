﻿using System;
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
            PianoPlayerSheetFile ppsf = GetMergedPPSF();

            SaveFileDialog sfd = new SaveFileDialog();
            sfd.Filter = "Piano Player Sheet Files|*.ppsf";
            sfd.RestoreDirectory = true;

            if (sfd.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                if (File.Exists(sfd.FileName)) File.Delete(sfd.FileName);
                File.WriteAllText(sfd.FileName, JsonSerializer.Serialize<PianoPlayerSheetFile>(ppsf));
            }
        }

        private void btn_merged_play_Click(object sender, RoutedEventArgs e)
        {
            DialogResult dr = ParentWindow.SheetSaveLoad.ShowConfirmationDialog();
            if (dr == System.Windows.Forms.DialogResult.Cancel) return;
            else if (dr == System.Windows.Forms.DialogResult.Yes)
                if (!ParentWindow.SheetSaveLoad.SaveFile()) return;

            try
            {
                ParentWindow.SheetSaveLoad.LoadFile_EXC(GetMergedPPSF());
                Hide();
            }
            catch (Exception ex)
            {
                ErrorWindow.ShowExceptionWindow("Unable to play merged sheet.", ex);
            }
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            e.Cancel = true;
            Hide();
        }
        // =======================================================
        [Serializable]
        private class MergerTimelineKey
        {
            public int Timestamp { get; set; } //ms
            public string NoteKeys { get; set; }

            public MergerTimelineKey(int t, char note)
            {
                Timestamp = t;
                NoteKeys = note.ToString();
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
                bool notesAdded = false;

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
                    else if (char.IsLetterOrDigit(ch))
                    {
                        MergerTimelineKey mtk = timeline.Find(i => i.Timestamp == iTimeA);
                        if (mtk != null) mtk.NoteKeys += ch;
                        else timeline.Add(new MergerTimelineKey(iTimeA, ch));
                        notesAdded = true;
                    }
                }

                if(notesAdded) iTimeA += ppsA_tpn;
            }

            int iTimeB = 0;
            foreach (string iKeysB in ppsB)
            {
                bool notesAdded = false;

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
                    {
                        MergerTimelineKey mtk = timeline.Find(i => i.Timestamp == iTimeB);
                        if (mtk != null) mtk.NoteKeys += ch;
                        else timeline.Add(new MergerTimelineKey(iTimeB, ch));
                        notesAdded = true;
                    }
                }

                if (notesAdded) iTimeB += ppsB_tpn;
            }

            //now all notes are aligned in a timeline
            //next up: sort the timeline and convert it back to PPSF
            timeline = timeline.OrderBy(i => i.Timestamp).ToList();

            //convert timeline keys's timestamps to "wait numbers"
            if (timeline.Count > 1)
            for (int index = timeline.Count - 1; index > 1; index--)
            {
                timeline[index].Timestamp -= timeline[index - 1].Timestamp;
            }

            //convert the timeline back into a sheet
            string finalSheet = "";

            foreach (MergerTimelineKey mtk in timeline)
            {
                if (mtk.Timestamp > 0)
                    finalSheet += "["+Player.PlayerCommandPrefix+"w "+mtk.Timestamp+"]";
                finalSheet += mtk.NoteKeys.Length == 1 ? mtk.NoteKeys : ("["+mtk.NoteKeys+"]");
            }

            //and finally, prepare the result
            PianoPlayerSheetFile ppsfR = new PianoPlayerSheetFile()
            {
                FileVersion = -1, //ignore this one
                TimePerNote = 10,
                TimePerSpace = 10,
                TimePerBreak = 10,
                Sheets = new string[] { finalSheet }
            };

            //and return the result
            return ppsfR;
        }

        public PianoPlayerSheetFile GetMergedPPSF()
        {
            return new PianoPlayerSheetFile()
            {
                FileVersion = 1,
                TimePerNote = 10,
                TimePerSpace = 10,
                TimePerBreak = 10,
                Sheets = new string[] { edit_merged_sheet.Text }
            };
        }
        // =======================================================
    }
}
