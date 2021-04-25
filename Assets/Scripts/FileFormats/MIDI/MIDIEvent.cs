using FileFormats;
using System;
using System.Collections.Generic;

namespace Scripts.FileFormats.MIDI
{
    public class MIDIEvent
    {
        public enum Type { Voice, Meta, Sys }
        public enum MidiEventTypeEnum
        {
            NoteOff = 0x80,
            NoteOn = 0x90,
            NoteAftertouch = 0xA0,
            Controller = 0xB0,
            ProgramChange = 0xC0,
            ChannelAftertouch = 0xD0,
            PitchBend = 0xE0
        }
        public enum MetaEventTypeEnum
        {
            // first byte = 0xFF + see below
            SequenceNumber = 0x00,
            TextEvent = 0x01,
            CopyrightNotice = 0x02,
            SequenceOrTrackName = 0x03,
            InstrumentName = 0x04,
            LyricText = 0x05,
            MarkerText = 0x06,
            CuePoint = 0x07,
            MidiChannel = 0x20,
            MidiPort = 0x21,
            EndOfTrack = 0x2F,
            Tempo = 0x51,
            SmpteOffset = 0x54,
            TimeSignature = 0x58,
            KeySignature = 0x59,
            SequencerSpecific = 0x7F
        }


        public enum SystemCommonTypeEnum
        {
            SystemExclusive = 0xF0,
            MtcQuarterFrame = 0xF1,
            SongPosition = 0xF2,
            SongSelect = 0xF3,
            TuneRequest = 0xF6
        }
        public enum ControllerTypeEnum
        {
            BankSelectCoarse = 0x00,
            ModulationCoarse = 0x01,
            BreathControllerCoarse = 0x02,
            FootControllerCoarse = 0x04,
            PortamentoTimeCoarse = 0x05,
            DataEntryCoarse = 0x06,
            VolumeCoarse = 0x07,
            BalanceCoarse = 0x08,
            PanCoarse = 0x0A,
            ExpressionControllerCoarse = 0x0B,
            EffectControl1Coarse = 0x0C,
            EffectControl2Coarse = 0x0D,
            GeneralPurposeSlider1 = 0x10,
            GeneralPurposeSlider2 = 0x11,
            GeneralPurposeSlider3 = 0x12,
            GeneralPurposeSlider4 = 0x13,
            BankSelectFine = 0x20,
            ModulationFine = 0x21,
            BreathControllerFine = 0x22,
            FootControllerFine = 0x24,
            PortamentoTimeFine = 0x25,
            DataEntryFine = 0x26,
            VolumeFine = 0x27,
            BalanceFine = 0x28,
            PanFine = 0x2A,
            ExpressionControllerFine = 0x2B,
            EffectControl1Fine = 0x2C,
            EffectControl2Fine = 0x2D,
            HoldPedal = 0x40,
            Portamento = 0x41,
            SostenutoPedal = 0x42,
            SoftPedal = 0x43,
            LegatoPedal = 0x44,
            Hold2Pedal = 0x45,
            SoundVariation = 0x46,
            SoundTimbre = 0x47,
            SoundReleaseTime = 0x48,
            SoundAttackTime = 0x49,
            SoundBrightness = 0x4A,
            SoundControl6 = 0x4B,
            SoundControl7 = 0x4C,
            SoundControl8 = 0x4D,
            SoundControl9 = 0x4E,
            SoundControl10 = 0x4F,
            GeneralPurposeButton1 = 0x50,
            GeneralPurposeButton2 = 0x51,
            GeneralPurposeButton3 = 0x52,
            GeneralPurposeButton4 = 0x53,
            EffectsLevel = 0x5B,
            TremuloLevel = 0x5C,
            ChorusLevel = 0x5D,
            CelesteLevel = 0x5E,
            PhaseLevel = 0x5F,
            DataButtonIncrement = 0x60,
            DataButtonDecrement = 0x61,
            NonRegisteredParameterFine = 0x62,
            NonRegisteredParameterCourse = 0x63,
            RegisteredParameterFine = 0x64,
            RegisteredParameterCourse = 0x65,
            AllSoundOff = 0x78,
            ResetControllers = 0x79,
            LocalKeyboard = 0x7A,
            AllNotesOff = 0x7B,
            OmniModeOff = 0x7C,
            OmniModeOn = 0x7D,
            MonoMode = 0x7E,
            PolyMode = 0x7F
        }


        public Type type;
        public uint deltatime = 0;
        public uint abstime = 0;
        public byte channel = 0;
        public byte[] code;
        public byte[] arguments;
        public string comment;

        public List<byte> GetMidiBytes()
        {
            List<byte> midiBytes = new List<byte>();
            VLQ time = new VLQ(deltatime);
            midiBytes.AddRange(time.Bytes);
            midiBytes.AddRange(code);
            midiBytes.AddRange(arguments);
            return midiBytes;
        }
        internal void SwitchChannel(byte id)
        {
            if (type == Type.Voice)
            {
                code[0] -= channel;
            }
            channel = id;
            code[0] += channel;
        }


