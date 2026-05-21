namespace backend.Models
{
    public class SupplyList
    {
        public int Id { get; set; }
        public int TripId { get; set; }
        public Trip? Trip { get; set; }
        public DateTime DateCreated { get; set; } = DateTime.UtcNow;
        public List<Item> Items { get; set; } = new();

        public void saveGeneratedSupplyList(IEnumerable<Item> items)
        {
            Items = items.ToList();
        }
    }
}
