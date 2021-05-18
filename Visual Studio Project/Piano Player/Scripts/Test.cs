using System;
using System.IO;
using System.Text;
using System.Collections.Generic;
using Microsoft.VisualBasic;
using Microsoft.VisualBasic.Devices;
using WaveAudio;
using WaveAudio.WaveProcessing;

namespace Piano_Player
{
    public class Test
    {
        public static Audio audio = new Audio();
        public static byte[] wavData;

        public static void AudioTest()
        {
            WaveRIFF wr = new WaveRIFF("C://Users/TheCSDev/Desktop/song.wav");
            WaveFX.ChangePitchA(ref wr, 100);
            
            wavData = wr.GetBytes();
            //Console.WriteLine(Encoding.Default.GetString(wavData));

            File.WriteAllBytes("C://Users/TheCSDev/Desktop/Sound.wav", wavData);
            audio.Play(wavData, AudioPlayMode.WaitToComplete);

            Environment.Exit(0);
        }
    }
}