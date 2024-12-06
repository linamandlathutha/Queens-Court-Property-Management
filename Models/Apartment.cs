using System.ComponentModel.DataAnnotations;

namespace MvcCoreUploadAndDisplayImage_Demo.Models
{
    public enum ApartmentType
    {
        OneRoom,
        TwoRoom,
        ThreeRoom
    }

    public enum ApartmentStatus
    {
        Free,
        Taken
    }

    public class Apartment
    {
        [Key]
        public int Id { get; set; }

        [Required(ErrorMessage = "Please enter Email")]
        [Display(Name = "Email")]
        [EmailAddress]
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
        public ApartmentStatus Status { get; set; }=ApartmentStatus.Free;

        [Required(ErrorMessage = "Please enter monthly rent")]
        [Display(Name = "Monthly Rent")]
        public decimal MonthlyRent { get; set; }

        [Required(ErrorMessage = "Please choose apartment image")]
        [Display(Name = "Apartment Image")]
        public string Image { get; set; }

        // New fields added below
        [Required(ErrorMessage = "Please enter the number of bedrooms")]
        [Display(Name = "Number of Bedrooms")]
        public int Bedrooms { get; set; }

        [Required(ErrorMessage = "Please enter the number of bathrooms")]
        [Display(Name = "Number of Bathrooms")]
        public int Bathrooms { get; set; }

        [Required(ErrorMessage = "Please enter the size of the apartment")]
        [Display(Name = "Size (sq ft)")]
        public int Size { get; set; } // Size in square feet

        [Required(ErrorMessage = "Please enter a short description of the apartment")]
        [Display(Name = "Description")]
        [StringLength(500, ErrorMessage = "Description cannot exceed 500 characters.")]
        public string Description { get; set; }

        [Required(ErrorMessage = "Please provide a contact phone number")]
        [Display(Name = "Contact Phone")]
        [Phone(ErrorMessage = "Please enter a valid phone number")]
        public string ContactPhone { get; set; }

        [Required(ErrorMessage = "Please select the availability date")]
        [Display(Name = "Available From")]
        [DataType(DataType.Date)]
        [FutureDate(ErrorMessage = "The available date cannot be in the past.")]
        public DateTime AvailableFrom { get; set; }

        [Display(Name = "Is Furnished?")]
        public bool IsFurnished { get; set; }

        [Display(Name = "Parking Available?")]
        public bool HasParking { get; set; }

        [Display(Name = "Pets Allowed?")]
        public bool PetsAllowed { get; set; }

        [Display(Name = "Has Balcony?")]
        public bool HasBalcony { get; set; }



    }
    public class FutureDateAttribute : ValidationAttribute
    {
        public override bool IsValid(object value)
        {
            if (value == null) return true; // Allow null, as [Required] handles it.

            DateTime dateValue;
            bool isDate = DateTime.TryParse(value.ToString(), out dateValue);

            // Ensure that the date is today or in the future
            return isDate && dateValue >= DateTime.Now.Date;
        }

        public override string FormatErrorMessage(string name)
        {
            return $"{name} cannot be in the past.";
        }


      

    }
    public class EditStatusViewModel
    {
        public int Id { get; set; }
        public ApartmentStatus Status { get; set; }
    }
}
