/* SOURCES:
 * https://www.codeguru.com/columns/dotnet/making-sounds-with-waves-using-c.html
 * http://soundfile.sapp.org/doc/WaveFormat/
 */

using System;
using System.IO;
using System.Text;
using System.Collections.Generic;

namespace WaveAudio
{
    public class WaveRIFF : ICloneable
    {
        // =======================================================
        private string ChunkID { get; } = "RIFF"; //4 BYTES
        public uint ChunkSize //4 BYTES
        {
            get
            {
                return FMT.ChunkSize + DATA.ChunkSize;
            }
        }
        private string Format { get; } = "WAVE"; //4 BYTES
        // -------------------------------------------------------
        /// <summary>Duration in seconds</summary>
        public uint Duration { get { return (uint)(DATA.AudioData.Count / FMT.ByteRate); } }
        // -------------------------------------------------------
        public WaveRIFF_FMT /*Chunk_*/FMT { get; private set; } = new WaveRIFF_FMT();
        public WaveRIFF_DATA /*Chunk_*/DATA { get; private set; } = new WaveRIFF_DATA();
        // =======================================================
        public WaveRIFF() { }

        ///<summary>Also throws exceptions from <seealso cref="File.ReadAllBytes(string)"/></summary>
        ///<exception cref="IOException"></exception>
        ///<exception cref="ArgumentNullException"></exception>
        public WaveRIFF(string wavPath, bool ignoreExceptions = false)
        {
            if (wavPath == null)
            {
                if (!ignoreExceptions) throw new ArgumentNullException();
                else return;
            }

            ConstructFromBytes(File.ReadAllBytes(wavPath), ignoreExceptions);
        }

        /// <exception cref="Exception"></exception>
        /// <exception cref="IndexOutOfRangeException"></exception>
        public WaveRIFF(byte[] uncompressedWavData, bool ignoreExceptions = false)
        {
            ConstructFromBytes(uncompressedWavData, ignoreExceptions);
        }

        /// <exception cref="Exception"></exception>
        /// <exception cref="IndexOutOfRangeException"></exception>
        private void ConstructFromBytes(byte[] uwd, bool ignoreExceptions = false)
        {
            try
            {
                //try to make sure the bytes are valid
                if (!ABTS(BFT(uwd, 0, 4)).ToLower().Equals(ChunkID.ToLower()) ||
                    !ABTS(BFT(uwd, 8, 4)).ToLower().Equals(Format.ToLower()))
                    throw new Exception("Invalid WAV byte ChunkID or Format.");

                //get chunk data
                int chSize = BitConverter.ToInt32(BFT(uwd, 4, 4), 0);
                byte[] chunk = BFT(uwd, 12, chSize);

                byte[] fmt = FindSUBCH(chunk, "fmt ");
                byte[] data = FindSUBCH(chunk, "data");

                //construct FMT and DATA
                FMT = new WaveRIFF_FMT(fmt);
                DATA = new WaveRIFF_DATA(data);
            }
            catch (Exception e)
            {
                if (!ignoreExceptions) throw e;
                else
                {
                    FMT = new WaveRIFF_FMT();
                    DATA = new WaveRIFF_DATA();
                    return;
                }
            }
        }
        // -------------------------------------------------------
        public object Clone()
        {
            WaveRIFF result = new WaveRIFF();
            result.FMT      = (WaveRIFF_FMT)FMT.Clone();
            result.DATA     = (WaveRIFF_DATA)DATA.Clone();
            return result;
        }
        // =======================================================
        public byte[] GetBytes()
        {
            List<byte> chunkData = new List<byte>();
            chunkData.AddRange(Encoding.ASCII.GetBytes(ChunkID));
            chunkData.AddRange(BitConverter.GetBytes(ChunkSize));
            chunkData.AddRange(Encoding.ASCII.GetBytes(Format));
            chunkData.AddRange(FMT.GetBytes());
            chunkData.AddRange(DATA.GetBytes());
            return chunkData.ToArray();
        }
        // =======================================================
        //returns a byte array with bytes from index [from] to [from + count]
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
        // -------------------------------------------------------
        //decode ascii bytes
        private static string ABTS(byte[] b)
        {
            return Encoding.ASCII.GetString(b);
        }
        // -------------------------------------------------------
        //substring byte array (remove first few elements)
        private static byte[] BSUB(byte[] b, int substr)
        {
            List<byte> bs = new List<byte>();
            for (int i = substr; i < b.Length; i++) { bs.Add(b[i]); }
            return bs.ToArray();
        }
        // -------------------------------------------------------
        //find a subchunk by name in a chunk (subchName.Length has to be 4)
        //subchName is not case sensitive
        //WARNING: CAN CAUSE STACK OVERFLOW AND CRASH (if the chunk is too big)
        private static byte[] FindSUBCH(byte[] chunk, string subchName, int layer = 0)
        {
            try
            {
                //Console.WriteLine("Looking for: \"" + subchName + "\", Found: \"" + ABTS(BFT(chunk, 0, 4)).ToLower() +  "\" Layer: " + layer);
                if (subchName.Length != 4) throw new Exception("subchName.Length != 4");

                int chSize = BitConverter.ToInt32(BFT(chunk, 4, 4), 0);

                if (ABTS(BFT(chunk, 0, 4)).ToLower().Equals(subchName.ToLower()))
                    return BFT(chunk, 8, chSize);
                else return FindSUBCH(BSUB(chunk, chSize + 8), subchName, layer +1);
            }
            catch (Exception) { return new byte[] { }; }
        }
        // =======================================================
        public static implicit operator byte[](WaveRIFF wave)
        {
            return wave.GetBytes();
        }
        // =======================================================
    }
}
 