using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace VS.FileFormats.AKAO
{
    public class AKAO:ScriptableObject
    {
        public Enums.AKAO.Type type;


        public static bool CheckHeader(byte[] bytes)
        {
            //41 4B 41 4F
            return (bytes[0] == 0x41 && bytes[1] == 0x4B && bytes[2] == 0x41 && bytes[3] == 0x4F) ? true : false;
        }
    }
}
