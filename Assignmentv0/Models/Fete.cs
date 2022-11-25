namespace Assignmentv1.Models
{
    public class Fete
    {
        public int Id { get; set; }
        public string? Name { get; set; }
        public string? Synopsis { get; set; }
        public string? EventURL { get; set; }
        public DateTime FeteTime { get; set; }
        public ICollection<Room>? Rooms { get; set; } = new List<Room>();
        public ICollection<Booking>? Bookings { get; set; } = new List<Booking>();
    }

    }

