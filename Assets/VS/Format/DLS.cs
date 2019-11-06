using System;
using System.Collections.Generic;
using UnityEngine;

namespace VS.Format
{


    //http://www.vgmpf.com/Wiki/index.php?title=DLS
    //https://www.midi.org/specifications-old/item/dls-technology-overview
    // https://www.recordingblogs.com/wiki/downloadable-sounds-dls-format

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

        private CKvers vers;
        private CKdlid dlid;
        private CKcolh colh;
        private Linsl linsl;
        private CKptbl ptbl;
        private Lwvpl wvpl;
        private LCInfo INFO;


        public DLS() : base("DLS ")
        {
            size = 4; // The size of the DLS file(number of bytes) less 8(less the size of "RIFF" and the "size")
            // Non optionnal
            colh = AddChunk(new CKcolh()) as CKcolh;
            linsl = AddChunk(new Linsl()) as Linsl;
            ptbl = AddChunk(new CKptbl()) as CKptbl;
            wvpl = AddChunk(new Lwvpl()) as Lwvpl;
        }

        public void SetName(string name)
        {
            _name = name;
        }


        public void AddInstrument(uint bank, uint instrumentId)
        {
            Lins instrument = new Lins(bank, instrumentId, "Instrument " + instrumentId);
            linsl.AddChunk(instrument);
        }

        public void AddInstrument(uint bank, uint instrumentId, string name)
        {
            Lins instrument = new Lins(bank, instrumentId, name);
            linsl.AddChunk(instrument);
        }

        public void AddInstrument(Lins dSLInstrument)
        {
            linsl.AddChunk(dSLInstrument);
        }

        public void AddWave(WAV wave)
        {
            wave.Riff = false;
            wvpl.AddChunk(wave);
        }

