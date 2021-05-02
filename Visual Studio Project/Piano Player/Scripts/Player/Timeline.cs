using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using Piano_Player.IO;

namespace Piano_Player.Player
{
    public class Timeline
    {
        // =======================================================
        public const string ValidNoteCharacters   = "1234567890qwertyuiopasdfghjklzxcvbnm!@$%^*(";
        //warning: special character order is very important (SHIFT+(1 to 9))
        public const string NumberShiftCharacters = "!@#$%^&*()";
        
        public class Keyframe
        {
            private int _timestamp; //ms
            private char _noteKey;

            public int Timestamp
            {
                get { return _timestamp; }
                set
                {
                    if (value < 0) _timestamp = 0;
                    else _timestamp = value;
                }
            } //ms
            public char NoteKey
            {
                get { return _noteKey; }
                set
                {
                    if (ChValid(value)) _noteKey = value;
                    else _noteKey = '\0';
                }
            }

            /// <summary>
            /// Keyframe does not automatically add itself to the parent timeline.
            /// That has to be done manually.
            /// </summary>
            /// <exception cref="ArgumentNullException"></exception>
            public Keyframe(int timestamp, char noteKey)
            {
                Timestamp = timestamp;
                NoteKey = noteKey;
            }
        }

        public enum PlayerAction { KeyPress, Sleep, Stop }
        // =======================================================
        private List<Keyframe> Keyframes { get; set; }
        public int KeyframeCount { get { return Keyframes.Count; } }
        // =======================================================
        public Timeline()
        {
            Keyframes = new List<Keyframe>();
        }
        // -------------------------------------------------------
        public bool AddKeyframe(Keyframe key)
        {
            if (key == null) return false;
            Keyframes.Add(key);
            return true;
        }

        public bool RemoveKeyframe(int index)
        {
            Keyframe k = null;
            try { k = Keyframes[index]; } catch (Exception) { return false; }
            return Keyframes.Remove(k);
        }

        public bool RemoveKeyframe(Keyframe key) { return Keyframes.Remove(key); }

        public int RemoveAllKeyframes() { return Keyframes.RemoveAll(i => true); }

        public Keyframe GetKeyframe(int index)
        {
            try { return Keyframes[index]; }
            catch (Exception) { return null; }
        }

        public int IndexOfKeyframe(Keyframe key) { return Keyframes.IndexOf(key); }
        // -------------------------------------------------------
        public void GetPlayerAction
        (int timestamp, out PlayerAction playerAction, out string[] args)
        {
            playerAction = PlayerAction.Stop;
            args = null;

            if (KeyframeCount == 0) return;

            Keyframe /*closestSmaller = null,*/ closestBigger = null;
            string keysToPress = "";

            foreach (Keyframe keyframe in Keyframes)
            {
                /*if ((closestSmaller == null && keyframe.Timestamp < timestamp) ||
                  (keyframe.Timestamp > closestSmaller.Timestamp && keyframe.Timestamp < timestamp))
                    closestSmaller = keyframe;

                else*/ if ((closestBigger == null && keyframe.Timestamp > timestamp) ||
                  (keyframe.Timestamp < closestBigger.Timestamp && keyframe.Timestamp > timestamp))
                    closestBigger = keyframe;

                if (keyframe.Timestamp == timestamp && ChValid(keyframe.NoteKey))
                    keysToPress += keyframe.NoteKey;


                if (keysToPress.Length > 0)
                {
                    playerAction = PlayerAction.KeyPress;
                    args = new string[] { keysToPress };
                    return;
                }
                else if (keysToPress.Length == 0 && closestBigger != null)
                {
                    playerAction = PlayerAction.Sleep;
                    args = new string[] { "" + (closestBigger.Timestamp - timestamp) };
                    return;
                }
                else if (closestBigger == null)
                {
                    playerAction = PlayerAction.Stop;
                    args = null;
                    return;
                }
            }
        }
        // -------------------------------------------------------
        public static bool ChValid(char ch)
        {
            return ValidNoteCharacters.ToLower().Contains
                (char.ToLower(ch).ToString()) && ch != '\0';
        }
        // =======================================================
        public static explicit operator Timeline(PianoPlayerSheetFile ppsf)
        {
            //create a timeline that will be returned in the end
            Timeline result = new Timeline();

            //convert ppsf sheet strings into arrays of instructions
            List<List<string>> instructions = new List<List<string>>();

            foreach (string sheet in ppsf.Sheets)
                instructions.Add(SheetToInstructions(sheet));

            //convert those arrays of instructions into a timeline and keyframes
            foreach (List<string> instruction in instructions)
            {
                AddInstructionsToTimeline(instruction, ref result,
                    ppsf.TimePerNote, ppsf.TimePerSpace, ppsf.TimePerBreak);
            }

            //finally, return the result
            return result;
        }

        public static void AddInstructionsToTimeline
        (List<string> instructions, ref Timeline timeline,
        int ppsf_tpn, int ppsf_tps, int ppsf_tpb)
        {
            //convert those arrays of instructions into a timeline and keyframes
            int tpn = ppsf_tpn, tps = ppsf_tps, tpb = ppsf_tpb;
            int timestamp = 0;

            foreach (string action in instructions)
            {
                //handling action commands
                if (action.StartsWith("" + TimelinePlayer.PlayerCommandPrefix))
                {
                    if (action.Substring(1).StartsWith("w"))
                    {
                        int i = 0;
                        try { i = int.Parse(action.Substring(3)); }
                        catch (Exception) { }
                        timestamp += i;
                        continue;
                    }
                }
                //handling other actions
                else
                {
                    bool actionContainedNotes = false;

                    //handle each key in action
                    foreach (char ch in action)
                    {
                        if (ChValid(ch))
                        {
                            actionContainedNotes = true;
                            Keyframe keyframe = new Keyframe(timestamp, ch);
                            timeline.AddKeyframe(keyframe);
                        }
                        else if (ch == ' ' && !actionContainedNotes)
                            timestamp += tps;
                        else if (ch == '|' && !actionContainedNotes)
                            timestamp += tpb;
                    }

                    if (actionContainedNotes) timestamp += tpn;
                }
            }
        }

        public static List<string> SheetToInstructions(string input_sheet)
        {
            List<string> FullSheet = new List<string>();

            //make sure the command prefix doesnt conflict with the special keys
            if (ChValid(TimelinePlayer.PlayerCommandPrefix))
            {
                ErrorWindow.ShowExceptionWindow(
                    "Invalid player command prefix: \"" +
                    TimelinePlayer.PlayerCommandPrefix + "\".",
                    new Exception(
                        "Player command prefix must not be one " +
                        "of the following characters:\n\""+ValidNoteCharacters+"\""));
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
                if (!char.IsLetterOrDigit(ch) && !("[]| !@$^*(" + TimelinePlayer.PlayerCommandPrefix).Contains("" + ch)) continue;

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

                    //Conclude character grouping
                    grouped = false;
                }
                else next_notes += ch;
            }

            //return the instructions
            return FullSheet;
        }
        // =======================================================
    }
}
