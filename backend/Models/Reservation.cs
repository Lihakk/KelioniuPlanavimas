namespace backend.Models
{
    public class Reservation
    {
        public int Id { get; set; }
        public int TripId { get; set; }
        public Trip? Trip { get; set; }
        public DateTime ReservationDate { get; set; } = DateTime.UtcNow;
        public ReservationStatus ReservationStatus { get; set; } = ReservationStatus.Created;
        public ReservationType ReservationType { get; set; }
        public string Provider { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public decimal Price { get; set; }

        public void updateReservationData(ReservationStatus status)
        {
            ReservationStatus = status;
        }
    }
}
