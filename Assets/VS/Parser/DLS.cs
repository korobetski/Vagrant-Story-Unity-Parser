using System.Collections.Generic;
using System.IO;
using VS.Utils;

namespace VS.Parser
{
    public class DLS
    {
        private string _name = "";
        private AKAOInstrument[] _instruments;
        private AKAOSample[] _samples;


        public DLS()
        {
            _instruments = new AKAOInstrument[0];
            _samples = new AKAOSample[0];
        }

        public void OutputDLSFile()
        {
            uint size = GetDLSSize();
            List<byte> dlsByte = new List<byte>();
            dlsByte.AddRange(new byte[] { 0x52, 0x49, 0x46, 0x46 }); // "RIFF"
            dlsByte.AddRange(new byte[] { (byte)((size & 0xFF000000) >> 24), (byte)((size & 0x00FF0000) >> 16), (byte)((size & 0x0000FF00) >> 8), (byte)(size & 0x000000FF) }); // size
            dlsByte.AddRange(new byte[] { 0x44, 0x4C, 0x53, 0x20 }); // "DLS"



            ToolBox.DirExNorCreate("Assets/Resources/Sounds/");
            using (FileStream fs = File.Create("Assets/Resources/Sounds/" + _name + ".dls"))
            {
                for (int i = 0; i < dlsByte.Count; i++)
                {
                    fs.WriteByte(dlsByte[i]);
                }
                fs.Close();
            }
        }

        private uint GetDLSSize()
        {
            uint size = 0;
            size += 12; // "RIFF" + size + "DLS"
            size += 12; // COLH chunk (collection chunk - tells how many instruments)
            size += 12; // "lins" list (list of instruments - contains all the "ins " lists)
            for (uint i = 0; i < _instruments.Length; i++)
            {
                //size += _instruments[i].GetDLSSize(); //each "ins " list
            }
            size += 16; // "ptbl" + size + cbSize + cCues
            size += (uint)_samples.Length * sizeof(uint);    //each wave gets a poolcue
            size += 12;                            //"wvpl" list (wave pool - contains all the "wave" lists)
            for (uint i = 0; i < _samples.Length; i++)
            {
                //size += _samples[i].GetDLSWAVSize();                   //each "wave" list
            }
            size += 12;                            //"INFO" list
            size += 8;                                        //"INAM" + size
            size += (uint)_name.Length;                   //size of name string


            return size;
        }
        /*
                int DLSFile::WriteDLSToBuffer(vector<uint8_t> &buf)
                {
                    uint32_t theDWORD;

                    PushTypeOnVectBE<uint32_t>(buf, 0x52494646);            //"RIFF"
                    PushTypeOnVect<uint32_t>(buf, GetSize() - 8);           //size
                    PushTypeOnVectBE<uint32_t>(buf, 0x444C5320);            //"DLS "

                    PushTypeOnVectBE<uint32_t>(buf, 0x636F6C68);            //"colh "
                    PushTypeOnVect<uint32_t>(buf, 4);                       //size
                    PushTypeOnVect<uint32_t>(buf, (uint32_t)aInstrs.size());    //cInstruments - number of instruments
                    theDWORD = 4;                                           //account for 4 "lins" bytes
                    for (uint32_t i = 0; i < aInstrs.size(); i++)
                        theDWORD += aInstrs[i]->GetSize();                    //each "ins " list
                    WriteLIST(buf, 0x6C696E73, theDWORD);                   //Write the "lins" LIST
                    for (uint32_t i = 0; i < aInstrs.size(); i++)
                        aInstrs[i]->Write(buf);                               //Write each "ins " list

                    PushTypeOnVectBE<uint32_t>(buf, 0x7074626C);            //"ptbl"
                    theDWORD = 8;
                    theDWORD += (uint32_t)aWaves.size() * sizeof(uint32_t);    //each wave gets a poolcue
                    PushTypeOnVect<uint32_t>(buf, theDWORD);                    //size
                    PushTypeOnVect<uint32_t>(buf, 8);                           //cbSize
                    PushTypeOnVect<uint32_t>(buf, (uint32_t)aWaves.size());    //cCues
                    theDWORD = 0;
                    for (uint32_t i = 0; i < (uint32_t)aWaves.size(); i++)
                    {
                        PushTypeOnVect<uint32_t>(buf, theDWORD);                //write the poolcue for each sample
                                                                                //hFile->Write(&theDWORD, sizeof(uint32_t));			//write the poolcue for each sample
                        theDWORD += aWaves[i]->GetSize();                       //increment the offset to the next wave
                    }

                    theDWORD = 4;
                    for (uint32_t i = 0; i < aWaves.size(); i++)
                        theDWORD += aWaves[i]->GetSize();                   //each "wave" list
                    WriteLIST(buf, 0x7776706C, theDWORD);                 //Write the "wvpl" LIST
                    for (uint32_t i = 0; i < aWaves.size(); i++)
                        aWaves[i]->Write(buf);                              //Write each "wave" list

                    theDWORD = 12 + (uint32_t)name.size();               //"INFO" + "INAM" + size + the string size
                    WriteLIST(buf, 0x494E464F, theDWORD);                 //write the "INFO" list
                    PushTypeOnVectBE<uint32_t>(buf, 0x494E414D);          //"INAM"
                    PushTypeOnVect<uint32_t>(buf, (uint32_t)name.size());    //size
                    PushBackStringOnVector(buf, name);        //The Instrument Name string

                    return true;
                }
                */
    }
}