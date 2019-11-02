using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using VS.Utils;
using System.Linq;

//Minoru Akao
//https://github.com/vgmtrans/vgmtrans/blob/master/src/main/formats/AkaoSeq.cpp

// Akao in MUSIC folder contains music instructions like a Midi file, Akao in SOUND folder contains samples collection like .sfb / .sf2 / .dls files
// Musics need to use a sample collection to give a sampled music sound or else a midi like sound will be created.
namespace VS.Parser
{

    public class AKAO : FileParser
    {
        public static readonly AKAOType SOUND = AKAOType.SOUND;
        public static readonly AKAOType MUSIC = AKAOType.MUSIC;

        public enum AKAOType { SOUND, MUSIC }

        public AudioClip audioClip;
        public AKAOType _type;
        public AKAOInstrument[] instruments;
        public AKAODrum drum;
        public AKAOArticulation[] articulations;
        public AKAOSample[] samples;
        public AKAOComposer composer;

        public void Parse(string filePath, AKAOType type )
        {
            PreParse(filePath);
            _type = type;
            if (FileSize < 4)
            {
                return;
            }
            Parse(buffer);

            buffer.Close();
            fileStream.Close();
        }

        public void Parse(BinaryReader buffer)
        {
            if (buffer.BaseStream.Length < 4)
            {
                return;
            }

            switch(_type)
            {
                case AKAOType.MUSIC:
                    // header datas
                    byte[] header = buffer.ReadBytes(4);// AKAO
                    ushort fileId = buffer.ReadUInt16();
                    ushort byteLen = buffer.ReadUInt16();
                    ushort reverb = buffer.ReadUInt16(); // 0x0500  | Just on case 0x0400 (MUSIC000.DAT)
                    buffer.ReadBytes(6); // padding
                    
                    uint unk1 = buffer.ReadUInt32();
                    uint sampleSet = buffer.ReadUInt32(); // ID of the WAVE*.DAT in the SOUND folder
                    buffer.ReadBytes(8); // padding

                    int bnumt = buffer.ReadInt32();
                    short unk3 = buffer.ReadInt16();
                    buffer.ReadBytes(10); // padding

                    int ptr1 = buffer.ReadUInt16() + 0x30; // instruments pointer
                    buffer.ReadUInt16(); // always 0000
                    int ptr2 = buffer.ReadUInt16() + 0x34; // Drums pointer
                    buffer.ReadBytes(10); // padding

                    ushort jump = buffer.ReadUInt16();
                    long jumpTo = buffer.BaseStream.Position + jump - 2;
                    uint numTrack = ToolBox.GetNumPositiveBits(bnumt);
                    // kind of listing here, i don't know what it is.

                    if (UseDebug)
                    {
                        Debug.Log("AKAO from : " + FileName + " FileSize = " + FileSize+ "    |    reverb : " + reverb + " numTrack : " + numTrack + " sampleSet : " + sampleSet);
                        Debug.Log("instruments at : " + ptr1 + "  Drums at : " + ptr2+ "   |    unk1 : " + unk1  + "  unk3 : " + unk3+ "   Jump to : "+ jumpTo);
                    }
                    buffer.BaseStream.Position = jumpTo;


                    // music instuctions begin here, MIDI like format
                    long basePtr = buffer.BaseStream.Position;
                    /*
                    MemoryStream memDat = new MemoryStream();
                    buffer.BaseStream.CopyTo(memDat, ptr1);
                    BinaryReader stream = new BinaryReader(memDat);
                    */

                    // Instruments
                    if (ptr1 > 0x30)
                    {
                        buffer.BaseStream.Position = ptr1;
                        // Instruments Header always 0x20 ?
                        List<ushort> instrPtrs = new List<ushort>();
                        for (int i = 0; i < 0x10; i++)
                        {
                            ushort instrPtr = buffer.ReadUInt16();
                            if (instrPtr != 0xFFFF)
                            {
                                //if (UseDebug) Debug.Log("Instrument "+ instrPtrs.Count+ " ptr : " + instrPtr);
                                instrPtrs.Add(instrPtr);
                            } else
                            {
                                // Padding
                            }
                        }

                        if (UseDebug) Debug.Log("Instruments number : "+ instrPtrs.Count);

                        instruments = new AKAOInstrument[instrPtrs.Count];
                        for (int i = 0; i < instrPtrs.Count; i++)
                        {
                            AKAOInstrument instrument = new AKAOInstrument();
                            long instrStart = ptr1 + 0x20 + instrPtrs[i];
                            long instrEnd;
                            if (i < instrPtrs.Count -1)
                            {
                                instrEnd = ptr1 + 0x20 + instrPtrs[i + 1];
                            } else
                            {
                                if (ptr2 > 0x34)
                                {
                                    instrEnd = ptr2;
                                } else
                                {
                                    instrEnd = byteLen;
                                }
                            }
                            int instrRegLoop = (int)(instrEnd - instrStart) / 0x08;
                            instrument.regions = new AKAOInstrumentRegion[instrRegLoop - 1]; // -1 because the last 8 bytes are padding
                            for (int j = 0; j < instrRegLoop - 1 ; j++)
                            {
                                instrument.regions[j] = new AKAOInstrumentRegion(buffer.ReadBytes(8));
                            }
                            buffer.ReadBytes(8);// 0000 0000 0000 0000 padding

                            instruments[i] = instrument;
                        }
                    }
                    // Drum
                    if (ptr2 > 0x34)
                    {
                        if (buffer.BaseStream.Position != ptr2)
                        {
                            if (UseDebug)
                            {
                                Debug.Log(buffer.BaseStream.Position + "  --  " + ptr2);
                            }
                            buffer.BaseStream.Position = ptr2;
                        }

                        drum = new AKAODrum();
                        drum.hasDrum = true;
                        int drumRegLoop = (int)(byteLen - ptr2) / 0x08;

                        List<AKAODrumRegion> dr = new List<AKAODrumRegion>();
                        for (int j = 0; j < drumRegLoop - 1; j++)
                        {
                            byte[] b = buffer.ReadBytes(8);
                            if (b[0] == 0xFF && b[1] == 0xFF && b[2] == 0xFF && b[3] == 0xFF && b[4] == 0xFF && b[5] == 0xFF && b[6] == 0xFF && b[7] == 0xFF)
                            {
                                break;
                            }
                            if (b[0] > 0 && b[1] > 0 && b[6] > 0 && b[7] > 0)
                            {
                                dr.Add(new AKAODrumRegion(b));
                            }
                        }

                        drum.regions = dr.ToArray();
                    }

                    // So we seek for the appropriate WAVE*.DAT in the SOUND folder
                    string[] hash = FilePath.Split("/"[0]);
                    hash[hash.Length - 2] = "SOUND";
                    string zz = "0";
                    if (sampleSet < 100)
                    {
                        zz += "0";
                    }
                    if (sampleSet < 10)
                    {
                        zz += "0";
                    }
                    hash[hash.Length - 1] = "WAVE"+zz+ sampleSet+ ".DAT";
                    string samplePath = String.Join("/", hash);
                    bool test = File.Exists(samplePath);
                    if (UseDebug)
                    {
                        Debug.Log("Seek for : "+ samplePath + " -> "+test);
                    }

                    AKAO sampleParser = new AKAO();
                    //sampleParser.UseDebug = true;
                    sampleParser.Parse(samplePath, AKAO.SOUND);

                    composer = new AKAOComposer(buffer, basePtr, ptr1, instruments, drum, sampleParser.articulations, sampleParser.samples, numTrack, FileName);


                    break;
                case AKAOType.SOUND:
                    // Samples Collection
                    // header datas
                    header = buffer.ReadBytes(4);// AKAO
                    ushort sampleId = buffer.ReadUInt16();
                    buffer.ReadBytes(10); // padding

                    reverb = buffer.ReadUInt16(); // 0x0031 - 0x0051
                    buffer.ReadBytes(2); // padding
                    var sampleSize = buffer.ReadUInt32();
                    var stArtId = buffer.ReadUInt32();
                    var numArts = buffer.ReadUInt32();

                    buffer.ReadBytes(32); // padding

                    if (UseDebug)
                    {
                        Debug.Log("AKAO from : " + FileName + " len = " + FileSize);
                        Debug.Log("ID : " + sampleId + " reverb : " + reverb + " sampleSize : " + sampleSize + " stArtId : " + stArtId + " numArts : " + numArts);
                    }

                    // Articulations section here
                    articulations = new AKAOArticulation[numArts];
                    for (uint i = 0; i < numArts; i++)
                    {
                        AKAOArticulation arti = new AKAOArticulation(buffer);
                        articulations[i] = arti;
                    }

                    // Samples section here
                    long samStart = buffer.BaseStream.Position;
                    // First we need to determine the start and the end of the samples, 16 null bytes indicate a new sample, so lets find them.
                    List<long> samPtr = new List<long>();
                    List<long> samEPtr = new List<long>();
                    while(buffer.BaseStream.Position < buffer.BaseStream.Length - 0x30)
                    {
                        if (buffer.ReadUInt64() + buffer.ReadUInt64() == 0)
                        {
                            if (samPtr.Count > 0)
                            {
                                samEPtr.Add(buffer.BaseStream.Position - 16);
                            }
                            samPtr.Add(buffer.BaseStream.Position);
                        }
                    }
                    samEPtr.Add(buffer.BaseStream.Length);
                    // Let's loop again to get samples
                    int numSam = samPtr.Count;
                    samples = new AKAOSample[numSam];
                    for (int i = 0; i < numSam; i++)
                    {
                        buffer.BaseStream.Position = samPtr[i];
                        AKAOSample sam = new AKAOSample(((int)samEPtr[i] - (int)samPtr[i] - 2), buffer);
                        samples[i] = sam;
                    }
                    break;
            }

        }
    }

