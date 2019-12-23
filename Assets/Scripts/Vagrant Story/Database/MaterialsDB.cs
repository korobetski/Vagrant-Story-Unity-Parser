using VagrantStory.Core;

namespace VagrantStory.Database
{

    public class MaterialsDB
    {
        public enum eMaterials { None, Wood, Leather, Bronze, Iron, Hagane, Silver, Damascus };

        public static readonly SmithMaterial Wood = new SmithMaterial("Wood", 1, 0, 0, 0, 0, 0, 0, 4, -6, 8, 8, -6, -4, -4, 5, 8, 0);
        public static readonly SmithMaterial Leather = new SmithMaterial("Leather", 2, 0, 0, 0, 0, 0, 0, 2, 5, 5, -1, -1, -5, -5, 1, 6, 0);
        public static readonly SmithMaterial Bronze = new SmithMaterial("Bronze", 3, -1, -1, 2, -1, -1, -5, 8, -5, -5, 3, 3, -2, -2, 3, 2, -2);
        public static readonly SmithMaterial Iron = new SmithMaterial("Iron", 4, 1, 1, -2, 1, 1, 0, 10, 0, -4, -4, 0, -1, -1, 5, 2, -2);
        public static readonly SmithMaterial Hagane = new SmithMaterial("Hagane", 5, 5, 5, 1, 0, 5, 5, 14, 3, 3, -5, -5, -5, -5, 6, 3, -1);
        public static readonly SmithMaterial Silver = new SmithMaterial("Silver", 6, 0, 0, 20, 15, 0, 5, 5, -5, -5, -5, -5, 20, -20, 4, 2, -1);
        public static readonly SmithMaterial Damascus = new SmithMaterial("Damascus", 7, 10, 10, -2, 0, 10, 10, 20, -5, -5, -5, -5, 20, -20, 9, 4, -1);

        public static SmithMaterial[] List = new SmithMaterial[8] {
            null,
            Wood, Leather, Bronze, Iron, Hagane, Silver, Damascus
        };
    }
}
