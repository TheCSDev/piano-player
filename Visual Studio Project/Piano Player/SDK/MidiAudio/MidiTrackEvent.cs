namespace MidiAudio
{
    public abstract class MidiTrackEvent
    {
        // =======================================================
        public abstract uint TotalEventChunkSize { get; }
        public abstract uint DeltaTime { get; set; } //[v length] BYTES
        // -------------------------------------------------------
        public abstract byte[] GetBytes();
        // =======================================================
    }
}
