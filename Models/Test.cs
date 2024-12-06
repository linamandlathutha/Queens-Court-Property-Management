using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MvcCoreUploadAndDisplayImage_Demo.Models
{
    public class Test
    {
        [Key]
        public int Id { get; set; }
      

        [Required]
        public string Email { get; set; }

        [Required(ErrorMessage = "Password is required.")]
        [StringLength(16, MinimumLength = 16, ErrorMessage = "Password must be exactly 16 characters long.")]
        [RegularExpression(@"^(?=.*[a-z])(?=.*[A-Z])(?=.*\d)(?=.*[\W_]).{16}$",
         ErrorMessage = "Password must be exactly 16 characters long, and include at least one uppercase letter, one lowercase letter, one digit, and one special character.")]
        public string Password { get; set; }

       



    }
}
