using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using VS.Utils;
using System.IO;

namespace VS.Parser
{
    public class DLS
    {
        private string _name = "";

        public void OutputDLSFile()
        {
            List<byte> midiByte = new List<byte>();
            midiByte.AddRange(new byte[] { 0x52, 0x49, 0x46, 0x46 }); // "RIFF"
            midiByte.AddRange(new byte[] { 0x00, 0x00, 0x00, 0x06 }); // size
            midiByte.AddRange(new byte[] { 0x44, 0x4C, 0x53, 0x20 }); // "DLS"



            ToolBox.DirExNorCreate("Assets/Resources/Sounds/");
            using (FileStream fs = File.Create("Assets/Resources/Sounds/" + _name + ".dls"))
            {
                for (int i = 0; i < midiByte.Count; i++)
                {
                    fs.WriteByte(midiByte[i]);
                }
                fs.Close();
            }
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