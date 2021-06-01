using System;
using System.Collections.Generic;

namespace MidiAudio.TrackEvents
{
    public class MidiMetaEvent : MidiTrackEvent
    {
        // =======================================================
        public class EventTypes
        {
            public const byte SequenceNumber              = 0x00;
            public const byte TextEvent                   = 0x01;
            public const byte CopyrightNotice             = 0x02;
            public const byte SequenceOrTrackName         = 0x03;
            public const byte InstrumentName              = 0x04;
            public const byte LyricText                   = 0x05;
            public const byte MarkerText                  = 0x06;
            public const byte CuePoint                    = 0x07;
            public const byte MidiChannelPrefixAssignment = 0x20;
            public const byte EndOfTrack                  = 0x2F;
            public const byte TempoSetting                = 0x51;
            public const byte SmpteOffset                 = 0x54;
            public const byte TimeSignature               = 0x58;
            public const byte KeySignature                = 0x59;
            public const byte SequencerSpecificEvent      = 0x7F;
        }
        // =======================================================
        public override uint TotalEventChunkSize => (uint)GetBytes().Length;
        public override uint DeltaTime { get; set; } //[VLQ] BYTES
        // -------------------------------------------------------
        public byte EventType { get; set; } //1 BYTE
        public uint Length { get { return (uint)Data.Count; } } //[VLQ] BYTES
        public List<byte> Data { get; private set; } //[count] BYTES
        // =======================================================
        public MidiMetaEvent()
        {
            DeltaTime = 0;
            EventType = EventTypes.TextEvent;
            Data = new List<byte>();
        }
        // -------------------------------------------------------
        public override byte[] GetBytes()
        {
            List<byte> result = new List<byte>();
            result.AddRange(MidiRIFF.UintToVLQ(DeltaTime));
            result.Add(0xFF);
            result.AddRange(BitConverter.GetBytes(EventType));
            result.AddRange(MidiRIFF.UintToVLQ(Length));
            result.AddRange(Data);
            return result.ToArray();
        }
        // =======================================================
    }
}
