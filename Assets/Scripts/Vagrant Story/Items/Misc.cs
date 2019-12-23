using System.Collections.Generic;

namespace VagrantStory.Items
{

    public class Misc : Item
    {
        public List<ItemEffect> effects;


        public Misc(string na, string desc, List<ItemEffect> eff)
        {
            name = na;
            description = desc;
            effects = eff;
        }
    }
}
