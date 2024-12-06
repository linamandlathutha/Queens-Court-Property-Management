using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MvcCoreUploadAndDisplayImage_Demo.Data;
using MvcCoreUploadAndDisplayImage_Demo.Models;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace MvcCoreUploadAndDisplayImage_Demo.Controllers
{
    public class HomeFileController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly IConfiguration _configuration;

        public HomeFileController(ApplicationDbContext context, IConfiguration configuration)
        {
            _context = context;
            _configuration = configuration;
        }


        public IActionResult ViewFiles()
        {
            var userEmail = User.Identity.IsAuthenticated ? User.Identity.Name : string.Empty;

            var model = new LeaseAgreementViewModel
            {
                Files = new List<FileDetails>(),
                Email = userEmail
            };

            // Get files from the server
            var uploadPath = Path.Combine(Directory.GetCurrentDirectory(), "upload");
            if (Directory.Exists(uploadPath))
            {
                foreach (var item in Directory.GetFiles(uploadPath))
                {
                    model.Files.Add(new FileDetails { Name = Path.GetFileName(item), Path = item });
                }
            }

            return View(model);
        }




        [HttpPost]
        public IActionResult DeleteFile(string filename)
        {
            try
            {
                var uploadPath = Path.Combine(Directory.GetCurrentDirectory(), "upload");
                var filePath = Path.Combine(uploadPath, filename);

                if (System.IO.File.Exists(filePath))
                {
                    System.IO.File.Delete(filePath);
                    TempData["Message"] = "File deleted successfully.";
                }
                else
                {
                    TempData["ErrorMessage"] = "File not found.";
                }
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"Error deleting file: {ex.Message}";
            }

            return RedirectToAction("ViewFiles");
        }






        // GET: HomeFile/UploadLeaseAgreement
        public async Task<IActionResult> UploadLeaseAgreement()
        {
            // Get the currently logged-in user's email
            var userEmail = User.Identity.IsAuthenticated ? User.Identity.Name : string.Empty;

            // Retrieve the profile information based on the logged-in user's email
            var userProfile = await _context.Profiles.FirstOrDefaultAsync(p => p.Email == userEmail);

            if (userProfile == null)
            {
                // Handle the case where the profile is not found (optional)
                return RedirectToAction("Index", "Home");
            }

            var model = new LeaseAgreementViewModel
            {
                FullName = $"{userProfile.FirstName} {userProfile.LastName}", // Combine first and last names
                PhoneNumber = userProfile.PhoneNumber, // Get phone number from profile
                Email = userProfile.Email, // Get email from profile (same as logged-in user's email)
                Files = new List<FileDetails>()
            };

            // Get files from the server
            var uploadPath = Path.Combine(Directory.GetCurrentDirectory(), "upload");
            if (Directory.Exists(uploadPath))
            {
                foreach (var item in Directory.GetFiles(uploadPath))
                {
                    model.Files.Add(new FileDetails { Name = Path.GetFileName(item), Path = item });
                }
            }
            // Pass Google API key to the view
            ViewBag.GoogleApiKey = _configuration["GoogleAPI:PlacesApiKey"];
            return View(model);
        }


        // POST: HomeFile/UploadLeaseAgreement
        // POST: HomeFile/UploadLeaseAgreement
        [HttpPost]
        public async Task<IActionResult> UploadLeaseAgreement(LeaseAgreementViewModel model)
        {
            // Get the currently logged-in user's email
            var userEmail = User.Identity.IsAuthenticated ? User.Identity.Name : string.Empty;

            if (ModelState.IsValid && model.File != null)
            {
                var fileName = Path.GetFileName(model.File.FileName);
                var filePath = Path.Combine(Directory.GetCurrentDirectory(), "upload", fileName);

                // Save the file to the server
                if (System.IO.File.Exists(filePath))
                {
                    System.IO.File.Delete(filePath);
                }

                using (var localFile = System.IO.File.OpenWrite(filePath))
                using (var uploadedFile = model.File.OpenReadStream())
                {
                    await uploadedFile.CopyToAsync(localFile);
                }

                // Save the lease agreement details to the database
                var leaseAgreement = new LeaseAgreementViewModel
                {
                    FullName = model.FullName, // Assuming FullName is still being provided by the user
                    Address = model.Address,
                    PhoneNumber = model.PhoneNumber,
                    Email = userEmail, // Set email from the currently logged-in user
                    DateOfBirth = model.DateOfBirth,
                    Occupation = model.Occupation,
                    IdentificationNumber = model.IdentificationNumber,
                    Files = new List<FileDetails>
            {
                new FileDetails { Name = fileName, Path = filePath }
            }
                };

                // Save to the database context (assuming you have injected the db context as _context)
                _context.LeaseAgreementViewModel.Add(leaseAgreement);
                await _context.SaveChangesAsync();

                ViewBag.Message = "Lease agreement successfully uploaded";
            }
            else
            {
                ViewBag.Message = "Please correct the errors and try again.";
            }

            // Refresh the list of files from the server
            model.Files = new List<FileDetails>();
            var uploadPath = Path.Combine(Directory.GetCurrentDirectory(), "upload");
            if (Directory.Exists(uploadPath))
            {
                foreach (var item in Directory.GetFiles(uploadPath))
                {
                    model.Files.Add(new FileDetails { Name = Path.GetFileName(item), Path = item });
                }
            }

            return RedirectToAction("CIndex","Apartment");
        }

        // GET: HomeFile/Index
        public async Task<IActionResult> Index()
        {
            // Fetch all lease agreements from the database
            var leaseAgreements = await _context.LeaseAgreementViewModel.ToListAsync();

            // Return the view with the list of lease agreements
            return View(leaseAgreements);
        }

        // GET: HomeFile/Download?filename=example.pdf
        public async Task<IActionResult> Download(string filename)
        {
            if (string.IsNullOrEmpty(filename))
                return Content("Filename is not available");

            var path = Path.Combine(Directory.GetCurrentDirectory(), "upload", filename);

            if (!System.IO.File.Exists(path))
                return NotFound();

            var memory = new MemoryStream();
            using (var stream = new FileStream(path, FileMode.Open))
            {
                await stream.CopyToAsync(memory);
            }
            memory.Position = 0;

            return File(memory, GetContentType(path), Path.GetFileName(path));
        }

        private string GetContentType(string path)
        {
            var ext = Path.GetExtension(path).ToLowerInvariant();
            switch (ext)
            {
                case ".pdf":
                    return "application/pdf";
                // Add more MIME types as needed
                default:
                    return "application/octet-stream";
            }
        }

        // GET: HomeFile/Index
       
    }
}
