namespace VS.FileFormats.GEOM
{
    public class NVector4
    {
        public int? x;
        public int? y;
        public int? z;
        public int? f;
        public static NVector4 zero = new NVector4(0, 0, 0, 0);
        public static NVector4 one = new NVector4(1, 1, 1, 1);

        public NVector4()
        {
        }

        public NVector4(int x, int y, int z, int f)
        {
            this.x = x;
            this.y = y;
            this.z = z;
            this.f = f;
        }

        public new string ToString()
        {
            return "NVector f:" + f + ", x:" + x + ", y:" + y + ", z:" + z;
        }
    }
}
