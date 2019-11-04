using System;
using System.Collections.Generic;
using VS.Parser;

namespace VS.Format
{
    public class DLS : RIFF
    {

        /*
           Articulation connection graph definitions
        */

        /* Generic Sources */
        public static readonly ushort CONN_SRC_NONE = 0x0000;           // No Source
        private static readonly int CONN_SRC_LFO = 0x0001;              // Low Frequency Oscillator
        private static readonly int CONN_SRC_KEYONVELOCITY = 0x0002;    // Key on Velocity
        private static readonly int CONN_SRC_KEYNUMBER = 0x0003;        // Key Number
        private static readonly int CONN_SRC_EG1 = 0x0004;              // Envelope Generator 1
        private static readonly int CONN_SRC_EG2 = 0x0005;              // Envelope Generator 2
        private static readonly int CONN_SRC_PITCHWHEEL = 0x0006;       // Pitch Wheel

        /* Midi Controllers 0-127 */
        private static readonly int CONN_SRC_CC1 = 0x0081;
        private static readonly int CONN_SRC_CC7 = 0x0087;
        private static readonly int CONN_SRC_CC10 = 0x008a;
        private static readonly int CONN_SRC_CC11 = 0x008b;

        /* Registered Parameter Numbers */
        private static readonly int CONN_SRC_RPN0 = 0x0100;
        private static readonly int CONN_SRC_RPN1 = 0x0101;
        private static readonly int CONN_SRC_RPN2 = 0x0102;

        /* Generic Destinations */
        private static readonly int CONN_DST_NONE = 0x0000;
        private static readonly int CONN_DST_ATTENUATION = 0x0001;
        private static readonly int CONN_DST_RESERVED = 0x0002;
        private static readonly int CONN_DST_PITCH = 0x0003;
        private static readonly int CONN_DST_PAN = 0x0004;

        /* LFO Destinations */
        private static readonly int CONN_DST_LFO_FREQUENCY = 0x0104;
        private static readonly int CONN_DST_LFO_STARTDELAY = 0x0105;

        /* EG1 Destinations */
        public static readonly short CONN_DST_EG1_ATTACKTIME = 0x0206;
        public static readonly short CONN_DST_EG1_DECAYTIME = 0x0207;
        public static readonly short CONN_DST_EG1_RESERVED = 0x0208;
        public static readonly short CONN_DST_EG1_RELEASETIME = 0x0209;
        public static readonly short CONN_DST_EG1_SUSTAINLEVEL = 0x020a;

        /* EG2 Destinations */
        private static readonly int CONN_DST_EG2_ATTACKTIME = 0x030a;
        private static readonly int CONN_DST_EG2_DECAYTIME = 0x030b;
        private static readonly int CONN_DST_EG2_RESERVED = 0x030c;
        private static readonly int CONN_DST_EG2_RELEASETIME = 0x030d;
        private static readonly int CONN_DST_EG2_SUSTAINLEVEL = 0x030e;

        public static readonly ushort CONN_TRN_NONE = 0x0000;
        private static readonly int CONN_TRN_CONCAVE = 0x0001;

        private static readonly uint F_INSTRUMENT_DRUMS = 0x80000000;

        public static readonly uint COLH_SIZE = 4 + 8;
        public static readonly uint INSH_SIZE = 12 + 8;
        public static readonly uint RGNH_SIZE = 14 + 8;  //(12+8)
        public static readonly uint WLNK_SIZE = 12 + 8;
        public static readonly uint LIST_HDR_SIZE = 12;


        private string _name;
        private List<WAV> waves;
        private List<DLSInstrument> instruments;

        public DLS() : base("DLS ")
        {
            waves = new List<WAV>();
            instruments = new List<DLSInstrument>();
        }

        public void SetName(string name)
        {
            _name = name;
        }


        public void AddInstrument(uint bank, uint instrumentId)
        {
            DLSInstrument instrument = new DLSInstrument(bank, instrumentId, "Instrument " + instrumentId);
            instruments.Add(instrument);
        }

        public void AddInstrument(uint bank, uint instrumentId, string name)
        {
            DLSInstrument instrument = new DLSInstrument(bank, instrumentId, name);
            instruments.Add(instrument);
        }

        public void AddWave(WAV wave)
        {
            waves.Add(wave);
        }

