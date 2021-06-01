using System;
using System.Text;
using System.Collections.Generic;

namespace MidiAudio
{
    public class MidiRIFF_MTrk
    {
        // =======================================================
        private string ChunkID { get; } = "MTrk"; //4 BYTES
        public uint ChunkSize //4 BYTES
        {
            get
            {
                uint totalSize = 0;
                foreach (MidiTrackEvent e in TrackEvents)
                    totalSize += e.TotalEventChunkSize;
                return totalSize;
            }
        }
        // -------------------------------------------------------
        public List<MidiTrackEvent> TrackEvents { get; private set; }
        // =======================================================
        public MidiRIFF_MTrk()
        {
            TrackEvents = new List<MidiTrackEvent>();
        }
        // -------------------------------------------------------
        public byte[] GetBytes()
        {
            //create a new list of bytes
            List<byte> result = new List<byte>();

            //put track data into the list
            result.AddRange(Encoding.ASCII.GetBytes(ChunkID));
            result.AddRange(BitConverter.GetBytes(ChunkSize));

            //put track events data into the list
            foreach (MidiTrackEvent e in TrackEvents)
                result.AddRange(e.GetBytes());

            //return the list array
            return result.ToArray();
        }
        // =======================================================
    }
}
