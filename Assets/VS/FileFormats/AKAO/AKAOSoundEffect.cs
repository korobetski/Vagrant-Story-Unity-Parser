using System;
using System.IO;
using UnityEditor;
using UnityEngine;

namespace VS.FileFormats.AKAO
{
    public class AKAOSoundEffect:ScriptableObject
    {
        public ushort length;
        public ushort unk;
        public ushort pad1;
        public byte sampleCollectionID;
        public byte unk1;
        public ushort pad2;
        public byte[] unk2;
        public ushort firstTrackLength;
        public AKAOTrack[] tracks; // always two tracks ?

        public void ParseFromBuffer(BinaryReader buffer, long limit)
        {
            length = (ushort)limit;
            byte[] header = buffer.ReadBytes(4);       // AKAO
            if (!AKAO.CheckHeader(header))
            {
                return;
            }

            unk = buffer.ReadUInt16();
            pad1 = buffer.ReadUInt16();
            sampleCollectionID = buffer.ReadByte(); // 0 or 200
            unk1 = buffer.ReadByte();
            pad2 = buffer.ReadUInt16();
            unk2 = buffer.ReadBytes(22);
            firstTrackLength = buffer.ReadUInt16();

            tracks = new AKAOTrack[2];
            tracks[0] = new AKAOTrack();
            tracks[0].SetDatas(buffer.ReadBytes((int)(firstTrackLength)), (uint)buffer.BaseStream.Position);
            tracks[1] = new AKAOTrack();
            tracks[1].SetDatas(buffer.ReadBytes((int)(limit - 36 - firstTrackLength)), (uint)buffer.BaseStream.Position);
        }

        public void ConvertToMidi(bool sf2Trigger)
        {
            // we transmute into an AKAOSequence to get methods
            AKAOSequence AKAOSequence = ScriptableObject.CreateInstance<AKAOSequence>();
            AKAOSequence.name = name + ".AKAO";
            AKAOSequence.Filename = AKAOSequence.name;
            AKAOSequence.length = length;
            AKAOSequence.sampleCollectionId = sampleCollectionID;
            AKAOSequence.bitwiseNumTracks = 0x03;
            AKAOSequence.tracks = tracks;
            AKAOSequence.ptrTracks = new uint[2];
            AKAOSequence.ptrTracks[0] = tracks[0].operations[0].adr;
            AKAOSequence.ptrTracks[1] = tracks[1].operations[0].adr;
            AKAOSequence.ConvertToMidi(sf2Trigger);
        }
    }

    [CustomEditor(typeof(AKAOSoundEffect))]
    public class AKAOSoundEffectEditor : Editor
    {
        bool sf2Trigger = true;
        public override void OnInspectorGUI()
        {
            var akaoSeq = target as AKAOSoundEffect;
            DrawDefaultInspector();
            sf2Trigger = GUILayout.Toggle(sf2Trigger, new GUIContent("Produce a SF2 (soundfont) file ?"));
            if (GUILayout.Button("Export as MIDI File"))
            {
                akaoSeq.ConvertToMidi(sf2Trigger);
            }
        }
    }
}
