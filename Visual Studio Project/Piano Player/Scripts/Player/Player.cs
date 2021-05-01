using System;
using System.Threading;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Windows;
using WindowsInput.Native;
using Piano_Player.IO;

namespace Piano_Player.Player
{
    [Obsolete]
    public class Player
    {
        // =======================================================
        //yes, the space (" ") is important as well, do not remove it
        //and the character order is important as well
        public const string SpecialCharKeys = "!@ $%^ *(";
        public const string SpecialCharKeysShift = "12 456 89";
        public const char PlayerCommandPrefix = '_';
        // =======================================================
        public readonly MainWindow parentWindow;
        //allows the new executed thread to access this class
        public readonly Player ThisPlayer;
        //when set to false, the new thread stops, and the player becomes useless
        //the owner window modifies this variable
        public bool PlayerAlive { get; set; } = true;
        //the owner window modifies this variable
        public bool IsMainWindowKeyboardFocused = true;
        //returns true if no notes were played last "frame" because
        //the player was either paused or the window was in focus
        public bool PausedLastFrame { get; private set; } = true;

        public PlayerInputHandler inputHandler { get; private set; }

        public int PlayerProgress { get; private set; } = 0;
        public delegate void PlayerProgressChangedHandler();
        public event PlayerProgressChangedHandler PlayerProgressChanged;

