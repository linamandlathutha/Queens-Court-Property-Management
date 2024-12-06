using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MvcCoreUploadAndDisplayImage_Demo.Models
{
    public class ReportApartment
    {
        [Key]
        [Required]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

       

       

        [Required]
        [Display(Name = "Apartment Name")]
        public string ApartmentName { get; set; }

    

        [Required]
        public ApartmentProblem Problem { get; set; }

        [Required]
        [Display(Name = "Description")]
        public string ReportDescription { get; set; }

        [Required(ErrorMessage = "Please select date")]
        [Display(Name = "Report Date")]
        [DataType(DataType.Date)]
        [FutureDate(ErrorMessage = "The date cannot be in the past.")]
        public DateTime ReportDate { get; set; }

        [Required]
        [DataType(DataType.EmailAddress)]
        public string Email { get; set; }

        [Required]
        public bool IsResolved { get; set; }

        

        public ReportApartment()
        {
            ReportDate = DateTime.Now;
            IsResolved = false;
        }
    }

    public enum ApartmentProblem
    {
        PlumbingIssues,
        ElectricalFaults,
        PestInfestation,
        StructuralDamage,
        BrokenAppliances,
        NoiseComplaints,
        HeatingCoolingProblems,
        WaterLeakage,
        SecurityConcerns
    }

}
