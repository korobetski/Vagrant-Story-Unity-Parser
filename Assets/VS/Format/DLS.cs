using System;
using System.Collections.Generic;
using UnityEngine;

namespace VS.Format
{


    //http://www.vgmpf.com/Wiki/index.php?title=DLS
    //https://www.midi.org/specifications-old/item/dls-technology-overview

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
        private static readonly int CONN_SRC_CC1 = 0x0081;              // Modulation Wheel
        private static readonly int CONN_SRC_CC7 = 0x0087;              // Channel Volume
        private static readonly int CONN_SRC_CC10 = 0x008a;             // Pan
        private static readonly int CONN_SRC_CC11 = 0x008b;             // Expression

        /* Registered Parameter Numbers */
        private static readonly int CONN_SRC_RPN0 = 0x0100;
        private static readonly int CONN_SRC_RPN1 = 0x0101;             // RPN1 - Fine Tune
        private static readonly int CONN_SRC_RPN2 = 0x0102;             // RPN2 - Coarse Tune

        /* Generic Destinations */
        private static readonly int CONN_DST_NONE = 0x0000;             // No Destination
        private static readonly int CONN_DST_ATTENUATION = 0x0001;      // Attenuation
        private static readonly int CONN_DST_RESERVED = 0x0002;         // 
        private static readonly int CONN_DST_PITCH = 0x0003;            // Pitch
        public static readonly ushort CONN_DST_PAN = 0x0004;            // Pan

        /* LFO Destinations */
        private static readonly int CONN_DST_LFO_FREQUENCY = 0x0104;    // LFO Frequency
        private static readonly int CONN_DST_LFO_STARTDELAY = 0x0105;   // LFO Start Delay Time

        /* EG1 Destinations */
        public static readonly ushort CONN_DST_EG1_ATTACKTIME = 0x0206; // EG1 Attack Time
        public static readonly ushort CONN_DST_EG1_DECAYTIME = 0x0207;  // EG1 Decay Time
        public static readonly ushort CONN_DST_EG1_RESERVED = 0x0208;
        public static readonly ushort CONN_DST_EG1_RELEASETIME = 0x0209;// EG1 Release Time
        public static readonly ushort CONN_DST_EG1_SUSTAINLEVEL = 0x020a;//EG1 Sustain Level

        /* EG2 Destinations */
        private static readonly int CONN_DST_EG2_ATTACKTIME = 0x030a;   // EG2 Attack Time
        private static readonly int CONN_DST_EG2_DECAYTIME = 0x030b;    // EG2 Decay Time
        private static readonly int CONN_DST_EG2_RESERVED = 0x030c;
        private static readonly int CONN_DST_EG2_RELEASETIME = 0x030d;  // EG2 Release Time
        private static readonly int CONN_DST_EG2_SUSTAINLEVEL = 0x030e; // EG2 Sustain Level

        public static readonly ushort CONN_TRN_NONE = 0x0000;           // No Transform
        private static readonly int CONN_TRN_CONCAVE = 0x0001;          // Concave Transform

        private static readonly uint F_INSTRUMENT_DRUMS = 0x80000000;

        public static readonly uint COLH_SIZE = 4 + 8;
        public static readonly uint INSH_SIZE = 12 + 8;
        public static readonly uint RGNH_SIZE = 14 + 8;  //(12+8)
        public static readonly uint WLNK_SIZE = 12 + 8;
        public static readonly uint LIST_HDR_SIZE = 12;


        private string _name;
        private List<WAV> waves;
        private List<CKinsh> instruments;

        public DLS() : base("DLS ")
        {
            waves = new List<WAV>();
            instruments = new List<CKinsh>();
        }

        public void SetName(string name)
        {
            _name = name;
        }


        public void AddInstrument(uint bank, uint instrumentId)
        {
            CKinsh instrument = new CKinsh(bank, instrumentId, "Instrument " + instrumentId);
            instruments.Add(instrument);
        }

        public void AddInstrument(uint bank, uint instrumentId, string name)
        {
            CKinsh instrument = new CKinsh(bank, instrumentId, name);
            instruments.Add(instrument);
        }

