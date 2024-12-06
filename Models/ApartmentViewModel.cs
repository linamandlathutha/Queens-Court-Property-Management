using System;
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Http;

namespace MvcCoreUploadAndDisplayImage_Demo.Models
{
    public class ApartmentViewModel


    {
        [Key]
        public int Id { get; set; }

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

        [Required(ErrorMessage = "Please choose apartment image")]
        [Display(Name = "Apartment Image")]
        public IFormFile Image { get; set; }

        [Required(ErrorMessage = "Please enter the number of bedrooms")]
        [Display(Name = "Number of Bedrooms")]
        public int Bedrooms { get; set; }

        [Required(ErrorMessage = "Please enter the number of bathrooms")]
        [Display(Name = "Number of Bathrooms")]
        public int Bathrooms { get; set; }

        [Required(ErrorMessage = "Please enter the size of the apartment")]
        [Display(Name = "Size (sq ft)")]
        public double Size { get; set; } // Size in square feet or meters

        [Display(Name = "Description")]
        public string Description { get; set; } // Detailed description of the apartment

        [Required(ErrorMessage = "Please enter contact phone number")]
        [Display(Name = "Contact Phone")]
        [Phone(ErrorMessage = "Please enter a valid phone number")]
        public string ContactPhone { get; set; }

        [Required(ErrorMessage = "Please select the availability date")]
        [Display(Name = "Available From")]
        [DataType(DataType.Date)]
        [FutureDate(ErrorMessage = "The available date cannot be in the past.")]
        public DateTime AvailableFrom { get; set; }

        [Display(Name = "Is Furnished")]
        public bool IsFurnished { get; set; }

        [Display(Name = "Has Parking")]
        public bool HasParking { get; set; }

        [Display(Name = "Pets Allowed")]
        public bool PetsAllowed { get; set; }

        [Display(Name = "Has Balcony")]
        public bool HasBalcony { get; set; }
    }
}