        #region Voice Events
        //"8nH + 2 Bytes"; // 1000	MIDI channel [0 - 15]	Key Number [0 - 127]	Velocity [0 - 127]
        public static MIDIEvent NoteOff(uint _delta, byte _channel, byte _key, byte _velocity = 0x40)
        {
            MIDIEvent ev = new MIDIEvent();
            ev.type = Type.Voice;
            ev.deltatime = _delta;
            ev.channel = _channel;
            ev.code = new byte[] { (byte)(0x80 + _channel) };
            ev.arguments = new byte[] { _key, _velocity };
            return ev;
        }

        //"9nH + 2 Bytes"; // 1001	MIDI channel [0 - 15]	Key Number [0 - 127]	Velocity [0 - 127]
        //          Velocity
        // 0   1        64         127
        // off ppp p pp mp mf f ff fff
        public static MIDIEvent NoteOn(uint _delta, byte _channel, byte _key, byte _velocity = 0x40)
        {
            MIDIEvent ev = new MIDIEvent();
            ev.type = Type.Voice;
            ev.deltatime = _delta;
            ev.channel = _channel;
            ev.code = new byte[] { (byte)(0x90 + _channel) };
            ev.arguments = new byte[] { _key, _velocity };
            return ev;
        }

        public static MIDIEvent ProgramChange(uint _delta, byte _channel, byte _program)
        {
            MIDIEvent ev = new MIDIEvent();
            ev.type = Type.Voice;
            ev.deltatime = _delta;
            ev.channel = _channel;
            ev.code = new byte[] { (byte)(0xC0 + _channel) };
            ev.arguments = new byte[] { _program };
            return ev;
        }
        #endregion

        #region Channel Controller Events (CC)
        public static MIDIEvent ControllerVolume(uint _delta, byte _channel, byte _volume)
        {
            MIDIEvent ev = new MIDIEvent();
            ev.type = Type.Voice;
            ev.deltatime = _delta;
            ev.channel = _channel;
            ev.code = new byte[] { (byte)(0xB0 + _channel), 0x07 };
            ev.arguments = new byte[] { _volume };
            return ev;
        }
        public static MIDIEvent ControllerPan(uint _delta, byte _channel, byte _pan)
        {
            MIDIEvent ev = new MIDIEvent();
            ev.type = Type.Voice;
            ev.deltatime = _delta;
            ev.channel = _channel;
            ev.code = new byte[] { (byte)(0xB0 + _channel), 0x0A };
            ev.arguments = new byte[] { _pan };
            return ev;
        }
        public static MIDIEvent ControllerExpression(uint _delta, byte _channel, byte _exp)
        {
            MIDIEvent ev = new MIDIEvent();
            ev.type = Type.Voice;
            ev.deltatime = _delta;
            ev.channel = _channel;
            ev.code = new byte[] { (byte)(0xB0 + _channel), 0x0B };
            ev.arguments = new byte[] { _exp };
            return ev;
        }
        public static MIDIEvent ControllerReverb(uint _delta, byte _channel, byte _level)
        {
            MIDIEvent ev = new MIDIEvent();
            ev.type = Type.Voice;
            ev.deltatime = _delta;
            ev.channel = _channel;
            ev.code = new byte[] { (byte)(0xB0 + _channel), 0x5B };
            ev.arguments = new byte[] { _level };
            return ev;
        }
        #endregion

        #region MIDI Meta Events
        public static MIDIEvent MetaTrackName(string _name)
        {
            MIDIEvent ev = new MIDIEvent();
            ev.code = new byte[] { 0xFF , 0x03 };
            ev.comment = _name;
            ev.arguments = new XString(_name).Bytes.ToArray();
            return ev;
        }
        public static MIDIEvent MetaMarker(byte _value, byte _unk)
        {
            MIDIEvent ev = new MIDIEvent();
            ev.code = new byte[] { 0xFF, 0x06 };
            ev.arguments = new XString(string.Concat("Marker ", _value, "-", _unk)).Bytes.ToArray();
            return ev;
        }
        public static MIDIEvent MetaEndTrack()
        {
            MIDIEvent ev = new MIDIEvent();
            ev.code = new byte[] { 0xFF, 0x2F };
            ev.arguments = new byte[] { 0x00 };
            return ev;
        }
        public static MIDIEvent MetaTempo(uint _delta, ushort _tempo )
        {
            double tempo = _tempo / 218.4555555555555555555555555;
            uint microSecs = (uint)Math.Round(60000000 / tempo);

            MIDIEvent ev = new MIDIEvent();
            ev.deltatime = _delta;
            ev.code = new byte[] { 0xFF, 0x51, 0x03};
            ev.arguments = new byte[] { (byte)((microSecs & 0xFF0000) >> 16), (byte)((microSecs & 0x00FF00) >> 8), (byte)(microSecs & 0x0000FF) };
            return ev;
        }
        public static MIDIEvent MetaTimeSignature(uint _delta, byte num, byte denom)
        {
            byte _denom = (byte) Math.Round(Math.Log((double)(denom / 0.69314718055994530941723212145818)));

            MIDIEvent ev = new MIDIEvent();
            ev.deltatime = _delta;
            ev.code = new byte[] { 0xFF, 0x58, 0x04 };
            ev.arguments = new byte[] { num, _denom, 0x20, 0x08 };
            return ev;
        }
        #endregion
    }
}
