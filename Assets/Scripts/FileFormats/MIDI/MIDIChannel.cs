using System;
using System.Collections.Generic;
using System.Linq;

namespace Scripts.FileFormats.MIDI
{
    public class MIDIChannel
    {
        public uint id = 0;
        public bool drum = false;
        public byte? pan;
        public List<byte> programs;
        public List<MIDIEvent> events;


        public MIDIChannel(int t)
        {
            id = (uint)t;
            events = new List<MIDIEvent>();
            programs = new List<byte>();
        }


        internal void AddEvent(MIDIEvent ev)
        {
            if (events.Count > 0) ev.abstime = events[events.Count - 1].abstime + ev.deltatime;
            events.Add(ev);
        }


        internal void AddEvents(List<MIDIEvent> _events)
        {
            for (int i = 0; i < _events.Count; i++)
            {
                AddEvent(_events[i]);
            }
        }

        internal void InsertEvents(List<MIDIEvent> _events)
        {
            events.AddRange(_events);

            events = events.OrderBy(o => o.abstime).ToList();
            for (int i = 0; i < events.Count; i++) events[i].SwitchChannel((byte)id);
            RecomputeDeltas();
        }

        private void RecomputeDeltas()
        {
            for (int i = 0; i < events.Count; i++)
            {
                MIDIEvent lEv = events[i];
                if (i == 0)
                {
                    lEv.deltatime = lEv.abstime;
                }
                else
                {
                    MIDIEvent pEv = events[i - 1];
                    lEv.deltatime = lEv.abstime - pEv.abstime;
                }
            }
        }
    }
}