    // VGMTrans schema
    public class AKAOInstrument
    {
        public AKAOInstrumentRegion[] regions;

        public AKAOInstrument()
        {

        }
    }
    public class AKAOInstrumentRegion
    {
        public byte articulationId;
        public byte lowRange;
        public byte hiRange;
        public byte unk1;
        public byte unk2;
        public byte unk3;
        public byte unk4;
        public byte volume;

        public AKAOInstrumentRegion(byte[] b)
        {
            articulationId = b[0];
            lowRange = b[1];
            hiRange = b[2];
            unk1 = b[3];
            unk2 = b[4];
            unk3 = b[5];
            unk4 = b[6];
            volume = b[7];
        }
        public AKAOInstrumentRegion(BinaryReader buffer)
        {
            articulationId = buffer.ReadByte();
            lowRange = buffer.ReadByte();
            hiRange = buffer.ReadByte();
            unk1 = buffer.ReadByte();
            unk2 = buffer.ReadByte();
            unk3 = buffer.ReadByte();
            unk4 = buffer.ReadByte();
            volume = buffer.ReadByte();
        }
    }
    public class AKAODrum
    {
        public bool hasDrum = false;
        public AKAODrumRegion[] regions;

        public AKAODrum()
        {

        }
    }
    public class AKAODrumRegion
    {
        public byte articulationId;
        public byte relativeKey;
        public byte unk1;
        public byte unk2;
        public byte unk3;
        public byte unk4;
        public byte attenuation;
        public byte pan;

        public AKAODrumRegion(byte[] b)
        {
            articulationId = b[0];
            relativeKey = b[1];
            unk1 = b[2];
            unk2 = b[3];
            unk3 = b[4];
            unk4 = b[5];
            attenuation = b[6];
            pan = b[7];
        }
        public AKAODrumRegion(BinaryReader buffer)
        {
            articulationId = buffer.ReadByte();
            relativeKey = buffer.ReadByte();
            unk1 = buffer.ReadByte();
            unk2 = buffer.ReadByte();
            unk3 = buffer.ReadByte();
            unk4 = buffer.ReadByte();
            attenuation = buffer.ReadByte();
            pan = buffer.ReadByte();
        }
    }
    public class AKAOArticulation
    {
        public uint sampleOff;
        public uint loopPt;
        public ushort fineTune;
        public ushort unityKey;
        public ushort adr1;
        public ushort adr2;


