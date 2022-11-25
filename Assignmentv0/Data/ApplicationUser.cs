using Microsoft.AspNetCore.Identity;
namespace Assignmentv1.Data
{
    public class ApplicationUser : IdentityUser
    {
        public string Firstname { get; set; } = default!;
        public string Lastname { get; set; } = default!;
    }
}
