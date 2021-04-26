using System;

namespace Piano_Player
{
    [Serializable]
    public class PianoPlayerSheetFile
    {
        public int FileVersion { get; set; }


        private int _noteTime = 150, _spaceTime = 150, _breakTime = 400; //ms
        public int TimePerNote
        {
            get { return _noteTime; }
            set { _noteTime = App.ClampInt(value, 10, 10000); }
        }
        public int TimePerSpace
        {
            get { return _spaceTime; }
            set { _spaceTime = App.ClampInt(value, 10, 10000); }
        }
        public int TimePerBreak
        {
            get { return _breakTime; }
            set { _breakTime = App.ClampInt(value, 10, 10000); }
        }

        public string[] Sheets { get; set; }

        public PianoPlayerSheetFile()
        {
            FileVersion = App.FileVersion;
            TimePerNote = 150;
            TimePerSpace = 150;
            TimePerBreak = 400;
            Sheets = new string[] { "" };
        }
    }
}
