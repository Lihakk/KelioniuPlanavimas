using System.ComponentModel.DataAnnotations.Schema;
namespace backend.Models
{
    public class Item
    {
        public int Id { get; set; }
        public int? SupplyListId { get; set; }
        public SupplyList? SupplyList { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Type { get; set; } = string.Empty;
        public int Quantity { get; set; } = 1;
        public bool IsPacked { get; set; }
        [NotMapped]
        public string? Reason { get; set; }

        public bool selectItemsByConditions(string condition)
        {
            return string.IsNullOrWhiteSpace(condition)
                || Type.Contains(condition, StringComparison.OrdinalIgnoreCase)
                || Name.Contains(condition, StringComparison.OrdinalIgnoreCase);
        }
    }
}