        public void Define()
        {
            List<byte> colhb = new List<byte>(BitConverter.GetBytes((ulong)instruments.Count));
            Chunk colh = new Chunk("colh", colhb);
            AddChunk(colh);

            LISTChunk lins = new LISTChunk("lins");
            foreach (DLSInstrument inst in instruments)
            {
                LISTChunk ins = new LISTChunk("ins ");
                List<byte> inshb = new List<byte>();
                inshb.AddRange(BitConverter.GetBytes((ulong)inst.regions.Count)); // cRegions : ULONG
                inshb.AddRange(BitConverter.GetBytes((ulong)inst.bank)); // ulBank : ULONG Specifies the MIDI bank location. Bits 0-6 are defined as MIDI CC32 and bits 8 - 14 are defined as MIDI CC0.
                // Bits 7 and 15 - 30 are reserved and should be written to zero. If Bit 31 is equal to 1 then the instrument is a drum instrument; if equal to 0 then the instrument is a melodic instrument.
                inshb.AddRange(BitConverter.GetBytes((ulong)inst.id)); // ulInstrument : ULONG Specifies the MIDI Program Change (PC) value. Bits 0-6 are defined as
                // PC value and bits 7 - 31 are reserved and should be written to zero.
                ins.AddChunk(new Chunk("insh", inshb));
                if (inst.regions.Count > 0)
                {
                    LISTChunk lrgn = new LISTChunk("lrgn");
                    foreach (DLSRegion reg in inst.regions)
                    {
                        LISTChunk rgn = new LISTChunk("rgn ");
                        List<byte> rgnhb = new List<byte>();
                        rgnhb.AddRange(BitConverter.GetBytes((ushort)reg.keyLow));
                        rgnhb.AddRange(BitConverter.GetBytes((ushort)reg.keyHigh));
                        rgnhb.AddRange(BitConverter.GetBytes((ushort)reg.velocityLow));
                        rgnhb.AddRange(BitConverter.GetBytes((ushort)reg.velocityHigh));
                        rgnhb.AddRange(BitConverter.GetBytes((ushort)reg.option)); // DLS Doc page 47 / 77
                        rgnhb.AddRange(BitConverter.GetBytes((ushort)reg.keyGroup)); // DLS Doc page 47 / 77
                        rgn.AddChunk(new Chunk("rgnh", rgnhb));
                        lrgn.AddChunk(rgn);
                    }
                    ins.AddChunk(lrgn);
                }
                lins.AddChunk(ins);
            }
            AddChunk(lins);

            LISTChunk wvpl = new LISTChunk("wvpl");
            List<byte> ptblb = new List<byte>();
            ptblb.AddRange(BitConverter.GetBytes((ulong)8));
            ptblb.AddRange(BitConverter.GetBytes((ulong)waves.Count));
            ulong offset = 0;
            foreach (WAV wave in waves)
            {
                ptblb.AddRange(BitConverter.GetBytes((ulong)offset));
                offset += wave.GetPaddedSize();

                wvpl.AddChunk(wave);



                CKwsmp wsmp = new CKwsmp();
                wvpl.AddChunk(wsmp);
            }
            Chunk ptbl = new Chunk("ptbl", ptblb);
            AddChunk(ptbl);


            AddChunk(wvpl);

        }

    }

    public class DLSInstrument
    {
        private uint _bank;
        private uint _instrumentId;
        private string _name;
        private List<DLSRegion> _regions;

        public DLSInstrument()
        {
            _regions = new List<DLSRegion>();
        }
        public DLSInstrument(uint bank, uint instrumentId)
        {
            _bank = bank;
            _instrumentId = instrumentId;
            _name = "Instrument " + instrumentId;
            _regions = new List<DLSRegion>();

            RIFF.AlignName(_name);
        }
        public DLSInstrument(uint bank, uint instrumentId, string name)
        {
            _bank = bank;
            _instrumentId = instrumentId;
            _name = name;
            _regions = new List<DLSRegion>();

            RIFF.AlignName(_name);
        }
        public DLSInstrument(uint bank, uint instrumentId, string name, List<DLSRegion> regions)
        {
            _bank = bank;
            _instrumentId = instrumentId;
            _name = name;
            _regions = regions;

            RIFF.AlignName(_name);
        }

        public List<DLSRegion> regions { get => _regions; }
        public ulong bank { get => _bank; }
        public ulong id { get => _instrumentId; }

        public void AddRegion(DLSRegion region)
        {
            _regions.Add(region);
        }

