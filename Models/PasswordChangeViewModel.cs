using System.ComponentModel.DataAnnotations;


namespace MvcCoreUploadAndDisplayImage_Demo.Models
{

    public class PasswordChangeViewModel
    {
        [Required(ErrorMessage = "Password is required.")]
        [StringLength(16, MinimumLength = 16, ErrorMessage = "Password must be exactly 16 characters long.")]
        [RegularExpression(@"^(?=.*[a-z])(?=.*[A-Z])(?=.*\d)(?=.*[\W_]).{16}$",
            ErrorMessage = "Password must be exactly 16 characters long, and include at least one uppercase letter, one lowercase letter, one digit, and one special character.")]
        public string Password { get; set; }
    }

}