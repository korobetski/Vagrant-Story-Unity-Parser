using VagrantStory.Core;


namespace VagrantStory.Items
{
    public class Gem : Item
    {
        public Statistics statistics;


        public Gem(string na, string desc, string v3, string v4)
        {
            name = na;
            description = desc;

            statistics = new Statistics(v3, v4);
        }
    }
}
