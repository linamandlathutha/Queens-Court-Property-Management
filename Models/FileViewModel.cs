using Microsoft.AspNetCore.Http;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MvcCoreUploadAndDisplayImage_Demo.Models
{
    public class LeaseAgreementViewModel
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public string FullName { get; set; }

        [Required]
        public string Address { get; set; }

        [Required]
        public string PhoneNumber { get; set; }

        [Required]
        [EmailAddress]
        public string Email { get; set; }

        [Required(ErrorMessage = "Date of birth is required.")]
        [Display(Name = "Date of Birth")]
        [DataType(DataType.Date)]
        [MinimumAge(14, ErrorMessage = "You must be at least 14 years old.")]
        public DateTime DateOfBirth { get; set; }

        [Required]
        public string Occupation { get; set; }

        [Required(ErrorMessage = "Identification number is required.")]
        [StringLength(13, MinimumLength = 13, ErrorMessage = "Identification number must be exactly 13 digits.")]
        [RegularExpression(@"^\d{13}$", ErrorMessage = "Identification number must be 13 digits long.")]
        public string IdentificationNumber { get; set; }

        [NotMapped]
        public IFormFile File { get; set; }

        public List<FileDetails> Files { get; set; } = new List<FileDetails>();
    }

    public class FileDetails
    {
        [Key]
        public int Id { get; set; }
        public string Name { get; set; }
        public string Path { get; set; }
    }

   
    
}
