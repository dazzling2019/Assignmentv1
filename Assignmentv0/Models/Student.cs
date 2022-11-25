using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;

namespace Assignmentv1.Models
{
    public class Student
    {
        
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;

        public virtual ICollection<Booking>? Bookings { get; set; } = new List<Booking>();

    }
}
