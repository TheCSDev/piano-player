using System;
using System.Linq;
using System.IO;
using System.Text;
using System.Collections;
using System.Collections.Generic;
using Microsoft.VisualBasic;
using Microsoft.VisualBasic.Devices;
using WaveAudio;
using WaveAudio.WaveProcessing;
using MidiAudio;
using MidiAudio.TrackEvents;

namespace Piano_Player
{
    public class Test
    {
        public static Audio audio = new Audio();
        public static byte[] wavData;

        public static void AudioTest()
        {
            MidiRIFF midi = new MidiRIFF();
            MidiRIFF_MTrk track = new MidiRIFF_MTrk();
            midi.Tracks.Add(track);

            track.TrackEvents.Add();

            File.WriteAllBytes("C://Users/TheCSDev/Desktop/Sound.mid", midi.GetBytes());

            /*WaveRIFF wr = new WaveRIFF("C://Users/TheCSDev/Desktop/song.wav");
            WaveFX.ChangePitchA(ref wr, 100);
            
            wavData = wr.GetBytes();
            //Console.WriteLine(Encoding.Default.GetString(wavData));

            File.WriteAllBytes("C://Users/TheCSDev/Desktop/Sound.wav", wavData);
            audio.Play(wavData, AudioPlayMode.WaitToComplete);*/

            Environment.Exit(0);
        }
    }
}