        public void AddInstrument(CKinsh dSLInstrument)
        {
            instruments.Add(dSLInstrument);
        }

        public void AddWave(WAV wave)
        {
            wave.Riff = false;
            waves.Add(wave);
        }

        internal bool WriteFile(string v)
        {
            List<byte> colhb = new List<byte>(BitConverter.GetBytes((UInt32)instruments.Count));
            Chunk colh = new Chunk("colh", colhb);
            AddChunk(colh);

            LISTChunk lins = new LISTChunk("lins");
            foreach (CKinsh inst in instruments)
            {
                LISTChunk ins = new LISTChunk("ins ");
                ins.AddChunk(inst);
                if (inst.regions.Count > 0)
                {
                    LISTChunk lrgn = new LISTChunk("lrgn");
                    foreach (CKrgnh reg in inst.regions)
                    {
                        LISTChunk rgn = new LISTChunk("rgn ");
                        rgn.AddChunk(reg);
                        lrgn.AddChunk(rgn);
                    }
                    ins.AddChunk(lrgn);
                }
                lins.AddChunk(ins);
            }
            AddChunk(lins);

            // Wave Pool
            LISTChunk wvpl = new LISTChunk("wvpl");
            List<byte> ptblb = new List<byte>();
            ptblb.AddRange(BitConverter.GetBytes((UInt32)8));
            ptblb.AddRange(BitConverter.GetBytes((UInt32)waves.Count));
            ulong offset = 0;
            foreach (WAV wave in waves)
            {
                ptblb.AddRange(BitConverter.GetBytes((UInt32)offset));
                offset += wave.GetPaddedSize();
                wave.Riff = false;
                wvpl.AddChunk(wave);
                CKwsmp wsmp = new CKwsmp();
                wvpl.AddChunk(wsmp);
            }
            Chunk ptbl = new Chunk("ptbl", ptblb);
            AddChunk(ptbl);
            AddChunk(wvpl);
            Resize();
            return base.WriteFile(v, this.Write());
        }
    }






    public class CKinsh : Chunk, IChunk
    {
        public new uint headerSize = 20;
        private uint _bank;
        private uint _instrumentId;
        private string _name;
        private List<CKrgnh> _regions;
        private List<CKart1> lart;

        public CKinsh() : base("insh")
        {
            _regions = new List<CKrgnh>();
        }
        public CKinsh(uint bank, uint instrumentId) : base("insh")
        {
            _bank = bank;
            _instrumentId = instrumentId;
            _name = "Instrument " + instrumentId;
            _regions = new List<CKrgnh>();
            lart = new List<CKart1>();

            RIFF.AlignName(_name);
        }
        public CKinsh(uint bank, uint instrumentId, string name) : base("insh")
        {
            _bank = bank;
            _instrumentId = instrumentId;
            _name = name;
            _regions = new List<CKrgnh>();
            lart = new List<CKart1>();

            RIFF.AlignName(_name);
        }
        public CKinsh(uint bank, uint instrumentId, string name, List<CKrgnh> regions) : base("insh")
        {
            _bank = bank;
            _instrumentId = instrumentId;
            _name = name;
            _regions = regions;
            lart = new List<CKart1>();

            RIFF.AlignName(_name);
        }

        public List<CKrgnh> regions { get => _regions; }
        public ulong bank { get => _bank; }
        public ulong instrumentId { get => _instrumentId; }

        public void AddRegion(CKrgnh region)
        {
            _regions.Add(region);
        }

        public void AddRegions(List<CKrgnh> regions)
        {
            _regions.AddRange(regions);
        }

        public void SetRegions(List<CKrgnh> regions)
        {
            _regions = regions;
        }
        public void AddArt(CKart1 art)
        {
            lart.Add(art);
        }
        public new List<byte> Write()
        {
            data = new List<byte>();
            data.AddRange(BitConverter.GetBytes((UInt32)_regions.Count));
            data.AddRange(BitConverter.GetBytes((UInt32)_bank));
            data.AddRange(BitConverter.GetBytes((UInt32)_instrumentId));
            List<byte> buffer = new List<byte>();
            buffer.AddRange(new byte[] { (byte)id[0], (byte)id[1], (byte)id[2], (byte)id[3] });
            if (SkipSize == false)
            {
                buffer.AddRange(BitConverter.GetBytes((UInt32)size));
            }

            if (data != null)
            {
                buffer.AddRange(data);
            }
            foreach (IChunk ck in chunks)
            {
                buffer.AddRange(ck.Write());
            }
            return buffer;
        }
    }

    public class CKrgnh : Chunk, IChunk
    {
        public new uint headerSize = 22;
        private ushort _keyLow;
        private ushort _keyHigh;
        private ushort _velocityLow;
        private ushort _velocityHigh;

        private CKwlnk _wlnk;
        private CKwsmp _sample;
        private List<CKart1> _lart;


        public CKrgnh(ushort keyLow, ushort keyHigh, ushort velocityLow, ushort velocityHigh) : base("rgnh")
        {
            _keyLow = keyLow;
            _keyHigh = keyHigh;
            _velocityLow = velocityLow;
            _velocityHigh = velocityHigh;
            _lart = new List<CKart1>();


        }


        public ushort keyLow { get => _keyLow; }
        public ushort keyHigh { get => _keyHigh; }
        public ushort velocityLow { get => _velocityLow; }
        public ushort velocityHigh { get => _velocityHigh; }
        public ushort option { get => _wlnk.options; }
        public ushort keyGroup { get => _wlnk.phaseGroup; }

        public void AddArticulation(CKart1 art)
        {
            _lart.Add(art);
        }

        public void SetSample(CKwsmp smp)
        {
            _sample = smp;
        }

        public void SetRange(ushort keyLow = 0x00, ushort keyHigh = 0x7F, ushort velocityLow = 0x00, ushort velocityHigh = 0x7F)
        {
            _keyLow = keyLow;
            _keyHigh = keyHigh;
            _velocityLow = velocityLow;
            _velocityHigh = velocityHigh;
        }

        public void SetWaveLink(CKwlnk wlk)
        {
            _wlnk = wlk;
        }

        public void SetWaveLinkInfo(ushort options, ushort phaseGroup, uint channel, uint index)
        {
            _wlnk = new CKwlnk(options, phaseGroup, channel, index);
        }

        public new List<byte> Write()
        {
            data = new List<byte>();
            data.AddRange(BitConverter.GetBytes((UInt16)_keyLow));
            data.AddRange(BitConverter.GetBytes((UInt16)_keyHigh));
            data.AddRange(BitConverter.GetBytes((UInt16)_velocityLow));
            data.AddRange(BitConverter.GetBytes((UInt16)_velocityHigh));
            List<byte> buffer = new List<byte>();
            buffer.AddRange(new byte[] { (byte)id[0], (byte)id[1], (byte)id[2], (byte)id[3] });
            if (SkipSize == false)
            {
                buffer.AddRange(BitConverter.GetBytes((UInt32)size));
            }

            if (data != null)
            {
                buffer.AddRange(data);
            }

            if (_sample != null)
            {
                AddChunk(_sample);
            }

            if (_wlnk != null)
            {
                AddChunk(_wlnk);
            }

            if (_lart != null && _lart.Count > 0)
            {
                List<IChunk> conv = new List<IChunk>(_lart);
                AddChunk(new LISTChunk("lart", conv));
            }
            foreach (IChunk ck in chunks)
            {
                buffer.AddRange(ck.Write());
            }
            return buffer;
        }
    }



    public class CKart1 : Chunk, IChunk
    {
        public new uint headerSize = 16;
        public ulong cbSize = 8;
        public ulong cConnectionBlocks;

        public List<ConnectionBlock> ConnectionBlocks;


        public CKart1() : base("art1")
        {
            ConnectionBlocks = new List<ConnectionBlock>();
        }
        public CKart1(List<ConnectionBlock> connections) : base("art1")
        {
            ConnectionBlocks = connections;
        }

        public void AddADSR(int attack, int decay, int sustain, int release, ushort attackTrans, ushort releaseTrans)
        {
            ConnectionBlocks.Add(new ConnectionBlock(DLS.CONN_SRC_NONE, DLS.CONN_SRC_NONE, DLS.CONN_DST_EG1_ATTACKTIME, attackTrans, attack));
            ConnectionBlocks.Add(new ConnectionBlock(DLS.CONN_SRC_NONE, DLS.CONN_SRC_NONE, DLS.CONN_DST_EG1_DECAYTIME, DLS.CONN_TRN_NONE, decay));
            ConnectionBlocks.Add(new ConnectionBlock(DLS.CONN_SRC_NONE, DLS.CONN_SRC_NONE, DLS.CONN_DST_EG1_SUSTAINLEVEL, DLS.CONN_TRN_NONE, sustain));
            ConnectionBlocks.Add(new ConnectionBlock(DLS.CONN_SRC_NONE, DLS.CONN_SRC_NONE, DLS.CONN_DST_EG1_RELEASETIME, releaseTrans, release));
        }
        public void AddPan(int pan)
        {
            ConnectionBlocks.Add(new ConnectionBlock(DLS.CONN_SRC_NONE, DLS.CONN_SRC_NONE, DLS.CONN_DST_PAN, DLS.CONN_TRN_NONE, pan));
        }



        public new List<byte> Write()
        {
            data = new List<byte>();
            data.AddRange(BitConverter.GetBytes((UInt32)cbSize));
            data.AddRange(BitConverter.GetBytes((UInt32)cConnectionBlocks));
            foreach(ConnectionBlock cb in ConnectionBlocks)
            {
                data.AddRange(BitConverter.GetBytes((UInt16)cb.usSource));
                data.AddRange(BitConverter.GetBytes((UInt16)cb.usControl));
                data.AddRange(BitConverter.GetBytes((UInt16)cb.usDestination));
                data.AddRange(BitConverter.GetBytes((UInt16)cb.usTransform));
                data.AddRange(BitConverter.GetBytes((UInt32)cb.lScale));
            }
            List<byte> buffer = new List<byte>();
            buffer.AddRange(new byte[] { (byte)id[0], (byte)id[1], (byte)id[2], (byte)id[3] });
            if (SkipSize == false)
            {
                buffer.AddRange(BitConverter.GetBytes((UInt32)size));
            }

            if (data != null)
            {
                buffer.AddRange(data);
            }
            foreach (IChunk ck in chunks)
            {
                buffer.AddRange(ck.Write());
            }
            return buffer;
        }
    }



    public class ConnectionBlock
    {
        public ushort usSource;
        public ushort usControl;
        public ushort usDestination;
        public ushort usTransform;
        public long lScale;

        public ConnectionBlock()
        {

        }

        public ConnectionBlock(ushort source, ushort control, ushort destination, ushort transform, int scale)
        {
            usSource = source;
            usControl = control;
            usDestination = destination;
            usTransform = transform;
            lScale = scale;
        }


    }





    public class CKwlnk : Chunk, IChunk // Wave Link Chunk
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
        public uint channel;
        // Specifies the 0 based index of the cue entry in the wave pool table.
        public uint tableIndex;


        public CKwlnk() : base("wlnk")
        {

        }
        public CKwlnk(ushort OP, ushort PG, uint CHA, uint TBI) : base("wlnk")
        {
            options = OP;
            phaseGroup = PG;
            channel = CHA;
            tableIndex = TBI;
        }

        public new List<byte> Write()
        {
            data.AddRange(BitConverter.GetBytes((ushort)options));
            data.AddRange(BitConverter.GetBytes((ushort)phaseGroup));
            data.AddRange(BitConverter.GetBytes((uint)channel));
            data.AddRange(BitConverter.GetBytes((uint)tableIndex));


            List<byte> buffer = new List<byte>();
            buffer.AddRange(new byte[] { (byte)id[0], (byte)id[1], (byte)id[2], (byte)id[3] });
            if (SkipSize == false)
            {
                buffer.AddRange(BitConverter.GetBytes((UInt32)size));
            }

            if (data != null)
            {
                buffer.AddRange(data);
            }
            foreach (IChunk ck in chunks)
            {
                buffer.AddRange(ck.Write());
            }
            return buffer;
        }
    }

    public class CKwsmp : Chunk, IChunk // Wave Sample Chunk
    {
        public ulong cbSize = 24;
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

        public new List<byte> Write()
        {
            data = new List<byte>();
            data.AddRange(BitConverter.GetBytes((UInt32)cbSize));
            data.AddRange(BitConverter.GetBytes((UInt16)unityNote));
            data.AddRange(BitConverter.GetBytes((UInt16)fineTune));
            data.AddRange(BitConverter.GetBytes((UInt32)attenuation));
            data.AddRange(BitConverter.GetBytes((UInt32)options));
            data.AddRange(BitConverter.GetBytes((UInt32)sampleLoops));
            foreach (CKloop l in loops)
            {
                data.AddRange(BitConverter.GetBytes((UInt32)l.cbSize));
                data.AddRange(BitConverter.GetBytes((UInt32)l.loopType));
                data.AddRange(BitConverter.GetBytes((UInt32)l.loopStart));
                data.AddRange(BitConverter.GetBytes((UInt32)l.loopLength));
            }
            List<byte> buffer = new List<byte>();
            buffer.AddRange(new byte[] { (byte)id[0], (byte)id[1], (byte)id[2], (byte)id[3] });
            if (SkipSize == false)
            {
                buffer.AddRange(BitConverter.GetBytes((UInt32)size));
            }

            if (data != null)
            {
                buffer.AddRange(data);
            }
            foreach (IChunk ck in chunks)
            {
                buffer.AddRange(ck.Write());
            }
            return buffer;
        }
    }

    public class CKloop : Chunk, IChunk
    {
        public ulong cbSize = 12;
        public ulong loopType; // Specifies the loop type : WLOOP_TYPE_FORWARD Forward Loop
        public ulong loopStart; // Specifies the start point of the loop in samples as an absolute offset from the beginning of the data in the<data-ck> subchunk of the<wave-list> wave file chunk.
        public ulong loopLength; // Specifies the length of the loop in samples.

        public CKloop() : base("loop")
        {

        }
        public CKloop(ulong LT, ulong LS, ulong LL) : base("loop")
        {
            loopType = LT;
            loopStart = LS;
            loopLength = LL;
        }
    }

    public class CKptbl : Chunk, IChunk // Pool Table Chunk
    {
        public ulong cbSize = 8;
        public ulong cues; // Specifies the number (count) of <poolcue> records that are contained in the <ptbl-ck>
        // chunk.The<poolcue> records are stored immediately following the cCues data field.
        public List<ulong> poolcues;

        public CKptbl() : base("ptbl")
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

        public new List<byte> Write()
        {
            data.AddRange(BitConverter.GetBytes((UInt32)cbSize));
            data.AddRange(BitConverter.GetBytes((UInt32)cues));
            foreach (ulong l in poolcues)
            {
                data.AddRange(BitConverter.GetBytes((UInt32)l));
            }
            List<byte> buffer = new List<byte>();
            buffer.AddRange(new byte[] { (byte)id[0], (byte)id[1], (byte)id[2], (byte)id[3] });
            if (SkipSize == false)
            {
                buffer.AddRange(BitConverter.GetBytes((UInt32)size));
            }

            if (data != null)
            {
                buffer.AddRange(data);
            }
            foreach (IChunk ck in chunks)
            {
                buffer.AddRange(ck.Write());
            }
            return buffer;
        }
    }

    public class CKvers : Chunk, IChunk
    {
        public ulong versionMS;
        public ulong versionLS;


        public CKvers() : base("vers")
        {

        }
    }

    public class LCInfo : LISTChunk, IChunk
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

        public LCInfo() : base("INFO")
        {

        }
    }

}