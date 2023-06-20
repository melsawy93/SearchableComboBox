namespace TestApp
{
    public class Item
    {
        public string Name { get; set; }

        public Item(string _name)
        {
            Name = _name;
        }

        public override string ToString()
        {
            return Name;
        }
    }
}