        public AKAOArticulation(BinaryReader buffer)
        {

            sampleOff = buffer.ReadUInt32();
            loopPt = buffer.ReadUInt32();
            fineTune = buffer.ReadUInt16();
            unityKey = buffer.ReadUInt16();
            adr1 = buffer.ReadUInt16();
            adr2 = buffer.ReadUInt16();
        }
    }
    public class AKAOSample
    {
        public static float[,] coeff = {
            { 0.0f, 0.0f },
            { 60.0f / 64.0f, 0.0f },
            { 115.0f / 64.0f, 52.0f / 64.0f },
            { 98.0f / 64.0f, 55.0f / 64.0f },
            { 122.0f / 64.0f, 60.0f / 64.0f }
        };

        public int range;
        public int filter;
        public int end;
        public int looping;
        public int loop;
        public float[] data;
        public float[] wave;

        public AKAOSample(int size, BinaryReader buffer)
        {
            byte a = buffer.ReadByte();
            byte b = buffer.ReadByte();

            range = a & 0xF;
            filter = (a & 0xF0) >> 4;
            end = b & 0x1;
            looping = b & 0x2;
            loop = b & 0x4;
            data = new float[size];
            for (int i = 0; i < size; i++) data[i] = buffer.ReadByte();
        }

        public float[] decompressData(float prev1, float prev2)
        {
            int i;
            float t;
            float f1, f2;
            float p1, p2;
            var shift = range + 16;
            wave = new float[29];

            for (i = 0; i < 14; i++)
            {
                wave[i * 2] = ((int)data[i] << 28) >> shift;
                wave[i * 2 + 1] = (((int)data[i] & 0xF0) << 24) >> shift;
            }

            i = filter;
            if (i > 0)
            {
                f1 = AKAOSample.coeff[i, 0];
                f2 = AKAOSample.coeff[i, 1];
                p1 = prev1;
                p2 = prev2;
                for (i = 0; i < 28; i++)
                {
                    t = wave[i] + (p1 * f1) - (p2 * f2);
                    wave[i] = t;
                    p2 = p1;
                    p1 = t;
                }
                prev1 = p1;
                prev2 = p2;
            }
            else
            {
                prev1 = wave[26];
                prev2 = wave[27];
            }

            float[] ret = { prev1 / 0x80000000, prev2 / 0x80000000 };
            return ret;
        }
    }
    public class AKAOComposer
    {
        public static readonly ushort[] delta_time_table = { 0xC0, 0x60, 0x30, 0x18, 0x0C, 0x6, 0x3, 0x20, 0x10, 0x8, 0x4, 0x0, 0xA0A0, 0xA0A0 };

        public enum ComposerMode { MIDI, SAMPLED }

        public AKAOInstrument[] instruments;
        public AKAODrum drum;
        public AKAOArticulation[] articulations;
        public AKAOSample[] samples;
        public ComposerMode MODE = ComposerMode.MIDI;

        private BinaryReader buffer;
        private string name;
        private long start;
        private long end;
        private uint numTrack;
        private AKAOTrack[] tracks;


        private uint velocity = 127;
        private int quarterNote = 0x30;

        public AKAOComposer(BinaryReader buffer, long start, long end, AKAOInstrument[] instruments, AKAODrum drum, AKAOArticulation[] articulations, AKAOSample[] samples, uint numTrack, string name)
        {
            this.buffer = buffer;
            this.instruments = instruments;
            this.drum = drum;
            this.articulations = articulations;
            this.samples = samples;
            this.numTrack = numTrack;
            this.name = name;

            this.start = start;
            this.end = end;

            buffer.BaseStream.Position = start;
            SetTracks();
        }


        public void BuildAudioClip()
        {
            /*
            int channel = 1;
            int sampleRate = 44100;
            int bufferSize = 1024;

            float[] datas = new float[256];

            AudioClip audio = AudioClip.Create(name, datas.Length, 1, 44100, false);
            audio.SetData(datas, 0);

            AssetDatabase.CreateAsset(audio, "Assets/Resources/Sounds/"+name+".mid");
            */
        }

        public void OutputMidiFile()
        {
            List<byte> midiByte = new List<byte>();
            midiByte.AddRange(new byte[] { 0x4D, 0x54, 0x68, 0x64 }); // MThd Header
            midiByte.AddRange(new byte[] { 0x00, 0x00, 0x00, 0x06 }); // Chunck length
            midiByte.AddRange(new byte[] { 0x00, 0x01 }); // Format Midi 1
            midiByte.Add((byte)(((byte)(tracks.Length) & 0xFF00) >> 8)); //num tracks hi
            midiByte.Add((byte)((byte)(tracks.Length) & 0x00FF)); //num tracks lo
            midiByte.Add((byte)((quarterNote & 0xFF00) >> 8)); //Per Quarter Note hi
            midiByte.Add((byte)(quarterNote & 0x00FF)); //Per Quarter Note lo
            foreach (AKAOTrack track in tracks)
            {
                midiByte.AddRange(new byte[] { 0x4D, 0x54, 0x72, 0x6B }); // MTrk Header
                List<byte> tb = new List<byte>();
                foreach (AKAOEvent ev in track.Events)
                {
                    List<byte> evb = ev.GetMidiBytes();
                    if (evb != null)
                    {
                        tb.AddRange(evb);
                    }
                }
                midiByte.AddRange(new byte[] { (byte)((tb.Count+4 & 0xFF000000) >> 24), (byte)((tb.Count+4 & 0x00FF0000) >> 16), (byte)((tb.Count+4 & 0x0000FF00) >> 8), (byte)(tb.Count+4 & 0x000000FF) }); // Chunck length

                midiByte.AddRange(tb); // Track datas
                midiByte.AddRange(new byte[] { 0x00, 0xFF, 0x2F, 0x00 }); // End Track
            }
            
            using (FileStream fs = File.Create("Assets/Resources/Sounds/" + name + ".mid"))
            {
                for (int i = 0; i < midiByte.Count; i++)
                {
                    fs.WriteByte(midiByte[i]);
                }
                fs.Close();
            }
        }

