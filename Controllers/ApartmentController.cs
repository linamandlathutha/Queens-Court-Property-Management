using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MvcCoreUploadAndDisplayImage_Demo.Data;
using MvcCoreUploadAndDisplayImage_Demo.Models;
using System;
using System.IO;
using System.Linq;
using System.Security.Claims;

using System.Threading.Tasks;


namespace MvcCoreUploadAndDisplayImage_Demo.Controllers
{
    public class ApartmentController : Controller
    {
        private readonly ApplicationDbContext _dbContext;
        private readonly IWebHostEnvironment _webHostEnvironment;
        private readonly UserManager<IdentityUser> _userManager;
        private readonly IConfiguration _configuration;


        public ApartmentController(ApplicationDbContext dbContext, IWebHostEnvironment webHostEnvironment, UserManager<IdentityUser> userManager)
        {
            _dbContext = dbContext;
            _webHostEnvironment = webHostEnvironment;
            _userManager = userManager;
        }



        public IActionResult Maintanance()
        {
            return View();
        }







        public async Task<IActionResult> Index()
        {
            // Get the logged-in user's email
            var currentUserEmail = User.Identity?.Name;

            // Check if the user is not authenticated or email is null
            if (string.IsNullOrEmpty(currentUserEmail))
            {
                return RedirectToAction("Login", "Account"); // Redirect to login if not authenticated
            }

            // Check if the user has the 'Admin' role
            bool isAdmin = User.IsInRole("Admin");

            IQueryable<Apartment> apartmentsQuery = _dbContext.Apartments;

            if (!isAdmin)
            {
                // If not an Admin, filter apartments by the logged-in user's email
                apartmentsQuery = apartmentsQuery.Where(a => a.Email == currentUserEmail);
            }

            // Fetch the filtered list of apartments
            var apartments = await apartmentsQuery.ToListAsync();

            // Pass the records to the view
            return View(apartments);
        }




        [HttpPost]
        public async Task<IActionResult> PayDeposit(int apartmentId)
        {
            // Find the apartment by its ID
            var apartment = await _dbContext.Apartments.FindAsync(apartmentId);

            if (apartment == null)
            {
                return NotFound();
            }

            // Update the apartment status to "Taken"
            apartment.Status = ApartmentStatus.Taken;

            // Save changes to the database
            _dbContext.Update(apartment);
            await _dbContext.SaveChangesAsync();

            // Optionally, display a success message
            TempData["SuccessMessage"] = "Deposit paid successfully and apartment status updated to 'Taken'.";

            // Redirect to a suitable view, such as the details page or index
            return RedirectToAction("Details", new { id = apartmentId });
        }


   

public async Task<IActionResult> CIndex(string searchString)
    {
        // Get the current user's ID from the claims
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

        // Fetch apartment IDs from the reservations made by the current user
        var reservedApartmentIds = await _dbContext.Reservations
                                                    .Where(r => r.UserId == userId) // Assuming UserId is stored in Reservation
                                                    .Select(r => r.ApartmentId)
                                                    .ToListAsync();

        // Fetch apartments with status 'Free' that match the reserved apartment IDs
        var apartmentsQuery = _dbContext.Apartments
                                         .Where(a => a.Status == ApartmentStatus.Free &&
                                                     reservedApartmentIds.Contains(a.Id));

        // If searchString is provided, further filter apartments by name
        if (!string.IsNullOrEmpty(searchString))
        {
            apartmentsQuery = apartmentsQuery
                              .Where(a => a.ApartmentName.Contains(searchString));
        }

        var apartments = await apartmentsQuery.ToListAsync();
        return View(apartments);
    }





    public IActionResult Home()
        {
            return View();
        }


        public IActionResult Tesr()
        {
            return View();
        }

        public async Task<IActionResult> ViewCIndex(string searchString, decimal? minRent, decimal? maxRent)
        {
            // Fetch apartments with status 'Free'
            var apartmentsQuery = _dbContext.Apartments
                                            .Where(a => a.Status == ApartmentStatus.Free);

            // Filter by apartment name if searchString is provided
            if (!string.IsNullOrEmpty(searchString))
            {
                apartmentsQuery = apartmentsQuery
                                  .Where(a => a.ApartmentName.Contains(searchString));
            }

            // Filter by price range if both minRent and maxRent are provided
            if (minRent.HasValue && maxRent.HasValue)
            {
                apartmentsQuery = apartmentsQuery
                                  .Where(a => a.MonthlyRent >= minRent && a.MonthlyRent <= maxRent);
            }

            var apartments = await apartmentsQuery.ToListAsync();

            // Pass search parameters back to the view
            ViewData["CurrentFilter"] = searchString;
            ViewData["MinRent"] = minRent;
            ViewData["MaxRent"] = maxRent;

            return View(apartments);
        }



