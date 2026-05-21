namespace backend.Models
{
    public enum TripStatus
    {
        Planned,
        Confirmed,
        Completed,
        Cancelled
    }

    public enum PaymentStatus
    {
        Waiting,
        Paid,
        Refunded,
        Cancelled
    }

    public enum ReservationStatus
    {
        Created,
        WaitingForPayment,
        Paid,
        Cancelled
    }

    public enum ReservationType
    {
        Flight,
        Accommodation,
        Car
    }

    public enum AccountStatus
    {
        Active,
        Blocked
    }
}
