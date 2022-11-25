namespace Assignmentv1.Models
{
    public class Booking
    {
        public int Id { get; set; }
        public int StudentId { get; set; }
        public Student? Student { get; set; }
        public DateTime FeteTime { get; set; }
        public int FeteId { get; set; }
        public Fete? Fete { get; set; }

        public ICollection<Room>? Rooms { get; set; } = new List<Room>();
        public string? ModifiedByUserId { get; set; }
    }
}
