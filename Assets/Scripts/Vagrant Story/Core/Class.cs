using MyBox;
using System;
using UnityEngine;

namespace VagrantStory.Core
{
    public class Class
    {
        public string name;
        public short value;
        public Class opposite;

        public Class(string v1, short v2)
        {
            name = v1;
            value = v2;
        }


        public static Class operator +(Class lhs, Class rhs)
        {
            lhs.value += rhs.value;
            return lhs;
        }
    }
}
