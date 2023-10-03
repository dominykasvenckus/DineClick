namespace DineClickAPI.Models
{
    public class Reservation
    {
        public int ReservationId { get; set; }
        public DateOnly Date { get; set; }
        public TimeOnly Time { get; set; }
        public int PartySize { get; set; }
        public ReservationStatus Status { get; set; }
        public required Restaurant Restaurant { get; set; }
    }

    public enum ReservationStatus
    {
        Pending,
        Confirmed,
        Canceled
    }
}
