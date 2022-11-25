using System.ComponentModel.DataAnnotations;
namespace Assignmentv1.ViewModels
{
    public class LoginUserViewModel
    {
        [Required(ErrorMessage = "User Email is required")]
        public string Email { get; set; }

        [Required(ErrorMessage = "Password is required")]
        public string Password { get; set; }
    }
}