        private void SetTracks()
        {
            //Debug.Log("SetTracks : "+ numTrack);
            tracks = new AKAOTrack[numTrack];
            for (uint i = 0; i < numTrack; i++)
            {
                tracks[i] = new AKAOTrack();
            }

            uint cTrackId = 0;
            bool playingNote = false;
            uint prevKey = 0;
            ushort delta = 0;
            uint channel = 0;
            uint octave = 0;

            while (buffer.BaseStream.Position < this.end)
            {
                AKAOTrack curTrack;
                if (cTrackId < tracks.Length)
                {
                    curTrack = tracks[cTrackId];
                    channel = cTrackId % 0xF;
                } else
                {
                    curTrack = tracks[tracks.Length - 1]; // using the last track instead
                    channel = cTrackId % 0xF;
                }
                byte STATUS_BYTE = buffer.ReadByte();
                int i, k;

                if (STATUS_BYTE < 0x9A)
                {
                    i = STATUS_BYTE / 11;
                    k = i * 2;
                    k += i;
                    k *= 4;
                    k -= i;
                    k = STATUS_BYTE - k;

                    if (STATUS_BYTE < 0x83) // Note On
                    {
                        if (playingNote)
                        {
                            curTrack.AddEvent(new EvNoteOff(channel, prevKey, delta));
                            delta = 0;
                            playingNote = false;
                        }
                        uint relativeKey = (uint)i;
                        uint baseKey = octave * 12;
                        uint key = baseKey + relativeKey;
                        curTrack.AddEvent(new EvNoteOn(channel, key, velocity, delta));
                        delta = delta_time_table[k];
                        prevKey = key;
                        playingNote = true;
                    }
                    else if (STATUS_BYTE < 0x8F) // Tie
                    {
                        curTrack.AddEvent(new EvTie(delta_time_table[k]));
                    } 
                    else // Rest
                    {
                        if (playingNote)
                        {
                            curTrack.AddEvent(new EvNoteOff(channel, prevKey, delta));
                            delta = 0;
                            playingNote = false;
                        }
                        uint duration = delta_time_table[k];
                        curTrack.AddEvent(new EvRest(duration));
                        delta = (ushort)duration;
                    }
                }
                else if ((STATUS_BYTE >= 0xF0) && (STATUS_BYTE <= 0xFB)) // Alternate Note On ?
                {
                    if (playingNote)
                    {
                        curTrack.AddEvent(new EvNoteOff(channel, prevKey, delta));
                        delta = 0;
                        playingNote = false;
                    }
                    uint relativeKey = (uint)STATUS_BYTE - 0xF0;
                    uint baseKey = octave * 12;
                    uint key = baseKey + relativeKey;
                    uint time = buffer.ReadByte();
                    curTrack.AddEvent(new EvNoteOn(channel, key, velocity, delta));
                    delta = (ushort)time;
                    prevKey = key;
                    playingNote = true;

                }
                else
                {
                    switch(STATUS_BYTE)
                    {
                        case 0xA0:
                            curTrack.AddEvent(new EvEndTrack());
                            break;
                        case 0xA1:// Program Change
                            uint articulationId = buffer.ReadByte();
                            curTrack.AddEvent(new EvProgramChange(channel, articulationId));
                            break;
                        case 0xA2: // Unknown
                            curTrack.AddEvent(new EvUnknown(STATUS_BYTE));
                            break;
                        case 0xA3:// Volume
                            uint volume = buffer.ReadByte();
                            curTrack.AddEvent(new EvVolume(channel, volume));
                            break;
                        case 0xA4:// Portamento
                            curTrack.AddEvent(new EvPortamento(channel));
                            break;
                        case 0xA5:// Octave
                            octave = buffer.ReadByte();
                            curTrack.AddEvent(new EvOctave(octave));
                            break;
                        case 0xA6:// Octave ++
                            octave++;
                            curTrack.AddEvent(new EvOctaveUp());
                            break;
                        case 0xA7:// Octave --
                            octave--;
                            curTrack.AddEvent(new EvOctaveDown());
                            break;
                        case 0xA8:// Expression
                            uint expression = buffer.ReadByte();
                            curTrack.AddEvent(new EvExpr(channel, expression));
                            break;
                        case 0xA9:// Expression Slide
                            uint duration = buffer.ReadByte();
                            expression = buffer.ReadByte();
                            curTrack.AddEvent(new EvExprSlide(duration, expression));
                            break;
                        case 0xAA:// Pan
                            int pan = buffer.ReadByte();
                            curTrack.AddEvent(new EvPan(channel, pan));
                            break;
                        case 0xAB:// Pan Fade
                            duration = buffer.ReadByte();
                            pan = buffer.ReadByte();
                            curTrack.AddEvent(new EvPanSlide(duration, pan));
                            break;
                        case 0xAC: // Unknown
                            curTrack.AddEvent(new EvUnknown(STATUS_BYTE));
                            break;
                        case 0xAD: // Attack
                            int attack = buffer.ReadByte();
                            curTrack.AddEvent(new EvAttack(attack));
                            break;
                        case 0xAE: // Decay
                            int decay = buffer.ReadByte();
                            curTrack.AddEvent(new EvDecay(decay));
                            break;
                        case 0xAF: // Sustain
                            int sustain = buffer.ReadByte();
                            curTrack.AddEvent(new EvSustain(sustain));
                            break;
                        case 0xB0: // Decay + Sustain
                            decay = buffer.ReadByte();
                            sustain = buffer.ReadByte();
                            curTrack.AddEvent(new EvDecay(decay));
                            curTrack.AddEvent(new EvSustain(sustain));
                            break;
                        case 0xB1: // Sustain release
                            duration = buffer.ReadByte();
                            curTrack.AddEvent(new EvSustainRelease(duration));
                            break;
                        case 0xB2: // Release
                            duration = buffer.ReadByte();
                            curTrack.AddEvent(new EvRelease(duration));
                            break;
                        case 0xB3: // Reset ADSR (Attack-Decay-Sustain-Release)
                            curTrack.AddEvent(new EvResetADSR());
                            break;
                        // LFO (low-frequency oscillators) Pitch bend
                        case 0xB4: // LFO Pitch bend Range
                            byte[] b = buffer.ReadBytes(3);
                            curTrack.AddEvent(new EvLFOPitchRange(b[0], b[1], b[2]));
                            break;
                        case 0xB5: // LFO Pitch bend Depth
                            int depth = buffer.ReadByte();
                            curTrack.AddEvent(new EvLFOPitchDepth(depth));
                            break;
                        case 0xB6: // LFO Pitch bend Off
                            curTrack.AddEvent(new EvLFOPitchOff());
                            break;
                        case 0xB7: // LFO Pitch bend ??
                            curTrack.AddEvent(new EvUnknown(STATUS_BYTE));
                            break;
                        // LFO (low-frequency oscillators) Expression
                        case 0xB8: // LFO Expression Range
                            b = buffer.ReadBytes(3);
                            curTrack.AddEvent(new EvLFOExprRange(b[0], b[1], b[2]));
                            break;
                        case 0xB9: // LFO Expression Depth
                            depth = buffer.ReadByte();
                            curTrack.AddEvent(new EvLFOExprDepth(depth));
                            break;
                        case 0xBA: // LFO Expression Off
                            curTrack.AddEvent(new EvLFOExprOff());
                            break;
                        case 0xBB: // LFO Expression ??
                            curTrack.AddEvent(new EvUnknown(STATUS_BYTE));
                            break;
                        // LFO (low-frequency oscillators) Panpot
                        case 0xBC: // LFO Panpot Range
                            b = buffer.ReadBytes(3);
                            curTrack.AddEvent(new EvLFOPanpotRange(b[0], b[1], b[2]));
                            break;
                        case 0xBD: // LFO Panpot Depth
                            depth = buffer.ReadByte();
                            curTrack.AddEvent(new EvLFOPanpotDepth(depth));
                            break;
                        case 0xBE: // LFO Panpot Off
                            curTrack.AddEvent(new EvLFOPanpotOff());
                            break;
                        case 0xBF: // LFO Panpot ??
                            curTrack.AddEvent(new EvUnknown(STATUS_BYTE));
                            break;

                        case 0xC0: // Transpose
                            int transpose = buffer.ReadByte();
                            curTrack.AddEvent(new EvTranspose(transpose));
                            break;
                        case 0xC1: // Transpose Move
                            transpose = buffer.ReadByte();
                            curTrack.AddEvent(new EvTransposeMove(transpose));
                            break;
                        case 0xC2: // Reverb On
                            curTrack.AddEvent(new EvReverbOn());
                            break;
                        case 0xC3: // Reverb Off
                            curTrack.AddEvent(new EvReverbOff());
                            break;
                        case 0xC4: // Noise On
                            curTrack.AddEvent(new EvNoiseOn());
                            break;
                        case 0xC5: // Noise Off
                            curTrack.AddEvent(new EvNoiseOff());
                            break;
                        case 0xC6: // FM (Frequency Modulation) On
                            curTrack.AddEvent(new EvFMOn());
                            break;
                        case 0xC7: // FM (Frequency Modulation) Off
                            curTrack.AddEvent(new EvFMOff());
                            break;
                        case 0xC8: // Repeat Start
                            curTrack.AddEvent(new EvRepeatStart());
                            break;
                        case 0xC9: // Repeat End
                            int loopId = buffer.ReadByte();
                            curTrack.AddEvent(new EvRepeatEnd(loopId));
                            break;
                        case 0xCA: // Repeat End
                            curTrack.AddEvent(new EvRepeatEnd());
                            break;
                        case 0xCB: // Unknown
                            curTrack.AddEvent(new EvUnknown(STATUS_BYTE));
                            break;
                        case 0xCC: // Slur On
                            curTrack.AddEvent(new EvSlurOn());
                            break;
                        case 0xCD: // Slur Off
                            curTrack.AddEvent(new EvSlurOff());
                            break;
                        case 0xCE: // Unknown
                            curTrack.AddEvent(new EvUnknown(STATUS_BYTE));
                            break;
                        case 0xCF: // Unknown
                            curTrack.AddEvent(new EvUnknown(STATUS_BYTE));
                            break;
                        case 0xD0: // Note Off
                            curTrack.AddEvent(new EvNoteOff());
                            break;
                        case 0xD1: // Desactivate Notes ?
                            curTrack.AddEvent(new EvUnknown(STATUS_BYTE));
                            break;
                        case 0xD2: // Unknown
                            curTrack.AddEvent(new EvUnknown(STATUS_BYTE, buffer.ReadByte()));
                            break;
                        case 0xD3: // Unknown
                            curTrack.AddEvent(new EvUnknown(STATUS_BYTE, buffer.ReadByte()));
                            break;
                        case 0xD4: // Unknown
                            curTrack.AddEvent(new EvUnknown(STATUS_BYTE));
                            break;
                        case 0xD5: // Unknown
                            curTrack.AddEvent(new EvUnknown(STATUS_BYTE));
                            break;
                        case 0xD6: // Unknown
                            curTrack.AddEvent(new EvUnknown(STATUS_BYTE));
                            break;
                        case 0xD7: // Unknown
                            curTrack.AddEvent(new EvUnknown(STATUS_BYTE));
                            break;
                        case 0xD8: // Pitch Bend
                            uint value = buffer.ReadByte();
                            uint fullValue = (uint) (value * 64.503937007874015748031496062992f);
                            fullValue += 0x2000;
                            uint high = fullValue & 0x7F;
                            uint low = (fullValue & 0x3F80) << 7;
                            curTrack.AddEvent(new EvPitchBend(channel, low, high));
                            break;
                        case 0xD9: // Pitch Bend Move
                            value = buffer.ReadByte();
                            curTrack.AddEvent(new EvPitchBendMove(value));
                            break;
                        case 0xDA: // Unknown
                            curTrack.AddEvent(new EvUnknown(STATUS_BYTE, buffer.ReadByte()));
                            break;
                        case 0xDB: // Unknown
                            curTrack.AddEvent(new EvUnknown(STATUS_BYTE));
                            break;
                        case 0xDC: // Unknown
                            curTrack.AddEvent(new EvUnknown(STATUS_BYTE, buffer.ReadByte()));
                            break;
                        case 0xDD: // LFO Pitch Bend times
                            b = buffer.ReadBytes(2);
                            curTrack.AddEvent(new EvUnknown(STATUS_BYTE));
                            break;
                        case 0xDE: // LFO Expression times
                            b = buffer.ReadBytes(2);
                            curTrack.AddEvent(new EvUnknown(STATUS_BYTE));
                            break;
                        case 0xDF: // LFO Panpot times
                            b = buffer.ReadBytes(2);
                            curTrack.AddEvent(new EvUnknown(STATUS_BYTE));
                            break;
                        case 0xE1: // Unknown
                            curTrack.AddEvent(new EvUnknown(STATUS_BYTE, buffer.ReadByte()));
                            break;
                        case 0xE6: // LFO Expression times
                            b = buffer.ReadBytes(2);
                            curTrack.AddEvent(new EvUnknown(STATUS_BYTE));
                            break;
                        case 0xFC: // Tie
                            value = buffer.ReadByte();
                            curTrack.AddEvent(new EvTieTime(value));
                            break;
                        case 0xFD: // Rest
                            // maybe we need to turn off current playing note ?
                            duration = buffer.ReadByte();
                            curTrack.AddEvent(new EvRest(duration));
                            break;
                        case 0xFE: // Meta Event
                            byte Meta = buffer.ReadByte();
                            switch(Meta)
                            {
                                case 0x00: // Tempo
                                    b = buffer.ReadBytes(2);
                                    double tempo = ((b[1] << 8) + b[0]) / 218.4555555555555555555555555;
                                    curTrack.AddEvent(new EvTempo((uint)tempo));
                                    break;
                                case 0x01: // Tempo Slide
                                    b = buffer.ReadBytes(2);
                                    curTrack.AddEvent(new EvTempoSlide());
                                    break;
                                case 0x02: // Reverb Level
                                    b = buffer.ReadBytes(2);
                                    curTrack.AddEvent(new EvReverbLevel(b[0], b[1]));
                                    break;
                                case 0x03: // Reverb Fade
                                    b = buffer.ReadBytes(2);
                                    curTrack.AddEvent(new EvReverbFade(b[0], b[1]));
                                    break;
                                case 0x04: // Drum kit On
                                    channel = 9;
                                    curTrack.AddEvent(new EvDrumKitOn());
                                    break;
                                case 0x05: // Drum kit On
                                    curTrack.AddEvent(new EvDrumKitOff());
                                    break;
                                case 0x06: // End Track
                                    b = buffer.ReadBytes(2);
                                    curTrack.AddEvent(new EvEndTrack());
                                    cTrackId++;
                                    if (cTrackId < tracks.Length) {
                                        curTrack = tracks[cTrackId];
                                    }
                                    break;
                                case 0x07: // End Track
                                    b = buffer.ReadBytes(2);
                                    curTrack.AddEvent(new EvEndTrack());
                                    cTrackId++;
                                    if (cTrackId < tracks.Length)
                                    {
                                        curTrack = tracks[cTrackId];
                                    }
                                    break;
                                case 0x09: // Repeat Break
                                    b = buffer.ReadBytes(3);
                                    curTrack.AddEvent(new EvRepeatEnd());
                                    break;
                                case 0x0E: // call subroutine
                                    curTrack.AddEvent(new EvUnknown(STATUS_BYTE, Meta));
                                    break;
                                case 0x0F: // return from subroutine
                                    curTrack.AddEvent(new EvUnknown(STATUS_BYTE, Meta));
                                    break;
                                case 0x10: // Unknown
                                    curTrack.AddEvent(new EvUnknown(STATUS_BYTE, Meta));
                                    break;
                                case 0x14: // Program Change
                                    uint program = buffer.ReadByte();
                                    curTrack.AddEvent(new EvProgramChange(channel, program));
                                    break;
                                case 0x15: // Time Signature
                                    uint num = buffer.ReadByte();
                                    uint denom = buffer.ReadByte();
                                    curTrack.AddEvent(new EvTimeSign(num, denom));
                                    break;
                                case 0x16: // Maker
                                    b = buffer.ReadBytes(2);
                                    curTrack.AddEvent(new EvMaker(b[0], b[1]));
                                    break;
                                case 0x1C: // Unknown
                                    value = buffer.ReadByte();
                                    curTrack.AddEvent(new EvUnknown(STATUS_BYTE, (byte)value));
                                    break;
                                default:
                                    curTrack.AddEvent(new EvUnknown(STATUS_BYTE, Meta));
                                    break;
                            }
                            break;
                        case 0xFF: // End Track Padding
                            curTrack.AddEvent(new EvEndTrack());
                            break;
                        default:
                            Debug.Log("Unknonw instruction in "+name+" at "+buffer.BaseStream.Position+"  ->  "+ (byte)STATUS_BYTE);
                            //curTrack.AddEvent(new EvUnknown(STATUS_BYTE));
                            break;


                    }
                }
            }
        }

