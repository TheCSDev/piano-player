using System;

namespace Piano_Player.IO
{
    [Serializable]
    public class PianoPlayerSheetFile
    {
        // =======================================================
        public int FileVersion { get; set; }


        public int TimePerNote { get; set; }
        public int TimePerSpace { get; set; }
        public int TimePerBreak { get; set; }

        public string[] Sheets { get; set; }
        // =======================================================
        public PianoPlayerSheetFile()
        {
            FileVersion = App.FileVersion;
            TimePerNote = 150;
            TimePerSpace = 150;
            TimePerBreak = 400;
            Sheets = new string[] { "" };
        }
        // =======================================================
    }
}
