/* SOURCES:
 * https://ccrma.stanford.edu/~craig/14q/midifile/MidiFileFormat.html
 * https://github.com/colxi/midi-parser-js/wiki/MIDI-File-Format-Specifications
 * https://en.wikipedia.org/wiki/Variable-length_quantity
 */

using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;

namespace MidiAudio
{
    public class MidiRIFF
    {
        // =======================================================
        public MidiRIFF_MThd Header { get; private set; }
        public List<MidiRIFF_MTrk> Tracks { get; private set; }
        // =======================================================
        public MidiRIFF()
        {
            Header = new MidiRIFF_MThd();
            Tracks = new List<MidiRIFF_MTrk>();
        }
        // -------------------------------------------------------
        public byte[] GetBytes()
        {
            //create the result list
            List<byte> result = new List<byte>();
            
            //add the header chunk data to the list
            result.AddRange(Header.GetBytes());

            //add track data to the list
            foreach (MidiRIFF_MTrk track in Tracks)
                result.AddRange(track.GetBytes());
            
            //return the result array
            return result.ToArray();
        }
        // =======================================================
        public static byte[] UintToVLQ(uint arg0)
        {
            //convert arg0 to bits
            //og. src: https://stackoverflow.com/questions/6758196/convert-int-to-a-bit-array-in-net
            int[] bitsInt = Convert.ToString(arg0, 2).PadLeft(8, '0') // Add 0's from left
                         .Select(c => int.Parse(c.ToString())) // convert each char to int
                         .ToArray(); // Convert IEnumerable from select to Array

            List<bool> bits = bitsInt.Select(x => !(x % 2 == 0)).ToList();
            
            //remove first bits that start with false
            while (bits.Count > 0 && !bits[0])
                bits.RemoveAt(0);

            //define lists
            List<byte> result = new List<byte>();
            List<bool> currCh = new List<bool>();

            //split the bits into 7 length chunks
            for (int index = bits.Count - 1; index >= 0; index--)
            {
                if (currCh.Count >= 7)
                {
                    currCh.Insert(0, result.Count != 0);
                    result.Add(Bits8ToByte(new BitArray(currCh.ToArray())));
                    currCh.Clear();
                }
                currCh.Insert(0, bits[index]);
            }

            //deal with any remaining bits that didnt fit
            if (currCh.Count > 0)
            {
                while (currCh.Count < 7) currCh.Insert(0, false);
                currCh.Insert(0, result.Count != 0);
                result.Add(Bits8ToByte(new BitArray(currCh.ToArray())));
                currCh.Clear();
            }

            //return the result
            return result.ToArray();
        }
        // -------------------------------------------------------
        //og. src: https://stackoverflow.com/questions/560123/convert-from-bitarray-to-byte
        public static byte Bits8ToByte(BitArray bits)
        {
            if (bits.Count != 8)
                throw new ArgumentException("Bits count must be 8.");

            byte[] bytes = new byte[1];
            bits.CopyTo(bytes, 0);
            return bytes[0];
        }
        // -------------------------------------------------------
        public static List<byte> BitConverterGetListBytes(List<byte> arg0)
        {
            List<byte> result = new List<byte>();
            foreach (byte lb in arg0)
                foreach (byte b in BitConverter.GetBytes(lb))
                    result.Add(b);
            return result;
        }
        // =======================================================
    }
}