        private class AKAOTrack
        {
            private List<AKAOEvent> events;
            private List<uint> times;

            public AKAOTrack()
            {
                events = new List<AKAOEvent>();
                times = new List<uint>();
            }

            public List<AKAOEvent> Events
            {
                get => events;
            }

            public void AddEvent(AKAOEvent ev)
            {
                if(events == null)
                {
                    events = new List<AKAOEvent>();
                }
                events.Add(ev);
            }
            public void AddTime(uint t)
            {
                if (times == null)
                {
                    times = new List<uint>();
                }
                times.Add(t);
            }
        }

        private class AKAOEvent
        {
            internal byte deltaTime = 0x00;
            internal byte midiStatusByte;
            internal byte? midiArg1;
            internal byte? midiArg2;
            internal byte[] tail;

            internal List<byte> GetMidiBytes()
            {
                List<byte> midiBytes = new List<byte>();
                midiBytes.AddRange(ToVlqCollection(deltaTime));
                midiBytes.Add(midiStatusByte);
                if (midiArg1 != null) midiBytes.Add((byte)midiArg1);
                if (midiArg2 != null) midiBytes.Add((byte)midiArg2);
                if (tail != null && tail.Length > 0) midiBytes.AddRange(tail);

                if (midiStatusByte != 0)
                {
                    return midiBytes;
                }
                else
                {
                    return null;
                }
            }
            private IEnumerable<byte> ToVlqCollection(int integer)
            {
                List<byte> vlq = new List<byte>();
                if (integer >= 0x80)
                {
                    string binary = Convert.ToString(integer, 2);
                    for (int i = binary.Length; i > 0; i -= 7)
                    {
                        if (i >= 7)
                        {
                            if (i == binary.Length)
                                vlq.Add((byte)Convert.ToInt32(binary.Substring(i - 7, 7).PadLeft(8, '0'), 2));
                            else
                                vlq.Add((byte)Convert.ToInt32("1" + binary.Substring(i - 7, 7), 2));
                        }
                        else if (binary.Length < 7)
                            vlq.Add((byte)Convert.ToInt32(binary.Substring(0, i).PadLeft(8, '0'), 2));
                        else
                            vlq.Add((byte)Convert.ToInt32("1" + binary.Substring(0, i).PadLeft(7, '0'), 2));
                    }
                    vlq.Reverse();
                }
                else
                {
                    vlq.Add((byte)integer);
                }


                return vlq.ToArray();
            }
        }


