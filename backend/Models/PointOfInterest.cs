namespace backend.Models
{
    public class PointOfInterest
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Type { get; set; } = string.Empty;
        public string Address { get; set; } = string.Empty;
        public bool HasTicket { get; set; }
        public string WorkingHours { get; set; } = string.Empty;
        public float Rating { get; set; }
        public string Longitude { get; set; } = string.Empty;
        public string Latitude { get; set; } = string.Empty;
    }
}