using System;
using System.Threading;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Windows;
using WindowsInput.Native;

namespace Piano_Player
{
    public class Player
    {
        // =======================================================
        //yes, the space (" ") is important as well, do not remove it
        //and the character order is important as well
        public const string SpecialCharKeys = "!@ $%^ *(";
        public const string SpecialCharKeysShift = "12 456 89";
        // =======================================================
        public readonly MainWindow parentWindow;
        //allows the new executed thread to access this class
        public readonly Player ThisPlayer;
        //when set to false, the new threaad stops, and the player becomes useless
        //the owner window modifies this variable
        public bool PlayerAlive = true;
        //the owner window modifies this variable
        public bool IsMainWindowKeyboardFocused = true;

        public PlayerInputHandler inputSimulator { get; private set; }

        public int PlayerProgress { get; private set; } = 0;
        public delegate void PlayerProgressChangedHandler();
        public event PlayerProgressChangedHandler PlayerProgressChanged;

        public Player(MainWindow parentWindow)
        {
            this.parentWindow = parentWindow;
            ThisPlayer = this;
            inputSimulator = new PlayerInputHandler(this);

            Thread player_thread = new Thread(() =>
            {
                while (Application.Current == null) { Thread.Sleep(100); }

                while (PlayerAlive)
                {
                    //Check if the main window is in focus, and if it is,
                    //don't do anything.
                    bool focus = IsMainWindowKeyboardFocused;
                    if (focus) { continue; }

                    //Also don't do anything if the player isn't playing
                    if (!ThisPlayer.IsPlaying) { continue; }

                    //Press the keys
                    if (ThisPlayer.CurrentSheet.RemainingKeys.Count > 0)
                    {
                        string keys = ThisPlayer.CurrentSheet.RemainingKeys[0];
                        ThisPlayer.CurrentSheet.RemainingKeys.RemoveAt(0);

                        foreach (char ch in keys)
                        {
                            if (ch == ' ') Thread.Sleep(SpaceTime);
                            else if (ch == '|') Thread.Sleep(BreakTime);
                            else
                            {
                                if (SpecialCharKeys.Contains("" + ch))
                                {
                                    inputSimulator.KeyDown(VirtualKeyCode.LSHIFT);
                                    inputSimulator.KeyPress((VirtualKeyCode)SpecialCharKeysShift[SpecialCharKeys.IndexOf("" + ch)]);
                                    inputSimulator.KeyUp(VirtualKeyCode.LSHIFT);
                                }
                                else
                                {
                                    if (char.IsUpper(ch))
                                        inputSimulator.KeyDown(VirtualKeyCode.LSHIFT);

                                    inputSimulator.KeyPress((VirtualKeyCode)char.ToUpper(ch));

                                    if (char.IsUpper(ch))
                                        inputSimulator.KeyUp(WindowsInput.Native.VirtualKeyCode.LSHIFT);
                                }
                            }

                            PlayerProgress = 100 - (int)Math.Round(((float)ThisPlayer.CurrentSheet.RemainingKeys.Count / (float)ThisPlayer.CurrentSheet.FullSheet.Count) * 100);
                            PlayerProgressChanged();
                        }

                        if(!keys.Contains(' ') && !keys.Contains('|'))
                            Thread.Sleep(ThisPlayer.NoteTime);
                    }
                    else Player_Stop();

                    Thread.Sleep(1);
                }
            });
            player_thread.IsBackground = true;
            player_thread.Start();
        }
        // =======================================================
        public class PianoSheet
        {
            /* A sheet is a list of strings.
             * Each string in a sheet (list) represents a set of notes
             * that need to be pressed at once per beat.
             * Usually it is one note per beat but sometimes it is multiple
             * notes, hence the list of strings instead of a list of chars.
             * The characters (A-Z)(a-z)(0-9) represent notes.
             * The character "|" represents a break, it's wait time is defined
             * by the BreakTime variable.
             * The characters "[" and "]" are used to specify that multiple
             * notes need to be perssed at once (for ex. "[wro]").
             * A space " " represents a break (usually shorter than the "|"),
             * it's wait time is defined by the Tempo variable.
             * All other characters are invalid or unsupported by this player.
             */
            public string RawSheet { get; private set; }
            public List<string> FullSheet { get; private set; }
            public List<string> RemainingKeys { get; private set; }