        private class EvMaker : AKAOEvent
        {
            private byte v1;
            private byte v2;

            public EvMaker(byte v1, byte v2)
            {
                this.v1 = v1;
                this.v2 = v2;
            }
        }

        private class EvTimeSign : AKAOEvent
        {
            private uint num;
            private uint denom;
            private byte clocks = 0x24;
            private byte quart = 0x08;

            public EvTimeSign(uint num, uint denom)
            {
                this.num = num;
                this.denom = denom;
                /*
                deltaTime = 0x00;
                midiStatusByte = 0xFF;
                midiArg1 = 0x58;
                midiArg2 = 0x04;
                tail = new byte[] { (byte)num, (byte)denom, clocks, quart };
                */
            }
        }
        private class EvExpr : AKAOEvent
        {
            private uint expression;

            public EvExpr(uint channel, uint expression)
            {
                this.expression = expression;

                deltaTime = 0x00;
                midiStatusByte = (byte)(0xB0+channel);
                midiArg1 = 0x0B;
                midiArg2 = (byte)expression;
            }
        }
        private class EvVolume : AKAOEvent
        {
            private uint volume;

            public EvVolume(uint channel, uint volume)
            {
                this.volume = volume;

                deltaTime = 0x00;
                midiStatusByte = (byte)(0xB0 + channel);
                midiArg1 = 0x07;
                midiArg2 = (byte)volume;
            }
        }

