using MyBox;
using System;
using UnityEngine;

namespace VagrantStory.Core
{
    public class Attribute
    {
        public string name;
        public short value;

        public Attribute(string v1, short v2)
        {
            name = v1;
            value = v2;
        }

        public static Attribute operator +(Attribute lhs, Attribute rhs)
        {
            lhs.value += rhs.value;
            return lhs;
        }
    }
}
