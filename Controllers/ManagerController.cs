using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MvcCoreUploadAndDisplayImage_Demo.Data;
using MvcCoreUploadAndDisplayImage_Demo.Models;
using System;
using System.IO;
using System.Linq;
using System.Net.Mail;
using System.Net;
using System.Threading.Tasks;

namespace MvcCoreUploadAndDisplayImage_Demo.Controllers
{
    public class ManagerController : Controller
    {
        private readonly ApplicationDbContext _dbContext;
        private readonly IWebHostEnvironment _webHostEnvironment;
        private readonly IConfiguration _configuration;

        public ManagerController(ApplicationDbContext dbContext, IWebHostEnvironment webHostEnvironment, IConfiguration configuration)
        {
            _dbContext = dbContext;
            _webHostEnvironment = webHostEnvironment;
            _configuration = configuration;
        }

        // Accept action to approve the manager
        [HttpPost]
        public async Task<IActionResult> Accept(int id)
        {
            var manager = await _dbContext.Managers.FindAsync(id);
            if (manager != null)
            {
                manager.Status = ManagerStatus.Approved; // Set status to Approved
                _dbContext.Managers.Update(manager);
                await _dbContext.SaveChangesAsync();

                // Send an approval email to the manager
                await SendEmailAsync(
                    manager.Email,
                    "Manager Approval",
                    $"Dear {manager.FullName},\n\nYour manager account has been approved.\n\nBest regards,\nThe Admin Team"
                );
            }

            return RedirectToAction("Index");
        }



        // Reject action to remove the manager
        [HttpPost]
        public async Task<IActionResult> Reject(int id)
        {
            var manager = await _dbContext.Managers.FindAsync(id);
            if (manager != null)
            {
                // Send a rejection email to the manager
                await SendEmailAsync(
                    manager.Email,
                    "Manager Rejection",
                    $"Dear {manager.FullName},\n\nWe regret to inform you that your manager account application has been rejected.\n\nBest regards,\nThe Admin Team"
                );

                _dbContext.Managers.Remove(manager); // Remove the manager if rejected
                await _dbContext.SaveChangesAsync();
            }

            return RedirectToAction("Index");
        }


        public async Task<IActionResult> Index()
        {
            var currentUserEmail = User.Identity.Name;

            if (string.IsNullOrEmpty(currentUserEmail))
            {
                return RedirectToAction("Login", "Account"); // Redirect to login if not authenticated
            }

            // Check if the user is in the Admin role
            bool isAdmin = User.IsInRole("Admin");

            IQueryable<Manager> managersQuery = _dbContext.Managers;

            if (!isAdmin)
            {
                // If not an Admin, filter by the current user's email
                managersQuery = managersQuery.Where(m => m.Email == currentUserEmail);
            }

            var managers = await managersQuery.ToListAsync();

            return View(managers);
        }







        public async Task<IActionResult> Register()
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

            // Automatically set the full name and phone number in the view model
            var model = new ManagerViewModel
            {
                Email = userEmail,
                FullName = $"{profile.FirstName} {profile.LastName}", // Combine first and last name
                PhoneNumber = profile.PhoneNumber // Set phone number from profile
            };

            // Pass Google API key to the view
            ViewBag.GoogleApiKey = _configuration["GoogleAPI:PlacesApiKey"];

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Register(ManagerViewModel model)
        {
            if (ModelState.IsValid)
            {
                // Check if the email already exists
                var existingManager = await _dbContext.Managers
                    .FirstOrDefaultAsync(m => m.Email == model.Email);

                if (existingManager != null)
                {
                    ModelState.AddModelError(string.Empty,
                        "The email address you entered is already associated with a registered manager account. If you believe this is an error, please contact support. Each account must have a unique email address.");
                    return View(model);
                }

                // Handle file uploads
                string identityDocFileName = UploadedFile(model.IdentityDocument, "identityDocs");
                string ownershipDocFileName = UploadedFile(model.OwnershipDocument, "ownershipDocs");
                string profilePicFileName = UploadedFile(model.ProfilePicture, "profilePics");

                // Create the manager record
                Manager manager = new Manager
                {
                    FullName = model.FullName,
                    Email = model.Email,
                    PhoneNumber = model.PhoneNumber,
                    Address = model.Address,
                    IdentityDocument = identityDocFileName,
                    OwnershipDocument = ownershipDocFileName,
                    ProfilePicture = profilePicFileName
                };

                // Add and save the manager record
                _dbContext.Managers.Add(manager);
                await _dbContext.SaveChangesAsync();

                // Send email notification directly in the controller
                var smtpSettings = _configuration.GetSection("EmailSettings");
                var smtpServer = smtpSettings["SmtpServer"];
                var smtpPort = int.Parse(smtpSettings["SmtpPort"]);
                var smtpUsername = smtpSettings["SmtpUsername"];
                var smtpPassword = smtpSettings["SmtpPassword"];
                var fromAddress = smtpSettings["FromAddress"];

                using (var mailMessage = new MailMessage())
                {
                    mailMessage.From = new MailAddress(fromAddress);
                    mailMessage.To.Add(manager.Email);
                    mailMessage.Subject = "Registration Pending Approval";
                    mailMessage.Body = $"Dear {model.FullName},<br/><br/>" +
                                       "Thank you for registering as a manager. Your registration is pending approval. " +
                                       "You will be notified about the status changes.<br/><br/>" +
                                       "Best regards,<br/>The Team";
                    mailMessage.IsBodyHtml = true;

                    using (var smtpClient = new SmtpClient(smtpServer, smtpPort))
                    {
                        smtpClient.Credentials = new NetworkCredential(smtpUsername, smtpPassword);
                        smtpClient.EnableSsl = true;

                        // Send the email
                        await smtpClient.SendMailAsync(mailMessage);
                    }
                }

                // Redirect to a confirmation view
                return RedirectToAction("Index");
            }

            return View(model);
        }