        public void AddRegions(List<DLSRegion> regions)
        {
            _regions.AddRange(regions);
        }

        public void SetRegions(List<DLSRegion> regions)
        {
            _regions = regions;
        }
    }
    /*
    public class DLSWave
    {
        private ushort _formatTag;
        private ushort _channels;
        private int _samplesPerSec;
        private int _aveBytesPerSec;
        private ushort _blockAlign;
        private ushort _bitsPerSample;
        private uint _waveDataSize;
        private char[] _waveData;
        private string _name;
        private DLSWaveSample _waveSample;

        public DLSWave()
        {
            _name = "Untitled Wave";
            DLS.AlignName(_name);
        }

        public DLSWave(ushort formatTag, ushort channels, int samplesPerSec, int aveBytesPerSec, ushort blockAlign, ushort bitsPerSample, uint waveDataSize, char[] waveData, string name)
        {
            _formatTag = formatTag;
            _channels = channels;
            _samplesPerSec = samplesPerSec;
            _aveBytesPerSec = aveBytesPerSec;
            _blockAlign = blockAlign;
            _bitsPerSample = bitsPerSample;
            _waveDataSize = waveDataSize;
            _waveData = waveData;

            _name = name;
            DLS.AlignName(_name);
        }

        public ulong size
        {
            get => _waveDataSize;
        }

    }
    */
    public class DLSRegion
    {
        private ushort _keyLow;
        private ushort _keyHigh;
        private ushort _velocityLow;
        private ushort _velocityHigh;

        private ushort _options = 1;
        private ushort _phaseGroup = 0;
        private uint _channel;
        private uint _index;

        private CKwsmp _sample;
        private DLSArticulation _articulation;


        public DLSRegion(ushort keyLow, ushort keyHigh, ushort velocityLow, ushort velocityHigh)
        {
            _keyLow = keyLow;
            _keyHigh = keyHigh;
            _velocityLow = velocityLow;
            _velocityHigh = velocityHigh;
        }

        public DLSRegion(ushort keyLow, ushort keyHigh, ushort velocityLow, ushort velocityHigh, DLSArticulation articulation)
        {
            _keyLow = keyLow;
            _keyHigh = keyHigh;
            _velocityLow = velocityLow;
            _velocityHigh = velocityHigh;
            _articulation = articulation;
        }

        public ushort keyLow { get => _keyLow; }
        public ushort keyHigh { get => _keyHigh; }
        public ushort velocityLow { get => velocityLow; }
        public ushort velocityHigh { get => _velocityHigh; }
        public ushort option { get => _options; }
        public ushort keyGroup { get => _phaseGroup; }

        public void AddArticulation()
        {
            _articulation = new DLSArticulation();
        }

        public void AddSample()
        {
            _sample = new CKwsmp();
        }

        public void SetRange(ushort keyLow = 0x00, ushort keyHigh = 0x7F, ushort velocityLow = 0x00, ushort velocityHigh = 0x7F)
        {
            _keyLow = keyLow;
            _keyHigh = keyHigh;
            _velocityLow = velocityLow;
            _velocityHigh = velocityHigh;
        }

        public void SetWaveLinkInfo(ushort options, ushort phaseGroup, uint channel, uint index)
        {
            _options = options;
            _phaseGroup = phaseGroup;
            _channel = channel;
            _index = index;
        }

    }



    public class DLSArticulation
    {
        private List<ConnectionBlock> _connections;


        public DLSArticulation()
        {

        }
        public DLSArticulation(List<ConnectionBlock> connections)
        {
            _connections = connections;
        }

        public void AddADSR(int attack, int decay, int sustain, int release, ushort attackTrans, ushort releaseTrans)
        {
            _connections.Add(new ConnectionBlock(DLS.CONN_SRC_NONE, DLS.CONN_SRC_NONE, DLS.CONN_DST_EG1_ATTACKTIME, attackTrans, attack));
            _connections.Add(new ConnectionBlock(DLS.CONN_SRC_NONE, DLS.CONN_SRC_NONE, DLS.CONN_DST_EG1_DECAYTIME, DLS.CONN_TRN_NONE, decay));
            _connections.Add(new ConnectionBlock(DLS.CONN_SRC_NONE, DLS.CONN_SRC_NONE, DLS.CONN_DST_EG1_SUSTAINLEVEL, DLS.CONN_TRN_NONE, sustain));
            _connections.Add(new ConnectionBlock(DLS.CONN_SRC_NONE, DLS.CONN_SRC_NONE, DLS.CONN_DST_EG1_RELEASETIME, releaseTrans, release));
        }
        public void AddPan(long pan)
        {

        }
    }
    public class ConnectionBlock
    {
        private ushort _source;
        private ushort _control;
        private short _destination;
        private ushort _transform;
        private int _scale;

