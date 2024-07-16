namespace WoWAHDataProject.Database.Entities
{
    public class Sales
    {
        public int OrderId { get; set; }
        public string ItemString { get; set; }
        public string ItemName { get; set; }
        public int StackSize { get; set; }
        public int Quantity { get; set; }
        public double Price { get; set; }
        public int OtherPlayerId { get; set; }
        public int PlayerId { get; set; }
        public string Time { get; set; }
        public string Source { get; set; }
    }
}