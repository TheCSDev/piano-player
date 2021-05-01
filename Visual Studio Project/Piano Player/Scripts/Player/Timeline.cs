using System;
using System.Collections.Generic;

namespace Piano_Player.Player
{
    public class Timeline
    {
        // =======================================================
        public const string ValidNoteCharacters = "1234567890qwertyuiopasdfghjklzxcvbnm!@$%^*(";

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
            public Keyframe(Timeline parent, int timestamp, char noteKey)
            {
                Timestamp = timestamp;
                NoteKey = noteKey;

                if (parent == null)
                    throw new ArgumentNullException
                        ("Keyframe parent timeline cannot be null.");
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
        // =======================================================
        public static bool ChValid(char ch)
        {
            return ValidNoteCharacters.Contains(ch.ToString())
                && ch != '\0';
        }
        // =======================================================
    }
}
