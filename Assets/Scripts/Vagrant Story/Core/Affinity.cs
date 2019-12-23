namespace VagrantStory.Core
{
    public class Affinity
    {
        public string name;
        public short value;
        public Affinity opposite;


        public Affinity(string v1, short v)
        {
            name = v1;
            value = v;
        }
        public Affinity(string v1, short v, Affinity opp)
        {
            name = v1;
            value = v;
            opposite = opp;
        }

        public static Affinity operator +(Affinity lhs, Affinity rhs)
        {
            lhs.value += rhs.value;
            return lhs;
        }
    }
}
