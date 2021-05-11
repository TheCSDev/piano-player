using System;
using System.Linq;
using System.Text;
using System.Collections.Generic;

namespace Piano_Player.WaveAudio
{
    public class WaveRIFF_DATA
    {
        // =======================================================
        public string ChunkID { get; } = "data"; //4 BYTES
        public uint ChunkSize //4 BYTES
        {
            get
            {
                return (uint)AudioData.Length * 2;
            }
        }
        public short[] AudioData { get; set; } = new short[] { }; //[Numsamples] BYTES
        // =======================================================
        public WaveRIFF_DATA() { }
        // -------------------------------------------------------
        /// <exception cref="Exception"></exception>
        public WaveRIFF_DATA(byte[] dataSubchunk)
        {
            List<byte[]> bs = SPLB(dataSubchunk, 2); //2 bytes per short

            List<short> shorts = new List<short>();
            foreach (byte[] b in bs)
            {
                shorts.Add(BitConverter.ToInt16(b, 0));
            }

            AudioData = shorts.ToArray();
        }
        // =======================================================
        public byte[] GetBytes()
        {
            List<byte> chunkBytes = new List<byte>();

            chunkBytes.AddRange(Encoding.ASCII.GetBytes(ChunkID));
            chunkBytes.AddRange(BitConverter.GetBytes(ChunkSize));

            byte[] bufferBytes = new byte[AudioData.Length * 2];
            Buffer.BlockCopy(AudioData, 0, bufferBytes, 0, bufferBytes.Length);
            chunkBytes.AddRange(bufferBytes.ToList());

            return chunkBytes.ToArray();
        }
        // =======================================================
        //split bye array into arrays
        private static List<byte[]> SPLB(byte[] ba, int split)
        {
            List<byte[]> r = new List<byte[]>();

            List<byte> i = new List<byte>();
            foreach (byte b in ba)
            {
                i.Add(b);
                if (i.Count >= split)
                {
                    r.Add(i.ToArray());
                    i.Clear();
                }
            }

            return r;
        }
        // -------------------------------------------------------
        // =======================================================
    }
}
