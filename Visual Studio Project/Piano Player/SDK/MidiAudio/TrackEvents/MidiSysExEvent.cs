using System;
using System.Collections.Generic;

namespace MidiAudio.TrackEvents
{
    public class MidiSysExEvent : MidiTrackEvent
    {
        // =======================================================
        public class EventTypes
        {
            public const byte _0F0 = 0xF0;
            public const byte _0F7 = 0XF7;
        }
        // =======================================================
        public override uint TotalEventChunkSize => (uint)GetBytes().Length;
        public byte EventType { get; set; }
        // -------------------------------------------------------
        public override uint DeltaTime { get; set; } //[VLQ] BYTES
        public uint Length { get { return (uint)Data.Count; } } //[VLQ] BYTES
        public List<byte> Data { get; private set; } //[count] BYTES
        // =======================================================
        public MidiSysExEvent()
        {
            DeltaTime = 0;
            EventType = EventTypes._0F0;
        }
        // -------------------------------------------------------
        public override byte[] GetBytes()
        {
            List<byte> result = new List<byte>();
            result.AddRange(MidiRIFF.UintToVLQ(DeltaTime));
            result.AddRange(BitConverter.GetBytes(EventType));
            result.AddRange(MidiRIFF.UintToVLQ(Length));
            result.AddRange(Data);
            return result.ToArray();
        }
        // =======================================================
    }
}
