using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Http;

namespace MvcCoreUploadAndDisplayImage_Demo.Models
{
    public class Manager
    {
        [Key]
        public int Id { get; set; }

        [Required(ErrorMessage = "Please enter your name")]
        [Display(Name = "Full Name")]
        public string FullName { get; set; }

        [Required(ErrorMessage = "Please enter your email")]
        [EmailAddress]
        public string Email { get; set; }

        [Required(ErrorMessage = "Please enter your phone number")]
        [Phone]
        public string PhoneNumber { get; set; }

        [Required(ErrorMessage = "Please enter your address")]
        public string Address { get; set; }

        [Required(ErrorMessage = "Please upload your identity document")]
        public string IdentityDocument { get; set; }

        [Required(ErrorMessage = "Please upload the ownership document")]
        public string OwnershipDocument { get; set; }

        [Required(ErrorMessage = "Please upload a profile picture")]
        public string ProfilePicture { get; set; }

        // New Status property to track manager approval
        public ManagerStatus Status { get; set; } = ManagerStatus.Pending;  // Default to Pending when a manager registers
    }

    public class ManagerViewModel
    {

        [Key]
        public int Id { get; set; }

        [Required(ErrorMessage = "Please enter your name")]
        [Display(Name = "Full Name")]
        public string FullName { get; set; }

        [Required(ErrorMessage = "Please enter your email")]
        [EmailAddress]
        public string Email { get; set; }

        [Required(ErrorMessage = "Please enter your phone number")]
        [Phone]
        [StringLength(10, MinimumLength = 10, ErrorMessage = "Phone number must be exactly 10 digits.")]
        public string PhoneNumber { get; set; }

        [Required(ErrorMessage = "Please enter your address")]
        public string Address { get; set; }

        [Required(ErrorMessage = "Please upload your identity document")]
        public IFormFile IdentityDocument { get; set; }

        [Required(ErrorMessage = "Please upload the ownership document")]
        public IFormFile OwnershipDocument { get; set; }

        [Required(ErrorMessage = "Please upload a profile picture")]
        public IFormFile ProfilePicture { get; set; }

       
    }


    public enum ManagerStatus
    {
        Pending,  // Default value when a manager registers
        Approved,
        Declined
    }

}
