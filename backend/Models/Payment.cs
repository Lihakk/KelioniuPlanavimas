namespace backend.Models
{
    public class Payment
    {
        public int Id { get; set; }
        public int? TripId { get; set; }
        public Trip? Trip { get; set; }
        public int? ReservationId { get; set; }
        public Reservation? Reservation { get; set; }
        public DateTime Date { get; set; } = DateTime.UtcNow;
        public decimal Amount { get; set; }
        public PaymentStatus PaymentStatus { get; set; } = PaymentStatus.Waiting;

        public void savePaymentData()
        {
            if (Date == default)
            {
                Date = DateTime.UtcNow;
            }
        }

        public void updatePaymentData(PaymentStatus status)
        {
            PaymentStatus = status;
        }
    }
}