        public ConnectionBlock()
        {

        }

        public ConnectionBlock(ushort source, ushort control, short destination, ushort transform, int scale)
        {
            _source = source;
            _control = control;
            _destination = destination;
            _transform = transform;
            _scale = scale;
        }
    }





    public class CKwlnk:Chunk // Wave Link Chunk
    {
        public ushort options;
        /*
        Specifies a group number for samples which are phase locked. All waves in a set of
        wave links with the same group are phase locked and follow the wave in the group with
        the F_WAVELINK_PHASE_MASTER flag set. If a wave is not a member of a phase
        locked group, this value should be set to 0.
        */
        public ushort phaseGroup;
        /*
        Specifies the channel placement of the file. This is used to place mono sounds within a
        stereo pair or for multi-track placement. Each bit position within the ulChannel field
        specifies a channel placement with bit 0 specifying a mono file or the left channel of a
        stereo file. Bit 1 specifies the right channel of a stereo file.
        */
        public ulong channel;
        // Specifies the 0 based index of the cue entry in the wave pool table.
        public ulong tableIndex;


        public CKwlnk():base("wlnk")
        {

        }
    }

    public class CKwsmp : Chunk
    {
        public ushort unityNote;
        public short fineTune;
        public long attenuation;
        public ulong options;
        public ulong sampleLoops = 0;
        public List<CKloop> loops;

        public CKwsmp() : base("wsmp")
        {
            loops = new List<CKloop>();
        }

        public CKwsmp(ushort UN, short FT, long AT, ulong OP) : base("wsmp")
        {
            unityNote = UN;
            fineTune = FT;
            attenuation = AT;
            options = OP;
            loops = new List<CKloop>();
        }

        public void SetPitchInfo(ushort UN, short FT, long AT, ulong OP)
        {
            unityNote = UN;
            fineTune = FT;
            attenuation = AT;
            options = OP;
            loops = new List<CKloop>();
        }
        public void AddLoop(CKloop LP)
        {
            loops.Add(LP);
            sampleLoops = (ulong)loops.Count;
        }

        public void AddLoops(List<CKloop> LPs)
        {
            loops.AddRange(LPs);
            sampleLoops = (ulong)loops.Count;
        }
    }

    public class CKloop : Chunk
    {
        public ulong loopType; // Specifies the loop type : WLOOP_TYPE_FORWARD Forward Loop
        public ulong loopStart; // Specifies the start point of the loop in samples as an absolute offset from the beginning of the data in the<data-ck> subchunk of the<wave-list> wave file chunk.
        public ulong loopLength; // Specifies the length of the loop in samples.

        public CKloop() : base("loop")
        {

        }
    }

    public class CKptbl:Chunk // Pool Table Chunk
    {
        public ulong cues; // Specifies the number (count) of <poolcue> records that are contained in the <ptbl-ck>
        // chunk.The<poolcue> records are stored immediately following the cCues data field.
        public List<ulong> poolcues;

        public CKptbl():base("ptbl")
        {
            poolcues = new List<ulong>();
        }

        public void AddCue(ulong offset)
        {
            poolcues.Add(offset);
            cues = (ulong)poolcues.Count;
        }
        public void AddCues(List<ulong> offsets)
        {
            poolcues.AddRange(offsets);
            cues = (ulong)poolcues.Count;
        }
    }

    public class CKvers:Chunk
    {
        public ulong versionMS;
        public ulong versionLS;


        public CKvers():base("vers")
        {

        }
    }

    public class LCInfo:LISTChunk
    {
        public Chunk IARL;
        public Chunk IART;
        public Chunk ICMS;
        public Chunk ICMT;
        public Chunk ICOP;
        public Chunk ICRD;
        public Chunk IENG;
        public Chunk IGNR;
        public Chunk IKEY;
        public Chunk IMED;
        public Chunk INAM;
        public Chunk IPRD;
        public Chunk ISBJ;
        public Chunk ISFT;
        public Chunk ISRC;
        public Chunk ISRF;
        public Chunk ITCH;

        public LCInfo():base("INFO")
        {

        }
    }

}