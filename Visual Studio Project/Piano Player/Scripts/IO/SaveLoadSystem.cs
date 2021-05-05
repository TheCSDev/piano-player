using System;
using System.IO;
using System.Text.Json;
using System.Windows.Forms;

namespace Piano_Player.IO
{
    public class SaveLoadSystem
    {
        // =======================================================
        public readonly MainWindow ParentWindow;
        // -------------------------------------------------------
        public string FilePath { get; set; }
        public bool   ChangesSaved { get; set; }
        // =======================================================
        public SaveLoadSystem(MainWindow parentWindow)
        {
            ParentWindow = parentWindow;
            FilePath     = null;
            ChangesSaved = true;
        }
        // -------------------------------------------------------
        // =======================================================
        public void NewFile(bool showSaveDialog = true)
        {
            ParentWindow.PianoPlayer.Stop();

            if (showSaveDialog && !ChangesSaved)
            {
                int r = QM.YesNoCancelDialog
                    ("Save changes made to the current sheet?", "Piano Player");
                if (r == 1) SaveFile();
                else if (r == 3) return;
            }

            ParentWindow.tabs_sheets.Items.Clear();
            ParentWindow.edit_startTimePerNote.Text = "150";
            ParentWindow.edit_startTimePerSpace.Text = "150";
            ParentWindow.edit_startTimePerBreak.Text = "400";

            ParentWindow.UpdatePlayerTimeline();
            ChangesSaved = true;
        }
        // -------------------------------------------------------
        public bool OpenFile(bool showSaveDialog = true)
        {
            ParentWindow.PianoPlayer.Stop();

            if (showSaveDialog && !ChangesSaved)
            {
                int r = QM.YesNoCancelDialog
                    ("Save changes made to the current sheet?", "Piano Player");
                if (r == 1) SaveFile();
                else if (r == 3) return false;
            }

            OpenFileDialog ofd = new OpenFileDialog();
            ofd.Filter = "Piano Player Sheet Files|*.ppsf";
            ofd.RestoreDirectory = true;

            if (ofd.ShowDialog() == DialogResult.OK) return LoadFile(ofd.FileName);
            return false;
        }
        // -------------------------------------------------------
        public bool LoadFile(string filePath)
        {
            ParentWindow.PianoPlayer.Stop();

            if (!filePath.EndsWith(App.FileExtension) || !File.Exists(filePath)) return false;
            try
            {
                PianoPlayerSheetFile ppsf = JsonSerializer.Deserialize
                    <PianoPlayerSheetFile>(File.ReadAllText(filePath));

                FilePath = filePath;
                ChangesSaved = true;
                ParentWindow.PPSFToUIInput(ppsf);
                ParentWindow.UpdatePlayerTimeline();

                ParentWindow.tabs.SelectedItem = ParentWindow.tabs.Items[1];
                ChangesSaved = true;
                return true;
            }
            catch (Exception e)
            {
                ErrorWindow.ShowExceptionWindow("Failed to open file: \"" + filePath + "\"", e);
                return false;
            }
        }
        // -------------------------------------------------------
        public bool SaveFile()
        {
            ParentWindow.PianoPlayer.Stop();

            if (string.IsNullOrWhiteSpace(FilePath) ||
                string.IsNullOrEmpty(FilePath)) { return SaveFileAs(); }

            try
            {
                PianoPlayerSheetFile ppsf = ParentWindow.UIInputToPPSF();

                File.Delete(FilePath);
                File.WriteAllText(FilePath, JsonSerializer.Serialize(ppsf));

                ChangesSaved = true;
                return true;
            }
            catch (Exception e)
            {
                ErrorWindow.ShowExceptionWindow
                    ("Failed to save file: \"" + FilePath + "\"", e);
                return false;
            }
        }
        // -------------------------------------------------------
        public bool SaveFileAs()
        {
            ParentWindow.PianoPlayer.Stop();

            try
            {
                SaveFileDialog sfd = new SaveFileDialog();
                sfd.Filter = "Piano Player Sheet Files|*.ppsf";
                sfd.RestoreDirectory = true;

                if (sfd.ShowDialog() == DialogResult.OK)
                {
                    //always set the path first and then save it
                    //not doing so results in stackoverflow
                    FilePath = sfd.FileName;
                    return SaveFile();
                }

                return false;
            }
            catch (Exception e)
            {
                ErrorWindow.ShowExceptionWindow
                    ("Failed to save file: \"" + FilePath + "\"", e);
                return false;
            }
        }
        // -------------------------------------------------------
        // =======================================================
    }
}
