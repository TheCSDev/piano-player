using System;
using System.Text;
using System.Collections.Generic;

namespace WaveAudio
{
    public class WaveRIFF_FMT : ICloneable
    {
        // =======================================================
        private string ChunkID { get; } = "fmt "; //4 BYTES
        public uint   ChunkSize { get { return 16; } } //4 BYTES
        public ushort AudioFormat { get; set; } = 1; //2 BYTES
        public ushort NumChannels { get; set; } = 1; //2 BYTES
        public uint   SampleRate { get; set; } = 256; //4 BYTES
        public uint   ByteRate //4 BYTES
        {
            get
            {
                return (ushort)(SampleRate * NumChannels * BitsPerSample / 8);
            }
        }
        public ushort BlockAlign //2 BYTES
        {
            get
            {
                return (ushort)(NumChannels * BitsPerSample / 8);
            }
        }
        //BitsPerSample = 8, 16, 32
        public ushort BitsPerSample { get; set; } = 8; //2 BYTES
        // =======================================================
        public WaveRIFF_FMT() { }

        /// <exception cref="Exception"></exception>
        public WaveRIFF_FMT(byte[] fmtSubchunk)
        {
            AudioFormat   = BitConverter.ToUInt16(BFT(fmtSubchunk, 0, 2), 0);
            NumChannels   = BitConverter.ToUInt16(BFT(fmtSubchunk, 2, 2), 0);
            SampleRate    = BitConverter.ToUInt32(BFT(fmtSubchunk, 4, 4), 0);
            BitsPerSample = BitConverter.ToUInt16(BFT(fmtSubchunk, 14, 2), 0);
        }
        // -------------------------------------------------------
        public object Clone()
        {
            WaveRIFF_FMT result = new WaveRIFF_FMT();
            result.AudioFormat   = AudioFormat;
            result.NumChannels   = NumChannels;
            result.SampleRate    = SampleRate;
            //ByteRate           - read only
            //BlockAlign         - read only
            result.BitsPerSample = BitsPerSample;
            return result;
        }
        // =======================================================
        public byte[] GetBytes()
        {
            List<byte> chunkBytes = new List<byte>();
            chunkBytes.AddRange(Encoding.ASCII.GetBytes(ChunkID));
            chunkBytes.AddRange(BitConverter.GetBytes(ChunkSize));
            chunkBytes.AddRange(BitConverter.GetBytes(AudioFormat));
            chunkBytes.AddRange(BitConverter.GetBytes(NumChannels));
            chunkBytes.AddRange(BitConverter.GetBytes(SampleRate));
            chunkBytes.AddRange(BitConverter.GetBytes(ByteRate));
            chunkBytes.AddRange(BitConverter.GetBytes(BlockAlign));
            chunkBytes.AddRange(BitConverter.GetBytes(BitsPerSample));
            return chunkBytes.ToArray();
        }
        // =======================================================
        private static byte[] BFT(byte[] b, int from, int count)
        {
            List<byte> bs = new List<byte>();
            for (int i = from; i < from + count; i++)
            {
                if (b.Length <= i) break;
                else bs.Add(b[i]);
            }
            return bs.ToArray();
        }
        // =======================================================
    }
}
