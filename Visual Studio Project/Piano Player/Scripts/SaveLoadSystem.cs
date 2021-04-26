using System;
using System.Windows.Forms;
using System.IO;
using System.Text.Json;

namespace Piano_Player
{
    public class SaveLoadSystem
    {
        // =======================================================
        public MainWindow ParentWindow { get; private set; }
        public bool ChangesSaved { get; set; }
        public string SaveFilePath { get; private set; }
        // =======================================================
        public SaveLoadSystem(MainWindow parentWindow)
        {
            ParentWindow = parentWindow;
            ChangesSaved = true;
            SaveFilePath = "";
        }
        // =======================================================
        public void NewFile()
        {
            ParentWindow.PianoPlayer.Player_Pause();

            DialogResult dr = ShowConfirmationDialog();
            if (dr == DialogResult.Cancel) return;
            else if (dr == DialogResult.Yes) if (!SaveFile()) return;

            ParentWindow.edit_sheets.Text = "";
            ParentWindow.edit_timePerNote.Text = "150";
            ParentWindow.edit_timePerSpace.Text = "150";
            ParentWindow.edit_timePerBreak.Text = "400";

            SaveFilePath = "";
            ChangesSaved = true;
        }
        public bool OpenFile()
        {
            ParentWindow.PianoPlayer.Player_Pause();

            DialogResult dr = ShowConfirmationDialog();
            if (dr == DialogResult.Cancel) return false;
            else if (dr == DialogResult.Yes) if (!SaveFile()) return false;

            OpenFileDialog ofd = new OpenFileDialog();
            ofd.Filter = "Piano Player Sheet Files|*.ppsf";
            ofd.RestoreDirectory = true;

            if (ofd.ShowDialog() == DialogResult.OK) return LoadFile(ofd.FileName);
            return false;
        }
        public bool SaveFile()
        {
            ParentWindow.PianoPlayer.Player_Pause();

            try
            {
                if (SaveFilePath == null || SaveFilePath.Length == 0) return SaveFileAs();

                PianoPlayerSheetFile ppsf = new PianoPlayerSheetFile();
                ppsf.FileVersion = App.FileVersion;
                ppsf.TimePerNote = ParentWindow.PianoPlayer.NoteTime;
                ppsf.TimePerSpace = ParentWindow.PianoPlayer.SpaceTime;
                ppsf.TimePerBreak = ParentWindow.PianoPlayer.BreakTime;
                ppsf.Sheets = new string[] { ParentWindow.PianoPlayer.CurrentSheet.RawSheet };
                
                File.Delete(SaveFilePath);
                File.WriteAllText(SaveFilePath, JsonSerializer.Serialize(ppsf));

                ChangesSaved = true;
                return true;
            }
            catch (Exception e)
            {
                ErrorWindow.ShowExceptionWindow("Failed to save file: \"" + SaveFilePath + "\"", e);
                return false;
            }
        }
        public bool SaveFileAs()
        {
            ParentWindow.PianoPlayer.Player_Pause();

            try
            {
                SaveFileDialog sfd = new SaveFileDialog();
                sfd.Filter = "Piano Player Sheet Files|*.ppsf";
                sfd.RestoreDirectory = true;

                if (sfd.ShowDialog() == DialogResult.OK)
                {
                    //always set the path first and then save it
                    //not doing so results in stackoverflow
                    SaveFilePath = sfd.FileName;
                    return SaveFile();
                }

                return false;
            }
            catch (Exception e)
            {
                ErrorWindow.ShowExceptionWindow("Failed to save file: \"" + SaveFilePath + "\"", e);
                return false;
            }
        }
        public bool LoadFile(string filePath)
        {
            ParentWindow.PianoPlayer.Player_Pause();

            if (!filePath.EndsWith(App.FileExtension) || !File.Exists(filePath)) return false;
            try
            {
                PianoPlayerSheetFile ppsf = JsonSerializer.Deserialize
                    <PianoPlayerSheetFile>(File.ReadAllText(filePath));

                if (ppsf.FileVersion != App.FileVersion)
                    throw new Exception("This version of Piano Player is unable to " +
                        "load ppsf files with file versions that aren't: " + App.FileVersion +
                        "\nThe chosen file's version is: " + ppsf.FileVersion);

                if(ppsf.Sheets != null && ppsf.Sheets.Length > 0)
                    ParentWindow.edit_sheets.Text = ppsf.Sheets[0];
                ParentWindow.edit_timePerNote.Text = "" + ppsf.TimePerNote;
                ParentWindow.edit_timePerSpace.Text = "" + ppsf.TimePerSpace;
                ParentWindow.edit_timePerBreak.Text = "" + ppsf.TimePerBreak;

                SaveFilePath = filePath;
                ChangesSaved = true;
                return true;
            }
            catch (Exception e)
            {
                ErrorWindow.ShowExceptionWindow("Failed to open file: \"" + filePath + "\"", e);
                return false;
            }
        }
        // =======================================================
        public void OnChangesMade()
        {
            ParentWindow.PianoPlayer.Player_Pause();
            ChangesSaved = false;
        }

        public DialogResult ShowConfirmationDialog()
        {
            if (ChangesSaved) return DialogResult.None;
            return MessageBox.Show("Save changes made to the current sheet?",
                "Piano Player", MessageBoxButtons.YesNoCancel);
        }
        // =======================================================
    }
}
