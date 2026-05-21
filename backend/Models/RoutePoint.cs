namespace backend.Models
{
    public class RoutePoint
    {
        public int Id { get; set; }
        public int RouteId { get; set; }
        public Route? Route { get; set; }
        public string Name { get; set; } = string.Empty;
        public string City { get; set; } = string.Empty;
        public string Latitude { get; set; } = string.Empty;
        public string Longitude { get; set; } = string.Empty;
        public int Order { get; set; }
        public int? PointOfInterestId { get; set; }
        public PointOfInterest? PointOfInterest { get; set; }
    }
}