        public IActionResult RegistrationSuccess()
        {
            return View();
        }

        public async Task<IActionResult> Details(int id)
        {
            var manager = await _dbContext.Managers
                .FirstOrDefaultAsync(m => m.Id == id);

            if (manager == null)
            {
                return NotFound();
            }

            return View(manager);
        }

        public async Task<IActionResult> Edit(int id)
        {
            var manager = await _dbContext.Managers.FindAsync(id);
            if (manager == null)
            {
                return NotFound();
            }

            ManagerViewModel model = new ManagerViewModel
            {
                FullName = manager.FullName,
                Email = manager.Email,
                PhoneNumber = manager.PhoneNumber,
                Address = manager.Address,
                IdentityDocument = null, // existing files, so null for update
                OwnershipDocument = null,
                ProfilePicture = null
            };

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, ManagerViewModel model)
        {
            if (id != model.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                var manager = await _dbContext.Managers.FindAsync(id);
                if (manager == null)
                {
                    return NotFound();
                }

                manager.FullName = model.FullName;
                manager.Email = model.Email;
                manager.PhoneNumber = model.PhoneNumber;
                manager.Address = model.Address;

                if (model.IdentityDocument != null)
                {
                    manager.IdentityDocument = UploadedFile(model.IdentityDocument, "identityDocs");
                }

                if (model.OwnershipDocument != null)
                {
                    manager.OwnershipDocument = UploadedFile(model.OwnershipDocument, "ownershipDocs");
                }

                if (model.ProfilePicture != null)
                {
                    manager.ProfilePicture = UploadedFile(model.ProfilePicture, "profilePics");
                }

                try
                {
                    _dbContext.Update(manager);
                    await _dbContext.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!ManagerExists(manager.Id))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }

                return RedirectToAction(nameof(Index));
            }

            return View(model);
        }

        public async Task<IActionResult> Delete(int id)
        {
            var manager = await _dbContext.Managers
                .FirstOrDefaultAsync(m => m.Id == id);

            if (manager == null)
            {
                return NotFound();
            }

            return View(manager);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var manager = await _dbContext.Managers.FindAsync(id);
            if (manager != null)
            {
                _dbContext.Managers.Remove(manager);
                await _dbContext.SaveChangesAsync();
            }

            return RedirectToAction(nameof(Index));
        }

        private bool ManagerExists(int id)
        {
            return _dbContext.Managers.Any(e => e.Id == id);
        }

        private string UploadedFile(IFormFile file, string folderName)
        {
            string uniqueFileName = null;

            if (file != null)
            {
                string uploadsFolder = Path.Combine(_webHostEnvironment.WebRootPath, folderName);

                if (!Directory.Exists(uploadsFolder))
                {
                    Directory.CreateDirectory(uploadsFolder);
                }

                uniqueFileName = Guid.NewGuid().ToString() + "_" + file.FileName;
                string filePath = Path.Combine(uploadsFolder, uniqueFileName);

                using (var fileStream = new FileStream(filePath, FileMode.Create))
                {
                    file.CopyTo(fileStream);
                }
            }
            return uniqueFileName;
        }


        private async Task SendEmailAsync(string toEmail, string subject, string message)
        {
            var smtpSettings = _configuration.GetSection("EmailSettings");
            var fromEmail = smtpSettings["FromAddress"];
            var host = smtpSettings["SmtpServer"];
            var port = int.Parse(smtpSettings["SmtpPort"]);
            var userName = smtpSettings["SmtpUsername"];
            var password = smtpSettings["SmtpPassword"];

            using (var smtpClient = new SmtpClient())
            {
                smtpClient.Host = host;
                smtpClient.Port = port;
                smtpClient.EnableSsl = true; // Use SSL for Gmail
                smtpClient.Credentials = new NetworkCredential(userName, password);

                var mailMessage = new MailMessage
                {
                    From = new MailAddress(fromEmail),
                    Subject = subject,
                    Body = message,
                    IsBodyHtml = false
                };

                mailMessage.To.Add(toEmail);

                await smtpClient.SendMailAsync(mailMessage);
            }
        }


    }
}
