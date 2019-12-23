namespace VagrantStory.Items
{

    public class Key : Item
    {
        public string lockId = ""; // the Key lockId must match with the Door lockId to unlock a Door.

        public Key(string id, string name = "Key")
        {
            stackable = false;
            quantity = 1;

            lockId = id;
            this.name = name;
        }
    }
}