        private class EvPan : AKAOEvent
        {
            private int pan;

            public EvPan(uint channel, int pan)
            {
                this.pan = pan;

                deltaTime = 0x00;
                midiStatusByte = (byte)(0xB0 + channel);
                midiArg1 = 0x0A;
                midiArg2 = (byte)pan;
            }
        }
        private class EvTempo : AKAOEvent
        {
            private double tempo;

            public EvTempo(uint tempo)
            {
                this.tempo = tempo;
                /*
                deltaTime = 0x00;
                midiStatusByte = 0xFF;
                midiArg1 = (byte)0x51;
                midiArg2 = (byte)0x03;
                tail = new byte[] { (byte)((tempo & 0xFF0000000000) >> 40), (byte)((tempo & 0x00FF00000000) >> 32), (byte)((tempo & 0x0000FF000000) >> 24), (byte)((tempo & 0x000000FF0000) >> 16), (byte)((tempo & 0x00000000FF00) >> 8), (byte)(tempo & 0x0000000000FF) };
                */
            }
        }
        private class EvReverbOn : AKAOEvent
        {
            public EvReverbOn()
            {

            }
        }



/*
General MIDI Sound Set Groupings: (all channels except 10)
Prog #      Instrument Group        Prog #      Instrument Group
1-8         Piano                   65-72       Reed
9-16        Chromatic Percussion    73-80       Pipe
17-24       Organ                   81-88       Synth Lead
25-32       Guitar                  89-96       Synth Pad
33-40       Bass                    97-104      Synth Effects
41-48       Strings                 105-112     Ethnic
49-56       Ensemble                113-120     Percussive
57-64       Brass                   121-128     Sound Effects
*/

        private class EvProgramChange : AKAOEvent
        {
            private uint articulationId;

            public EvProgramChange(uint channel, uint articulationId)
            {
                this.articulationId = articulationId;

                deltaTime = 0x00;
                midiStatusByte = (byte)(0xC0 + channel);
                midiArg1 = (byte)articulationId;
            }
        }
        private class EvNoteOn : AKAOEvent
        {
            //"9nH + 2 Bytes"; // 1001	MIDI channel [0 - 15]	Key Number [0 - 127]	Velocity [0 - 127]
            private uint key;
/*
0        1      64      127
off ppp p pp mp mf f ff fff
*/
            private uint velocity;

            public EvNoteOn(uint channel, uint key, uint velocity, uint t = 0x00)
            {
                this.key = key;
                this.velocity = velocity;

                deltaTime = (byte)t;
                midiStatusByte = (byte)(0x90 + channel);
                midiArg1 = (byte)key;
                midiArg2 = (byte)velocity;
            }
        }
        private class EvNoteOff : AKAOEvent
        {
            //"8nH + 2 Bytes"; // 1000	MIDI channel [0 - 15]	Key Number [0 - 127]	Velocity [0 - 127]
            private uint key;

            public EvNoteOff()
            {
                this.midiStatusByte = 0x81;
            }

            public EvNoteOff(uint channel, uint key, uint t)
            {
                this.key = key;

                deltaTime = (byte)t;
                midiStatusByte = (byte)(0x80 + channel);
                midiArg1 = (byte)key;
                midiArg2 = 0x40; // Standard velocity
            }
        }
        private class EvRest : AKAOEvent
        {
            private uint duration;

            public EvRest(uint duration)
            {
                this.duration = duration;

                this.deltaTime = (byte)duration;
            }
        }
        private class EvTie : AKAOEvent
        {
            public EvTie(uint t = 0x00)
            {
                this.deltaTime = (byte)t;
            }
        }
        private class EvOctave : AKAOEvent
        {
            private uint octave;

            public EvOctave(uint octave)
            {
                this.octave = octave;
            }
        }

