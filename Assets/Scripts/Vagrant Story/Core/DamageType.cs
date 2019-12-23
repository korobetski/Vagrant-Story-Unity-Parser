namespace VagrantStory.Core
{

    public class DamageType
    {
        public string name;
        public short value;

        public DamageType(string v1, short v2)
        {
            name = v1;
            value = v2;
        }
        public static DamageType operator +(DamageType lhs, DamageType rhs)
        {
            lhs.value += rhs.value;
            return lhs;
        }
    }
}