        // GET: Apartment/Create
        public async Task<IActionResult> Create()
        {

            // Get the logged-in user's email
            var userEmail = User.Identity.Name;


            // Query the profile of the logged-in user based on email
            var profile = await _dbContext.Profiles.FirstOrDefaultAsync(p => p.Email == userEmail);

            // If no profile is found, handle it accordingly (e.g., return an error or redirect)
            if (profile == null)
            {
                // Handle case where profile is not found (optional)
                return NotFound("Profile not found.");
            }

            // If userName is null, handle this appropriately (optional)
            if (string.IsNullOrEmpty(userEmail))
            {
                return RedirectToAction("Login", "Account");
            }


           
            



                // Optionally pass the userName to the view if needed
                ViewBag.UserName = userEmail;

            

            return View();
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(ApartmentViewModel model)
        {
            if (ModelState.IsValid)
            {
                // Retrieve the currently logged-in user's email
                var userEmail = User.Identity?.Name;

                // If email is null, handle this appropriately (e.g., redirect to login)
                if (string.IsNullOrEmpty(userEmail))
                {
                    // Redirect to login or handle the case where the user is not authenticated
                    return RedirectToAction("Login", "Account");
                }

                try
                {
                    // Handle file upload
                    string uniqueFileName = UploadedFile(model);

                    // Create the apartment record
                    Apartment apartment = new Apartment
                    {
                        Email = userEmail, // Use the logged-in user's email
                        ApartmentName = model.ApartmentName,
                        Location = model.Location,
                        Type = model.Type,
                        Status = model.Status,
                        MonthlyRent = model.MonthlyRent,
                        Image = uniqueFileName,
                        Bedrooms = model.Bedrooms,
                        Bathrooms = model.Bathrooms,
                        Size = (int)model.Size, // Explicitly cast double to int
                        Description = model.Description,
                        ContactPhone = model.ContactPhone,
                        AvailableFrom = model.AvailableFrom,
                        IsFurnished = model.IsFurnished,
                        HasParking = model.HasParking,
                        PetsAllowed = model.PetsAllowed,
                        HasBalcony = model.HasBalcony
                    };

                    // Add and save the apartment record
                    _dbContext.Apartments.Add(apartment);
                    await _dbContext.SaveChangesAsync();

                    // Provide feedback to the user (optional)
                    TempData["SuccessMessage"] = "Apartment created successfully!";

                    // Redirect to the index view
                    return RedirectToAction(nameof(Index));
                }
                catch (Exception ex)
                {
                    // Log the exception and handle the error
                    // You might want to log the error to a logging service or file
                    ModelState.AddModelError("", "An error occurred while creating the apartment. Please try again.");
                }
            }
           
            return View(model);
        }











        // GET: Apartment/EditStatus/5
        public async Task<IActionResult> EditStatus(int id)
        {
            var apartment = await _dbContext.Apartments.FindAsync(id);
            if (apartment == null)
            {
                return NotFound();
            }

            // Create a view model with only the fields you want to edit
            var viewModel = new EditStatusViewModel
            {
                Id = apartment.Id,
                Status = apartment.Status
            };

            return View(viewModel);
        }

        // POST: Apartment/EditStatus/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditStatus(int id, EditStatusViewModel model)
        {
            if (id != model.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                var apartment = await _dbContext.Apartments.FindAsync(id);
                if (apartment == null)
                {
                    return NotFound();
                }

                // Update only the status
                apartment.Status = model.Status;

                _dbContext.Update(apartment);
                await _dbContext.SaveChangesAsync();

                return RedirectToAction(nameof(Index));
            }

            return View(model);
        }





        public IActionResult Edit(int id)
        {
            var apartment = _dbContext.Apartments.Find(id);
            if (apartment == null)
            {
                return NotFound();
            }
            return View(apartment);  // Ensure this returns View("Edit", apartment) or just View(apartment)
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, Apartment apartment)
        {
            if (id != apartment.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                _dbContext.Update(apartment);
                await _dbContext.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(apartment);
        }

        public async Task<IActionResult> Delete(int id)
        {
            var apartment = await _dbContext.Apartments.FindAsync(id);
            if (apartment == null)
            {
                return NotFound();
            }

            _dbContext.Apartments.Remove(apartment);
            await _dbContext.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        public async Task<IActionResult> Details(int id)
        {
            var apartment = await _dbContext.Apartments.FindAsync(id);
            if (apartment == null)
            {
                return NotFound();
            }
            return View(apartment);
        }


        private string UploadedFile(ApartmentViewModel model)
        {
            string uniqueFileName = null;

            if (model.Image != null)
            {
                string uploadsFolder = Path.Combine(_webHostEnvironment.WebRootPath, "images");

                if (!Directory.Exists(uploadsFolder))
                {
                    Directory.CreateDirectory(uploadsFolder);
                }

                uniqueFileName = Guid.NewGuid().ToString() + "_" + model.Image.FileName;
                string filePath = Path.Combine(uploadsFolder, uniqueFileName);

                using (var fileStream = new FileStream(filePath, FileMode.Create))
                {
                    model.Image.CopyTo(fileStream);
                }
            }
            return uniqueFileName;
        }
    }
}
