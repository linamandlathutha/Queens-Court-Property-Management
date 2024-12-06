namespace MvcCoreUploadAndDisplayImage_Demo.Models
{
    public class Reservation
    {
        public int ReservationId { get; set; }
        public int ApartmentId { get; set; }
        public int ProfileId { get; set; }
        public string UserId { get; set; } // Ensure this field is present
        public string CustomerName { get; set; }
        public string CustomerEmail { get; set; }
        public DateTime ReservationDate { get; set; }
        public TimeSpan ViewingDate { get; set; }

        // Navigation property to Apartment
        public virtual Apartment Apartment { get; set; }
        public string PhoneNumber { get; internal set; }

        public virtual Profile Profile { get; set; }

       
    }

}