            public PianoSheet(string input_sheet)
            {
                RawSheet = input_sheet;
                FullSheet = new List<string>();
                RemainingKeys = new List<string>();

                //Convert the data from input_sheet to FullSheet

                //first off, deal with new line characters
                //line breaks are replaced with "||"
                //the rest of the new line characters are repaced with " "
                input_sheet = Regex.Replace(input_sheet, @"^\s+$[\r\n]*", "||", RegexOptions.Multiline);
                input_sheet = input_sheet.Replace("\n", " ");

                //remove double spaces
                input_sheet = Regex.Replace(input_sheet, @"[ ]{2,}", " ", RegexOptions.None);
                //fix the bug where the last note isn't played
                input_sheet += " ";

                bool grouped = false;
                string next_notes = "";
                foreach (char ch in input_sheet)
                {
                    //Skip all characters besides the supported characters
                    if (!char.IsLetterOrDigit(ch) && !"[]| !@$^*(".Contains(""+ch)) continue;

                    //Move on with the complicated process
                    if (!grouped && next_notes.Length > 0)
                    {
                        FullSheet.Add("" + next_notes);
                        next_notes = "";
                    }

                    if (ch == '[') grouped = true;
                    else if (ch == ']')
                    {
                        //Make sure there are no errors and invalid characters that
                        //can end up there such as "|".
                        next_notes = next_notes.Replace("|", "");

                        //There are 2 types of grouped notes:
                        //1st one is [abc] which means play all 3 of them at once
                        //2nd one is [a b c] which means play those notes fast 1 by one
                        if (next_notes.Contains(" ")) //if the type is 2nd, handle it
                        {
                            next_notes = next_notes.Replace(" ", "");
                            foreach (char i in next_notes) FullSheet.Add("" + i);
                            next_notes = "";
                        }

                        //Conclude character grouping
                        grouped = false;
                    }
                    else next_notes += ch;
                }

                //And finally, copy the data to the RemainingKeys
                Reset();
            }

            public void Reset() { RemainingKeys = new List<string>(FullSheet); }
        }
        //
        private PianoSheet _CurrentSheet = new PianoSheet
                 ("t|y|[qp]|\r\n"
                + "o o|i|[wo]|\r\n"
                + "i i|u|[ei]|\r\n"
                + "u u|y|[ru]|\r\n"
                + "y y|t|r|t|[pq2]|\r\n"
                + "o o|i|[ow3]|\r\n"
                + "i i|u|[ie4]|\r\n"
                + "u u|y|[ur5]|\r\n"
                + "y y|t|r|t|[pq2]\r\n"
                + "u i o o|i|[ow3]\r\n"
                + "y u i i|u|[ie4]\r\n"
                + "t y u u|y|[ur5]\r\n"
                + "r t y y|t||[pq2]||||o||||[p02]| |\r\n"
                + "i|u|[ow3]||||i||||[or5]| |\r\n"
                + "i|u|[ow] |\r\n"
                + "i|u|[ie] |\r\n"
                + "u|y|[ur] |\r\n"
                + "y|||[q2t]");
        public PianoSheet CurrentSheet
        {
            get { return _CurrentSheet; }
            set
            {
                IsPlaying = false;
                _CurrentSheet = value;
                Player_Stop();
            }
        }
        // -------------------------------------------------------
        public int NoteTime { get; set; } = 150; //ms
        public int SpaceTime { get; set; } = 150; //ms
        public int BreakTime { get; set; } = 400; //ms
        // -------------------------------------------------------
        public bool IsPlaying { get; private set; } = false;
        public delegate void PlayStateChangedHandler();
        public event PlayStateChangedHandler PlayStateChanged;
        // =======================================================
        public void Player_Play()
        {
            inputSimulator.RefreshData();
            inputSimulator.DebugPlayerHelper();
            IsPlaying = true;
            PlayStateChanged();
        }
        public void Player_Pause() { IsPlaying = false; PlayStateChanged(); }
        public void Player_Toggle()
        {
            inputSimulator.RefreshData();
            if (!IsPlaying) Player_Play(); else Player_Pause();
        }
        public void Player_Stop()
        {
            //first update the variables
            IsPlaying = false;
            PlayerProgress = 0;
            //then reset
            CurrentSheet.Reset();
            //and only then can you trigger the event
            PlayStateChanged();
        }
        // =======================================================
    }
}
