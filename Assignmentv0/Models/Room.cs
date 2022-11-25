namespace Assignmentv1.Models
{
    public class Room
    {
        public int Id { get; set; }
        public string? RoomTitleId { get; set; } = string.Empty;
        public string? Name { get; set; } = string.Empty;
        public int Capacity { get; set; }
        public ICollection<Fete>? Fetes { get; set; } = new List<Fete>();

      


    }
}