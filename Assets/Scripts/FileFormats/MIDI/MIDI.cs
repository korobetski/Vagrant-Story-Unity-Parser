using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using VS.Utils;

namespace Scripts.FileFormats.MIDI
{
    public class MIDI
    {
        //--Constants
        public const int MicroSecondsPerMinute = 60000000; //microseconds in a minute
        public const int MinChannel = 0;
        public const int MaxChannel = 15;
        public const int DrumChannel = 9;


        public MIDIChannel[] channels;


        public void MergeChannels(List<MIDIChannel> channels)
        {

            // we reorganise channels to have 16 channels maximum
            // so we merge channel with the same program
            MIDIChannel[] MIDIchannels = new MIDIChannel[16];
            for (int t = 0; t < 16; t++) MIDIchannels[t] = new MIDIChannel(t);
            MIDIchannels[0] = channels[0]; // the first track must stay in the first slot (it contains important events like tempo, time signature etc...)
            MIDIchannels[9].drum = true; // channel 9 is reserved for drums

            for (int t = 1; t < channels.Count; t++)
            {
                if (channels[t].drum)
                {
                    MIDIchannels[9].InsertEvents(channels[t].events);
                }
                else if (channels[t].programs.Count == 1)
                {
                    bool pcheck = false;
                    byte prg = channels[t].programs[0];
                    foreach (MIDIChannel c in MIDIchannels)
                    {
                        if (c.programs.Count > 0)
                        {
                            foreach (byte p in c.programs)
                            {
                                if (p == prg && channels[t].pan == c.pan)
                                {
                                    c.InsertEvents(channels[t].events);
                                    pcheck = true;
                                    break;
                                }
                            }
                        }
                    }

                    if (!pcheck)
                    {
                        foreach (MIDIChannel c in MIDIchannels)
                        {
                            if (c.programs.Count == 0 && c.drum == false)
                            {
                                c.programs.Add(prg);
                                c.pan = channels[t].pan;
                                c.InsertEvents(channels[t].events);
                                pcheck = true;
                                break;
                            }
                        }
                    }

                    if (!pcheck)
                    {
                        Debug.Log("Not enough midi channels...");
                    }
                }
            }


            this.channels = channels.ToArray();
        }

        public void SaveAs(string name)
        {
            byte quarterNote = 0x30;
            List<byte> midiByte = new List<byte>();
            midiByte.AddRange(new byte[] { 0x4D, 0x54, 0x68, 0x64 }); // MThd Header
            midiByte.AddRange(new byte[] { 0x00, 0x00, 0x00, 0x06 }); // Chunck length
            midiByte.AddRange(new byte[] { 0x00, 0x01 }); // Format Midi 1
            midiByte.Add((byte)(((byte)(16) & 0xFF00) >> 8)); //num tracks hi
            midiByte.Add((byte)((byte)(16) & 0x00FF)); //num tracks lo
            midiByte.Add((byte)((quarterNote & 0xFF00) >> 8)); //Per Quarter Note hi
            midiByte.Add((byte)(quarterNote & 0x00FF)); //Per Quarter Note lo
            foreach (MIDIChannel channel in channels)
            {
                //track.OrderByAbsTime();
                midiByte.AddRange(new byte[] { 0x4D, 0x54, 0x72, 0x6B }); // MTrk Header
                List<byte> tb = new List<byte>();
                foreach (MIDIEvent ev in channel.events)
                {
                    List<byte> evb = ev.GetMidiBytes();
                    if (evb != null) tb.AddRange(evb);
                }
                midiByte.AddRange(new byte[] { (byte)((tb.Count + 4 & 0xFF000000) >> 24), (byte)((tb.Count + 4 & 0x00FF0000) >> 16), (byte)((tb.Count + 4 & 0x0000FF00) >> 8), (byte)(tb.Count + 4 & 0x000000FF) }); // Chunck length
                midiByte.AddRange(tb); // Track datas
                midiByte.AddRange(new byte[] { 0x00, 0xFF, 0x2F, 0x00 }); // End Track

            }

            ToolBox.DirExNorCreate("Assets/MIDI/");
            using (FileStream fs = File.Create("Assets/MIDI/" + name + ".mid"))
            {
                for (int i = 0; i < midiByte.Count; i++)
                {
                    fs.WriteByte(midiByte[i]);
                }
                fs.Close();
            }
        }
    }
}
