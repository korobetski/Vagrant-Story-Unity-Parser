using System;
using System.Collections.Generic;

namespace VS.Utils
{
    public static class Util
    {
        public static T[] Slice<T>(T[] inArray, int start, int end)
        {
            T[] outArray = new T[end - start];
            Array.Copy(inArray, start, outArray, 0, end - start);

            return outArray;
        }

        public static T[] Slice<T>(T[] inArray, uint start, uint end)
        {
            T[] outArray = new T[end - start];
            Array.Copy(inArray, start, outArray, 0, end - start);

            return outArray;
        }
    }

    /// <summary>
    /// Byte array value comparer. Compare the arrays component-wise
    /// </summary>
    public class ByteArrayValueComparer : IEqualityComparer<char[]>
    {
        public bool Equals(char[] x, char[] y)
        {
            if (x.Length != y.Length)
            {
                return false;
            }
            for (int i = 0; i < x.Length; i++)
            {
                if (x[i] != y[i])
                {
                    return false;
                }
            }
            return true;
        }

        public int GetHashCode(char[] obj)
        {
            int result = 17;
            for (int i = 0; i < obj.Length; i++)
            {
                unchecked
                {
                    result = result * 23 + obj[i];
                }
            }
            return result;
        }
    }
}

