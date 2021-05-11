using System;
using System.IO;
using System.Text;
using System.Collections.Generic;
using Microsoft.VisualBasic;
using Microsoft.VisualBasic.Devices;
using Piano_Player.WaveAudio;

namespace Piano_Player
{
    public class Test
    {
        public static Audio audio = new Audio();
        public static byte[] wavData;

        public static void AudioTest()
        {
            WaveRIFF wr = new WaveRIFF("C://Users/TheCSDev/Desktop/song.wav");
            wavData = wr.GetBytes();

            Console.WriteLine(Encoding.Default.GetString(wavData));

            audio.Play(wavData, AudioPlayMode.WaitToComplete);

            File.WriteAllBytes("C://Users/TheCSDev/Desktop/Sound.wav", wavData);
            Environment.Exit(0);
        }
    }
}
