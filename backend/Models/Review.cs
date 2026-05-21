namespace backend.Models
{
    public class Review
    {
        public int Id { get; set; }
        public int? TripId { get; set; }
        public Trip? Trip { get; set; }
        public string TripElementType { get; set; } = string.Empty;
        public int TripElementId { get; set; }
        public string ReviewText { get; set; } = string.Empty;
        public int Rating { get; set; }
        public DateTime Date { get; set; } = DateTime.UtcNow;

        public bool getTripStatus()
        {
            return Trip?.TripStatus == TripStatus.Completed;
        }

        public bool getReviewData()
        {
            return Rating is >= 1 and <= 5 && !string.IsNullOrWhiteSpace(ReviewText);
        }
    }
}
