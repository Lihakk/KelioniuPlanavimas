namespace backend.Models
{
    public class Trip
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public TripStatus TripStatus { get; set; } = TripStatus.Planned;
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public int? RouteId { get; set; }
        public Route? Route { get; set; }
        public string SelectedAccommodation { get; set; } = string.Empty;
        public string SelectedFlight { get; set; } = string.Empty;
        public string SelectedCar { get; set; } = string.Empty;
        public decimal Price { get; set; } = 0m;

        public bool checkTrip()
        {
            return !string.IsNullOrWhiteSpace(Name)
                && StartDate != default
                && EndDate != default
                && EndDate >= StartDate;
        }

        public int determineTripConditions()
        {
            return Math.Max(1, (EndDate.Date - StartDate.Date).Days + 1);
        }
    }
}
