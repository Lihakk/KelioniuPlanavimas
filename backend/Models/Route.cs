namespace backend.Models
{
    public class Route
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public float Length { get; set; }
        public string StartingCity { get; set; } = string.Empty;
        public string EndCity { get; set; } = string.Empty;
        public string Polyline { get; set; } = string.Empty;
        public string TravelTime { get; set; } = string.Empty;
        public List<RoutePoint> RoutePoints { get; set; } = new();

        public bool checkRoute()
        {
            return !string.IsNullOrWhiteSpace(Name)
                && !string.IsNullOrWhiteSpace(StartingCity)
                && !string.IsNullOrWhiteSpace(EndCity);
        }

        public string getRoute()
        {
            return $"{StartingCity} -> {EndCity}";
        }
    }
}