        internal bool WriteFile(string v)
        {
            colh.Instruments = (uint)linsl.chunks.Count;
            ptbl.Cues = (uint)wvpl.chunks.Count;

            // Wave Pool
            uint offset = 0;
            foreach (WAV wave in wvpl.chunks)
            {
                ptbl.AddCue(offset);
                offset += wave.GetPaddedSize();
            }

            Resize();
            return base.WriteFile(v, this.Write());
        }

    }



    /// <summary>
    /// Instruments
    /// </summary>
    public class CKcolh : Chunk, IChunk
    {
        private uint _cInstruments = 0;
        public CKcolh() : base("colh")
        {
            headerSize = 12;
        }

        public uint Instruments { get => _cInstruments; set => _cInstruments = value; }

        public new List<byte> Write()
        {
            SetData(BitConverter.GetBytes((uint)_cInstruments));

            List<byte> buffer = base.Write();
            return buffer;
        }
    }

    public class Linsl : LISTChunk, IChunk
    {
        public Linsl() : base("lins")
        {

        }
    }

    public class Lins : LISTChunk, IChunk
    {
        private string _name;
        private CKinsh _insh;
        private Lrgnl _lrgnl;
        private Lart _lart;

        public Lins() : base("insh")
        {
            _insh = AddChunk(new CKinsh()) as CKinsh;
            _lrgnl = AddChunk(new Lrgnl()) as Lrgnl;
        }

        public Lins(uint bank, uint instrumentId) : base("ins ")
        {

            _insh = AddChunk(new CKinsh(bank, instrumentId)) as CKinsh;
            _lrgnl = AddChunk(new Lrgnl()) as Lrgnl;

            _name = "Instrument " + instrumentId;
            RIFF.AlignName(_name);
        }
        public Lins(uint bank, uint instrumentId, string name) : base("ins ")
        {
            _insh = AddChunk(new CKinsh(bank, instrumentId)) as CKinsh;
            _lrgnl = AddChunk(new Lrgnl()) as Lrgnl;

            _name = name;
            RIFF.AlignName(_name);
        }
        public Lins(uint bank, uint instrumentId, string name, Lrgnl regions) : base("ins ")
        {
            _insh = AddChunk(new CKinsh(bank, instrumentId)) as CKinsh;
            _lrgnl = AddChunk(regions) as Lrgnl;

            _name = name;
            RIFF.AlignName(_name);
        }

        public Lrgnl regions { get => _lrgnl; }
        public ulong bank { get => _insh.bank; }
        public ulong instrumentId { get => _insh.instrumentId; }

        public void AddRegion(Lrgn region)
        {
            _lrgnl.AddChunk(region);
            _insh.regions = (uint)_lrgnl.chunks.Count;
        }
        /*
        public new List<byte> Write()
        {
            data = new List<byte>();
            data.AddRange(BitConverter.GetBytes((UInt32)_lrgnl.chunks.Count));
            data.AddRange(BitConverter.GetBytes((UInt32)_insh.bank));
            data.AddRange(BitConverter.GetBytes((UInt32)_insh.instrumentId));
            List<byte> buffer = base.Write();
            return buffer;
        }
        */
    }

    public class CKinsh : Chunk, IChunk
    {
        private uint cRegions = 0;      // Specifies the count of regions for this instrument.
        private uint ulBank = 0;        // Specifies the MIDI locale(Bank) for this instrument.
        private uint ulInstrument = 0;  // Specifies the MIDI locale(Program Change) for this instrument.

        public CKinsh() : base("insh")
        {
            headerSize = 12;
        }

        public CKinsh(uint BA, uint INS) : base("insh")
        {
            headerSize = 12;
            ulBank = BA;
            ulInstrument = INS;
        }

        public void SetMIDILoc(uint BA, uint INS)
        {
            ulBank = BA;
            ulInstrument = INS;
        }

        public uint bank { get => ulBank; }
        public uint instrumentId { get => ulInstrument; }
        public uint regions { get => cRegions; set => cRegions = value; }
        public new List<byte> Write()
        {
            AddDatas(BitConverter.GetBytes((uint)cRegions));
            AddDatas(BitConverter.GetBytes((uint)ulBank));
            AddDatas(BitConverter.GetBytes((uint)ulInstrument));

            List<byte> buffer = base.Write();
            return buffer;
        }
    }


    /// <summary>
    /// Regions
    /// </summary>
    public class Lrgnl : LISTChunk, IChunk
    {
        public Lrgnl():base("lrgn")
        {

        }
    }

    public class Lrgn : LISTChunk, IChunk
    {

        private CKrgnh _rgnh;
        private CKwlnk _wlnk;
        private CKwsmp _wsmp;
        private Lart _lart;  // Optionnal


        public Lrgn(ushort keyLow, ushort keyHigh, ushort velocityLow, ushort velocityHigh) : base("rgn ")
        {
            _rgnh = AddChunk(new CKrgnh(keyLow, keyHigh, velocityLow, velocityHigh)) as CKrgnh;
            //_lart = AddChunk(new Lart()) as Lart;
        }

        public void AddArticulation(CKart1 art)
        {
            if (_lart == null)
            {
                _lart = AddChunk(new Lart()) as Lart;
            }
            _lart.AddChunk(art);
        }

        public void SetSample(CKwsmp smp)
        {
            _wsmp = AddChunk(smp) as CKwsmp;
        }

        public void SetRange(ushort keyLow = 0x00, ushort keyHigh = 0x7F, ushort velocityLow = 0x00, ushort velocityHigh = 0x7F)
        {
            _rgnh.keyLow = keyLow;
            _rgnh.keyHigh = keyHigh;
            _rgnh.velocityLow = velocityLow;
            _rgnh.velocityHigh = velocityHigh;
        }

        public void SetWaveLink(CKwlnk wlk)
        {
            _wlnk = AddChunk(wlk) as CKwlnk;
        }

        public void SetWaveLinkInfo(ushort options, ushort phaseGroup, uint channel, uint index)
        {
            _wlnk = AddChunk(new CKwlnk(options, phaseGroup, channel, index)) as CKwlnk;
        }
        /*
        public new List<byte> Write()
        {
            data = new List<byte>();
            data.AddRange(BitConverter.GetBytes((UInt16)_rgnh.keyLow));
            data.AddRange(BitConverter.GetBytes((UInt16)_rgnh.keyHigh));
            data.AddRange(BitConverter.GetBytes((UInt16)_rgnh.velocityLow));
            data.AddRange(BitConverter.GetBytes((UInt16)_rgnh.velocityHigh));

            if (_wsmp != null)
            {
                AddChunk(_wsmp);
            }

            if (_wlnk != null)
            {
                AddChunk(_wlnk);
            }

            List<byte> buffer = base.Write();
            return buffer;
        }
        */
    }

    public class CKrgnh:Chunk, IChunk
    {
        private ushort _keyLow;         // Specifies the key range for this region.
        private ushort _keyHigh;        // Specifies the key range for this region.
        private ushort _velocityLow;    // Specifies the velocity range for this region.
        private ushort _velocityHigh;   // Specifies the velocity range for this region.
        private ushort _options;        // Specifies flag options for the synthesis of this region.
        // The only flag defined at this time is the Self Non Exclusive flag.See Note Exclusivity section for more detail.
        private ushort _keyGroup;
        /*
        Specifies the key group for a drum instrument. Key group values allow multiple
        regions within a drum instrument to belong to the same “key group.”
        If a synthesis engine is instructed to play a note with a key group setting and any
        other notes are currently playing with this same key group, the synthesis engine
        should turn off all notes with the same key group value as soon as possible.
        Valid values are:
        0 No Key group
        1-15 Key groups 1 to 15.
        All Others Reserved
        */

        public CKrgnh(ushort keyLow, ushort keyHigh, ushort velocityLow, ushort velocityHigh) : base("rgnh")
        {
            headerSize = 12;
            _keyLow = keyLow;
            _keyHigh = keyHigh;
            _velocityLow = velocityLow;
            _velocityHigh = velocityHigh;
        }


        public void SetRange(ushort keyLow = 0x00, ushort keyHigh = 0x7F, ushort velocityLow = 0x00, ushort velocityHigh = 0x7F)
        {
            _keyLow = keyLow;
            _keyHigh = keyHigh;
            _velocityLow = velocityLow;
            _velocityHigh = velocityHigh;
        }

        public ushort keyLow { get => _keyLow; set =>_keyLow = value; }
        public ushort keyHigh { get => _keyHigh; set => _keyHigh = value; }
        public ushort velocityLow { get => _velocityLow; set => _velocityLow = value; }
        public ushort velocityHigh { get => _velocityHigh; set => _velocityHigh = value; }
        public ushort option { get => _options; set => _options = value; }
        public ushort keyGroup { get => _keyGroup; set => _keyGroup = value; }

        public new List<byte> Write()
        {
            AddDatas(BitConverter.GetBytes((ushort)_keyLow));
            AddDatas(BitConverter.GetBytes((ushort)_keyHigh));
            AddDatas(BitConverter.GetBytes((ushort)_velocityLow));
            AddDatas(BitConverter.GetBytes((ushort)_velocityHigh));
            AddDatas(BitConverter.GetBytes((ushort)_options));
            AddDatas(BitConverter.GetBytes((ushort)_keyGroup));

            List<byte> buffer = base.Write();
            return buffer;
        }
    }

    /// <summary>
    /// Articulations
    /// </summary>
    public class Lart : LISTChunk, IChunk
    {
        public Lart() : base("lart")
        {

        }
    }

    public class CKart1 : Chunk, IChunk
    {
        public uint cbSize = 8;
        public uint cConnectionBlocks;
        public List<ConnectionBlock> ConnectionBlocks;

        public CKart1() : base("art1")
        {
            headerSize = 8;
            ConnectionBlocks = new List<ConnectionBlock>();
        }
        public CKart1(List<ConnectionBlock> connections) : base("art1")
        {
            headerSize = 8;
            ConnectionBlocks = connections;
            size += (uint)connections.Count * 12;
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
            AddDatas(BitConverter.GetBytes((uint)cbSize));
            AddDatas(BitConverter.GetBytes((uint)cConnectionBlocks));
            foreach(ConnectionBlock cb in ConnectionBlocks)
            {
                AddDatas(BitConverter.GetBytes((ushort)cb.usSource));
                AddDatas(BitConverter.GetBytes((ushort)cb.usControl));
                AddDatas(BitConverter.GetBytes((ushort)cb.usDestination));
                AddDatas(BitConverter.GetBytes((ushort)cb.usTransform));
                AddDatas(BitConverter.GetBytes((int)cb.lScale));
            }
            List<byte> buffer = base.Write();
            return buffer;
        }
    }


    public class ConnectionBlock
    {
        /*
Cid#        Articulator Name        usSource        usControl       usDestination       usTransform
LFO Section
1*          LFO Frequency           SRC_NONE        SRC_NONE        DST_LFO_FREQ        TRN_NONE
2*          LFO Start Delay         SRC_NONE        SRC_NONE        DST_LFO_DELAY       TRN_NONE
3*          LFO Attenuation Scale   SRC_LFO         SRC_NONE        DST_ATTENUATION     TRN_NONE
4           LFO Pitch Scale         SRC_LFO         SRC_NONE        DST_PITCH           TRN_NONE
5           LFO Modw to Attenuation SRC_LFO         SRC_CC1         DST_ATTENUATION     TRN_NONE
6           LFO Modw to Pitch       SRC_LFO         SRC_CC1         DST_PITCH           TRN_NONE
EG1 Section
7* EG1 Attack Time SRC_NONE SRC_NONE DST_EG1_ATTACKTIME TRN_NONE
8* EG1 Decay Time SRC_NONE SRC_NONE DST_EG1_DECAYTIME TRN_NONE
9* EG1 Sustain Level SRC_NONE SRC_NONE DST_EG1_SUSTAINLEVEL TRN_NONE
10* EG1 Release Time SRC_NONE SRC_NONE DST_EG1_RELEASETIME TRN_NONE
11 EG1 Velocity to Attack SRC_KEYONVELOCITY SRC_NONE DST_EG1_ATTACKTIME TRN_NONE
12 EG1 Key to Decay SRC_KEYNUMBER SRC_NONE DST_EG1_DECAYTIME TRN_NONE
EG2 Section
13* EG2 Attack Time SRC_NONE SRC_NONE DST_EG2_ATTACKTIME TRN_NONE
14* EG2 Decay Time SRC_NONE SRC_NONE DST_EG2_DECAYTIME TRN_NONE
15* EG2 Sustain Level SRC_NONE SRC_NONE DST_EG2_SUSTAINLEVEL TRN_NONE
16* EG2 Release Time SRC_NONE SRC_NONE DST_EG2_RELEASETIME TRN_NONE
17 EG2 Velocity to Attack SRC_KEYONVELOCITY SRC_NONE DST_EG2_ATTACKTIME TRN_NONE
18 EG2 Key to Decay SRC_KEYNUMBER SRC_NONE DST_EG2_DECAYTIME TRN_NONE
Miscellaneous Section
19* Initial Pan SRC_NONE SRC_NONE DST_PAN TRN_NONE
Connections inferred by DLS1 Architecture
20 EG1 To Attenuation SRC_EG1 SRC_NONE DST_ATTENUATION TRN_NONE
21 EG2 To Pitch SRC_EG2 SRC_NONE DST_PITCH TRN_NONE
22 Key On Velocity to Attenuation SRC_KEYONVELOCITY SRC_NONE DST_ATTENUATION TRN_CONCAVE
23 Pitch Wheel to Pitch SRC_PITCHWHEEL SRC_RPN0 DST_PITCH TRN_NONE
24 Key Number to Pitch SRC_KEYNUMBER SRC_NONE DST_PITCH TRN_NONE
25 MIDI Controller 7 to Atten. SRC_CC7 SRC_NONE DST_ATTENUATION TRN_CONCAVE
26 MIDI Controller 10 to Pan SRC_CC10 SRC_NONE DST_PAN TRN_NONE
27 MIDI Controller 11 to Atten. SRC_CC11 SRC_NONE DST_ATTENUATION TRN_CONCAVE
28 RPN1 to Pitch SRC_RPN1 SRC_NONE DST_PITCH TRN_NONE
29 RPN2 to Pitch SRC_RPN2 SRC_NONE DST_PITCH TRN_NONE
*/
        public ushort usSource;
        public ushort usControl;
        public ushort usDestination;
        public ushort usTransform;
        public int lScale;

        public ConnectionBlock(ushort source, ushort control, ushort destination, ushort transform, int scale)
        {
            usSource = source;
            usControl = control;
            usDestination = destination;
            usTransform = transform;
            lScale = scale;
        }
    }

    /// <summary>
    /// Samples
    /// </summary>
    public class Lwvpl : LISTChunk, IChunk
    {
        public Lwvpl() : base("wvpl")
        {

        }

        public List<WAV> Waves
        {
            get
            {
                List<WAV> ws = new List<WAV>();
                foreach (IChunk ck in chunks)
                {
                    ws.Add(ck as WAV);
                }
                return ws;
            }
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
            headerSize = 12;
        }
        public CKwlnk(ushort OP, ushort PG, uint CHA, uint TBI) : base("wlnk")
        {
            headerSize = 12;
            options = OP;
            phaseGroup = PG;
            channel = CHA;
            tableIndex = TBI;
        }

        public new List<byte> Write()
        {
            AddDatas(BitConverter.GetBytes((ushort)options));
            AddDatas(BitConverter.GetBytes((ushort)phaseGroup));
            AddDatas(BitConverter.GetBytes((uint)channel));
            AddDatas(BitConverter.GetBytes((uint)tableIndex));

            List<byte> buffer = base.Write();
            return buffer;
        }
    }

    public class CKwsmp : Chunk, IChunk // Wave Sample Chunk
    {
        public uint cbSize = 20;
        public ushort unityNote;
        public short fineTune;
        public int attenuation;
        public uint options;
        public uint sampleLoops = 0;
        public Loop loop;

        public CKwsmp() : base("wsmp")
        {
            headerSize = 20;
        }

        public CKwsmp(ushort UN, short FT, int AT, uint OP) : base("wsmp")
        {
            headerSize = 20;
            unityNote = UN;
            fineTune = FT;
            attenuation = AT;
            options = OP;
        }

        public void SetPitchInfo(ushort UN, short FT, int AT, uint OP)
        {
            unityNote = UN;
            fineTune = FT;
            attenuation = AT;
            options = OP;
        }
        public void AddLoop(Loop LP)
        {
            loop = LP;
            sampleLoops = 1;
            headerSize = 20 + 16;
        }


        public new List<byte> Write()
        {
            AddDatas(BitConverter.GetBytes((uint)cbSize));
            AddDatas(BitConverter.GetBytes((ushort)unityNote));
            AddDatas(BitConverter.GetBytes((short)fineTune));
            AddDatas(BitConverter.GetBytes((int)attenuation));
            AddDatas(BitConverter.GetBytes((uint)options));
            AddDatas(BitConverter.GetBytes((uint)sampleLoops));
            if (loop != null)
            {
                AddDatas(BitConverter.GetBytes((uint)loop.cbSize));
                AddDatas(BitConverter.GetBytes((uint)loop.loopType));
                AddDatas(BitConverter.GetBytes((uint)loop.loopStart));
                AddDatas(BitConverter.GetBytes((uint)loop.loopLength));
            }
            List<byte> buffer = base.Write();
            return buffer;
        }
    }

    public class Loop
    {
        public uint cbSize = 12;
        public uint loopType; // Specifies the loop type : WLOOP_TYPE_FORWARD Forward Loop
        public uint loopStart; // Specifies the start point of the loop in samples as an absolute offset from the beginning of the data in the<data-ck> subchunk of the<wave-list> wave file chunk.
        public uint loopLength; // Specifies the length of the loop in samples.

        public Loop(uint LT = 0, uint LS = 0, uint LL = 0)
        {
            loopType = LT;
            loopStart = LS;
            loopLength = LL;
        }
    }

    public class CKptbl : Chunk, IChunk // Pool Table Chunk
    {
        public uint cbSize = 8;
        public uint _cCues; // Specifies the number (count) of <poolcue> records that are contained in the <ptbl-ck>
        // chunk.The<poolcue> records are stored immediately following the cCues data field.
        public List<uint> poolcues;

        public CKptbl() : base("ptbl")
        {
            headerSize = 28;
            poolcues = new List<uint>();
        }

        public uint Cues { get => _cCues; internal set => _cCues = value; }

        public void AddCue(uint offset)
        {
            poolcues.Add(offset);
            _cCues = (uint)poolcues.Count;
        }
        public void AddCues(List<uint> offsets)
        {
            poolcues.AddRange(offsets);
            _cCues = (uint)poolcues.Count;
        }

        public new List<byte> Write()
        {
            AddDatas(BitConverter.GetBytes((uint)cbSize));
            AddDatas(BitConverter.GetBytes((uint)_cCues));
            foreach (uint l in poolcues)
            {
                AddDatas(BitConverter.GetBytes((uint)l));
            }

            List<byte> buffer = base.Write();
            return buffer;
        }
    }

    /// <summary>
    /// Misc
    /// </summary>
    public class CKdlid : Chunk, IChunk
    {
        public CKdlid() : base("dlid")
        {

        }
    }

    public class CKvers : Chunk, IChunk
    {
        public uint versionMS;
        public uint versionLS;

        public CKvers(uint maj, uint min) : base("vers")
        {
            versionMS = maj;
            versionLS = min;
            AddDatas(BitConverter.GetBytes((uint)versionMS));
            AddDatas(BitConverter.GetBytes((uint)versionLS));
        }
    }

    /// <summary>
    /// See Page 23 / 97 RIFF doc
    /// </summary>
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
    } //TODO

}