namespace WaveAudio.WaveProcessing
{
    public static class WaveFX
    {
        //performance inefficient - takes up double the RAM - fix!
        public static WaveRIFF ChangePitch(WaveRIFF input, int percentage)
        {
            //create a copy that will be worked on
            WaveRIFF result = (WaveRIFF)input.Clone();

            //change the pitch (aka frequency)
            result.FMT.SampleRate = (uint)(result.FMT.SampleRate *
                ((double)percentage / 100));

            //return the result
            return result;
        }
    }
}
