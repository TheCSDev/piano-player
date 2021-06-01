using System;
using System.Collections.Generic;

namespace MidiAudio.TrackEvents
{
    public class MidiControlEvent : MidiTrackEvent
    {
        // =======================================================
        public class EventTypes
        {
            public const byte NoteOff           = 0x8;
            public const byte NoteOn            = 0x9;
            public const byte NoteAftertouch    = 0xA;
            public const byte Controller        = 0xB;
            public const byte ProgramChange     = 0xC;
            public const byte ChannelAftertouch = 0xD;
            public const byte PitchBend         = 0xE;
        }
        // =======================================================
        public override uint TotalEventChunkSize => (uint)GetBytes().Length;
        public override uint DeltaTime { get; set; } //[VLQ] BYTES
        // -------------------------------------------------------
        public byte EventType { get; set; } //1 BYTE
        public byte MidiChannel { get; set; } //1 BYTE
        public byte Parameter1 { get; set; } //1 BYTE
        public byte Parameter2 { get; set; } //1 BYTE
        // =======================================================
        public MidiControlEvent()
        {
            DeltaTime   = 0;
            EventType   = EventTypes.NoteOff;
            MidiChannel = 0;
            Parameter1  = 0;
            Parameter2  = 0;
        }
        // -------------------------------------------------------
        public override byte[] GetBytes()
        {
            List<byte> result = new List<byte>();
            result.AddRange(MidiRIFF.UintToVLQ(DeltaTime));
            result.AddRange(BitConverter.GetBytes(EventType));
            result.AddRange(BitConverter.GetBytes(MidiChannel));
            result.AddRange(BitConverter.GetBytes(Parameter1));
            result.AddRange(BitConverter.GetBytes(Parameter2));
            return result.ToArray();
        }
        // =======================================================
    }
}
