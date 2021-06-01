namespace WaveAudio.WaveProcessing
{
    public static class WaveFX
    { 
        // =======================================================
        /// <summary>
        /// Affects audio speed and length.
        /// </summary>
        /// <param name="input"></param>
        /// <param name="percentage"></param>
        public static void ChangePitchA(ref WaveRIFF input, int percentage)
        {
            //change the sample rate
            input.FMT.SampleRate = (uint)(input.FMT.SampleRate *
                ((double)percentage / 100));
        }
        // ------------------------------------------------------
        // =======================================================
    }
}
