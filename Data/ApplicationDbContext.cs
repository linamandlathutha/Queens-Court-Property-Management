using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using MvcCoreUploadAndDisplayImage_Demo.Models;

namespace MvcCoreUploadAndDisplayImage_Demo.Data
{
    public class ApplicationDbContext : IdentityDbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        public DbSet<Apartment> Apartments { get; set; }
        public DbSet<CustomerClass> Customers { get; set; }
        public DbSet<Manager> Managers { get; set; }
        public DbSet<Credit> Creditsdb { get; set; }
        public DbSet<Test> Tests { get; set; }
        public DbSet<Profile> Profiles { get; set; }
        public DbSet<History> Histories { get; set; }
        public DbSet<Reservation> Reservations { get; set; }
        public DbSet<ReportApartment> ReportApartments { get; set; }


        // Add a proper entity class for lease agreements here
        public DbSet<MvcCoreUploadAndDisplayImage_Demo.Models.LeaseAgreementViewModel> LeaseAgreementViewModel { get; set; } = default!;
    }
}
