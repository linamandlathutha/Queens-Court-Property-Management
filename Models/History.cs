using System.ComponentModel.DataAnnotations;

namespace MvcCoreUploadAndDisplayImage_Demo.Models
{
    public class History
    {
        [Key]
        public int Id { get; set; }

        [Required(ErrorMessage = "Please enter Email")]
        [Display(Name = "Email")]
        public string Email { get; set; }

        


        [Required(ErrorMessage = "Please enter apartment name")]
        [Display(Name = "Apartment Name")]
        public string ApartmentName { get; set; }

        [Required(ErrorMessage = "Please enter location")]
        public string Location { get; set; }

        [Required(ErrorMessage = "Please choose apartment type")]
        [Display(Name = "Apartment Type")]
        public ApartmentType Type { get; set; }

        [Required(ErrorMessage = "Please choose apartment status")]
        [Display(Name = "Status")]
        public ApartmentStatus Status { get; set; }

        [Required(ErrorMessage = "Please enter monthly rent")]
        [Display(Name = "Monthly Rent")]
        public decimal MonthlyRent { get; set; }

        // Set default to the current date and time
        public DateTime PaymentDate { get; set; } = DateTime.Now;

        // Use PaymentStatus enum instead of a string
        public PaymentStatus PaymentStatus { get; set; } = PaymentStatus.Paid;

        public DateTime? DueDate { get; set; }

        public bool IsRecurring { get; set; }


    }
    public enum PaymentStatus
    {
        
        Paid,
        Overdue
    }

}