        private class EvOctaveDown : AKAOEvent
        {
            public EvOctaveDown()
            {

            }
        }

        private class EvOctaveUp : AKAOEvent
        {
            public EvOctaveUp()
            {

            }
        }
        private class EvRepeatStart : AKAOEvent
        {
            public EvRepeatStart()
            {

            }
        }
        private class EvRepeatEnd : AKAOEvent
        {
            private int loopId;

            public EvRepeatEnd()
            {

            }

            public EvRepeatEnd(int loopId)
            {
                this.loopId = loopId;
            }
        }
        private class EvEndTrack : AKAOEvent
        {
            public EvEndTrack()
            {

            }
        }
        private class EvUnknown : AKAOEvent
        {
            private byte v;

            public EvUnknown(byte value)
            {

            }

            public EvUnknown(byte value, byte v)
            {
                this.v = v;
            }
        }

        private class EvPortamento : AKAOEvent
        {
            public EvPortamento(uint channel)
            {
                deltaTime = 0x00;
                midiStatusByte = (byte)(0xB0 + channel);
                midiArg1 = 0x41;
            }
        }

        private class EvExprSlide : AKAOEvent
        {
            private uint duration;
            private uint expression;

            public EvExprSlide(uint duration, uint expression)
            {
                this.duration = duration;
                this.expression = expression;
            }
        }

        private class EvPanSlide : AKAOEvent
        {
            private uint duration;
            private int pan;

            public EvPanSlide(uint duration, int pan)
            {
                this.duration = duration;
                this.pan = pan;
            }
        }

        private class EvAttack : AKAOEvent
        {
            private int attack;

            public EvAttack(int attack)
            {
                this.attack = attack;
            }
        }

        private class EvDecay : AKAOEvent
        {
            private int decay;

            public EvDecay(int decay)
            {
                this.decay = decay;
            }
        }

        private class EvSustain : AKAOEvent
        {
            private int sustain;

            public EvSustain(int sustain)
            {
                this.sustain = sustain;
            }
        }

        private class EvSustainRelease : AKAOEvent
        {
            private uint duration;

            public EvSustainRelease(uint duration)
            {
                this.duration = duration;
            }
        }

        private class EvRelease : AKAOEvent
        {
            private uint duration;

            public EvRelease(uint duration)
            {
                this.duration = duration;
            }
        }

        private class EvResetADSR : AKAOEvent
        {
        }

        private class EvLFOPitchRange : AKAOEvent
        {
            private byte v1;
            private byte v2;
            private byte v3;

            public EvLFOPitchRange(byte v1, byte v2, byte v3)
            {
                this.v1 = v1;
                this.v2 = v2;
                this.v3 = v3;
            }
        }

        private class EvLFOPitchDepth : AKAOEvent
        {
            private int depth;

            public EvLFOPitchDepth(int depth)
            {
                this.depth = depth;
            }
        }

        private class EvLFOPitchOff : AKAOEvent
        {
        }

        private class EvLFOExprRange : AKAOEvent
        {
            private byte v1;
            private byte v2;
            private byte v3;

            public EvLFOExprRange(byte v1, byte v2, byte v3)
            {
                this.v1 = v1;
                this.v2 = v2;
                this.v3 = v3;
            }
        }

        private class EvLFOExprDepth : AKAOEvent
        {
            private int depth;

            public EvLFOExprDepth(int depth)
            {
                this.depth = depth;
            }
        }

        private class EvLFOExprOff : AKAOEvent
        {
        }

        private class EvLFOPanpotRange : AKAOEvent
        {
            private byte v1;
            private byte v2;
            private byte v3;

            public EvLFOPanpotRange(byte v1, byte v2, byte v3)
            {
                this.v1 = v1;
                this.v2 = v2;
                this.v3 = v3;
            }
        }

        private class EvLFOPanpotDepth : AKAOEvent
        {
            private int depth;

            public EvLFOPanpotDepth(int depth)
            {
                this.depth = depth;
            }
        }

        private class EvLFOPanpotOff : AKAOEvent
        {
        }

        private class EvTranspose : AKAOEvent
        {
            private int transpose;

            public EvTranspose(int transpose)
            {
                this.transpose = transpose;
            }
        }

        private class EvTransposeMove : AKAOEvent
        {
            private int transpose;

            public EvTransposeMove(int transpose)
            {
                this.transpose = transpose;
            }
        }

        private class EvReverbOff : AKAOEvent
        {
        }

        private class EvNoiseOn : AKAOEvent
        {
        }

        private class EvNoiseOff : AKAOEvent
        {
        }

        private class EvFMOn : AKAOEvent
        {
        }

        private class EvFMOff : AKAOEvent
        {
        }

        private class EvSlurOn : AKAOEvent
        {
        }

        private class EvSlurOff : AKAOEvent
        {
        }

        private class EvPitchBend : AKAOEvent
        {
            private uint low;
            private uint high;

            public EvPitchBend(uint channel, uint low, uint high)
            {
                this.low = low;
                this.high = high;
                /*
                deltaTime = 0x00;
                midiStatusByte = (byte)(0xE0 + channel);
                midiArg1 = (byte)low;
                midiArg2 = (byte)high;
                */
            }
        }

        private class EvPitchBendMove : AKAOEvent
        {
            private uint value;

            public EvPitchBendMove(uint value)
            {
                this.value = value;
            }
        }

        private class EvTieTime : AKAOEvent
        {
            private uint value;

            public EvTieTime(uint value)
            {
                this.value = value;
            }
        }

        private class EvTempoSlide : AKAOEvent
        {
        }

        private class EvReverbLevel : AKAOEvent
        {
            private byte v1;
            private byte v2;

            public EvReverbLevel(byte v1, byte v2)
            {
                this.v1 = v1;
                this.v2 = v2;
            }
        }

        private class EvReverbFade : AKAOEvent
        {
            private byte v1;
            private byte v2;

            public EvReverbFade(byte v1, byte v2)
            {
                this.v1 = v1;
                this.v2 = v2;
            }
        }

        private class EvDrumKitOn : AKAOEvent
        {
        }

        private class EvDrumKitOff : AKAOEvent
        {
        }
    }

}
