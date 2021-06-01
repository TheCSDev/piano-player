using System;
using System.Text;
using System.Collections.Generic;

namespace MidiAudio
{
    public class MidiRIFF_MThd
    {
        // =======================================================
        private string ChunkID { get; } = "MThd"; //4 BYTES
        public uint ChunkSize { get; } = 6; //4 BYTES
        // -------------------------------------------------------
        public ushort Format { get; set; } //2 BYTES
        public ushort NumTracksToFollow { get; set; } //2 BYTES
        public ushort Divison { get; set; } //2 BYTES
        // =======================================================
        public MidiRIFF_MThd()
        {
            Format = 0;
            NumTracksToFollow = 1;
            Divison = 60;
        }
        // -------------------------------------------------------
        public byte[] GetBytes()
        {
            //create the result list
            List<byte> result = new List<byte>();

            //add the header chunk data to the list
            result.AddRange(Encoding.ASCII.GetBytes(ChunkID));
            result.AddRange(BitConverter.GetBytes(ChunkSize));
            result.AddRange(BitConverter.GetBytes(Format));
            result.AddRange(BitConverter.GetBytes(NumTracksToFollow));
            result.AddRange(BitConverter.GetBytes(Divison));

            //return the result array
            return result.ToArray();
        }
        // =======================================================
    }
}