        public Player(MainWindow parentWindow)
        {
            this.parentWindow = parentWindow;
            ThisPlayer = this;
            PlayerAlive = true;
            inputHandler = new PlayerInputHandler(this);

            Thread player_thread = new Thread(() =>
            {
                while (Application.Current == null) { Thread.Sleep(100); }
                while (PlayerAlive)
                {
                    //wait a little bit to prevend 1000s of commanands
                    //being executed per second
                    if (PausedLastFrame) { Thread.Sleep(250); }

                    //Check if the main window is in focus, and if it is,
                    //don't do anything.
                    if (IsMainWindowKeyboardFocused) { PausedLastFrame = true; continue; }

                    //Also don't do anything if the player isn't playing
                    if (!ThisPlayer.IsPlaying) { PausedLastFrame = true;  continue; }

                    //if the player was paused last "frame", wait a little
                    //because of a "bug" that'd occur without the wait
                    if (PausedLastFrame)
                    {
                        PausedLastFrame = false;
                        Thread.Sleep(250);
                    }

                    //Press the keys
                    if (ThisPlayer.CurrentSheet.RemainingKeys.Count > 0)
                    {
                        string keys = ThisPlayer.CurrentSheet.RemainingKeys[0];
                        ThisPlayer.CurrentSheet.RemainingKeys.RemoveAt(0);

                        //check if there is a command that needs to be executed
                        if (keys.StartsWith("" + PlayerCommandPrefix) && keys.Length > 1)
                        {
                            keys = keys.Substring(1);

                            //wait command
                            if (keys.StartsWith("w "))
                            {
                                keys = keys.Substring(2);
                                try { Thread.Sleep(App.ClampInt(int.Parse(keys), 10, 10000)); }
                                catch (Exception) { }
                            }
                        }
                        //if not, continue pressing the keys
                        else
                        foreach (char ch in keys)
                        {
                            //update 1.2: breaks are ignored in note groups
                            //aka breaks only work when keys length is 1
                            if (ch == ' ') { if (keys.Length == 1) Thread.Sleep(SpaceTime); }
                            else if (ch == '|') { if (keys.Length == 1) Thread.Sleep(BreakTime); }
                            else if (ch == PlayerCommandPrefix) { /*do nothing*/ }
                            else
                            {
                                if (SpecialCharKeys.Contains("" + ch))
                                {
                                    inputHandler.KeyDown(VirtualKeyCode.LSHIFT);
                                    inputHandler.KeyPress((VirtualKeyCode)SpecialCharKeysShift[SpecialCharKeys.IndexOf("" + ch)]);
                                    inputHandler.KeyUp(VirtualKeyCode.LSHIFT);
                                }
                                else
                                {
                                    if (char.IsUpper(ch))
                                        inputHandler.KeyDown(VirtualKeyCode.LSHIFT);

                                    inputHandler.KeyPress((VirtualKeyCode)char.ToUpper(ch));

                                    if (char.IsUpper(ch))
                                        inputHandler.KeyUp(WindowsInput.Native.VirtualKeyCode.LSHIFT);
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
            public static explicit operator PianoSheet(PianoPlayerSheetFile a)
            {
                try { return new PianoSheet(a.Sheets[0]); }
                catch (Exception) { return new PianoSheet(""); }
            }

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

                //make sure the command prefix doesnt conflict with the
                //special keys
                if ("[]| !@$^*(".Contains(PlayerCommandPrefix) ||
                    char.IsLetterOrDigit(PlayerCommandPrefix))
                {
                    ErrorWindow.ShowExceptionWindow(
                        "Invalid player command prefix: \"" + PlayerCommandPrefix + "\".",
                        new Exception(
                            "Player command prefix must not be one " +
                            "of the following characters:\n\"[]| !@$^*(\"\n" +
                            "And it also must not be a letter or a digit."));
                    Environment.Exit(0);
                }

                //Convert the data from input_sheet to FullSheet

                //first off, deal with new line characters
                //line breaks are replaced with "||"
                //the rest of the new line characters are repaced with " "
                input_sheet = Regex.Replace(input_sheet, @"^\s+$[\r\n]*", "||", RegexOptions.Multiline);
                input_sheet = input_sheet.Replace("\n", " ");

                //remove double spaces
                input_sheet = Regex.Replace(input_sheet, @"[ ]{2,}", " ", RegexOptions.None);
                //"fix" the bug where the last note isn't played
                //(what a lazy way to fix a bug...)
                input_sheet += " ";

                bool grouped = false;
                string next_notes = "";
                foreach (char ch in input_sheet)
                {
                    //Skip all characters besides the supported characters
                    if (!char.IsLetterOrDigit(ch) && !("[]| !@$^*("+PlayerCommandPrefix).Contains(""+ch)) continue;

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
                        //
                        //edit: in update 1.2, space in note groups will be allowed,
                        //and the player will now ignore spaces in grouped notes.
                        //space in grouped notes will be used for player "commands"
                        /*if (next_notes.Contains(" ")) //if the type is 2nd, handle it
                        {
                            next_notes = next_notes.Replace(" ", "");
                            foreach (char i in next_notes) FullSheet.Add("" + i);
                            next_notes = "";
                        }*/

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
        private PianoSheet _CurrentSheet = new PianoSheet("");
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
        private int _noteTime = 150, _spaceTime = 150, _breakTime = 400; //ms
        public int NoteTime
        {
            get { return _noteTime; }
            set { _noteTime = App.ClampInt(value, 10, 10000); }
        }
        public int SpaceTime
        {
            get { return _spaceTime; }
            set { _spaceTime = App.ClampInt(value, 10, 10000); }
        }
        public int BreakTime
        {
            get { return _breakTime; }
            set { _breakTime = App.ClampInt(value, 10, 10000); }
        }
        // -------------------------------------------------------
        public bool IsPlaying { get; private set; } = false;
        public delegate void PlayStateChangedHandler();
        public event PlayStateChangedHandler PlayStateChanged;
        // =======================================================
        public void Player_Play()
        {
            inputHandler.RefreshData();
            inputHandler.DebugPlayerHelper();
            IsPlaying = true;
            PlayStateChanged();
        }
        public void Player_Pause()
        {
            if(inputHandler.javaHelperProcess != null && !inputHandler.javaHelperProcess.HasExited)
                inputHandler.javaHelperProcess.Kill();
            IsPlaying = false;
            PlayStateChanged();
        }
        public void Player_Toggle() { if (!IsPlaying) Player_Play(); else Player_Pause(); }
        public void Player_Stop()
        {
            //first update the variables
            Player_Pause();
            PlayerProgress = 0;
            //then reset
            CurrentSheet.Reset();
            //and only then can you trigger the event
            PlayStateChanged();
        }
        // =======================================================
    }
}
