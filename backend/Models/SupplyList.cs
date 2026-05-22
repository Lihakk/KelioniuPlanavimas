using System.ComponentModel.DataAnnotations.Schema;
namespace backend.Models
{
    public class SupplyList
    {
        public int Id { get; set; }
        public int TripId { get; set; }
        public Trip? Trip { get; set; }
        public DateTime DateCreated { get; set; } = DateTime.UtcNow;
        public List<Item> Items { get; set; } = new();
        [NotMapped]
        public string? WeatherSummary { get; set; }

        public void saveGeneratedSupplyList(IEnumerable<Item> items)
        {
            Items = items.ToList();
        }
    }
}
