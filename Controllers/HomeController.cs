using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using MvcCoreUploadAndDisplayImage_Demo.Data;
using MvcCoreUploadAndDisplayImage_Demo.Models;
using Stripe;
using Stripe.Checkout;
using System.Globalization;
using System.Net;
using System.Net.Mail;
using System.Threading.Tasks;
using testsubject.Models;
using System.IO;
using System.Configuration;
using System.Data.SqlClient;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;

namespace MvcCoreUploadAndDisplayImage_Demo.Controllers
{
    public class HomeController : Controller
    {
        private readonly StripeSettings _stripeSettings;
        private readonly ApplicationDbContext _context;
        private readonly IConfiguration _configuration;

        public HomeController(IOptions<StripeSettings> stripeSettings, ApplicationDbContext context, IConfiguration configuration)
        {
            _stripeSettings = stripeSettings.Value;
            _context = context;
            _configuration = configuration;
        }

        public IActionResult Index()
        {
            return View();
        }

        public IActionResult CreateCheckoutSession(string amount, string productName, string description)
        {
            var currency = "zar";
            var successUrl = Url.Action("Success", "Home", new { amount, productName, description }, Request.Scheme);
            var cancelUrl = Url.Action("Cancel", "Home", null, Request.Scheme);
            StripeConfiguration.ApiKey = _stripeSettings.SecretKey;

           // Normalize the amount for parsing
    var normalizedAmount = amount.Replace(',', '.'); 

    // Attempt to parse the normalized amount to a decimal
    if (!decimal.TryParse(normalizedAmount, NumberStyles.Number, CultureInfo.InvariantCulture, out var decimalAmount))
    {
        return BadRequest("Invalid amount format.");
    }

            var unitAmount = Convert.ToInt32(decimalAmount * 100);

            var options = new SessionCreateOptions
            {
                PaymentMethodTypes = new List<string> { "card" },
                LineItems = new List<SessionLineItemOptions>
                {
                    new SessionLineItemOptions
                    {
                        PriceData = new SessionLineItemPriceDataOptions
                        {
                            Currency = currency,
                            UnitAmount = unitAmount,
                            ProductData = new SessionLineItemPriceDataProductDataOptions
                            {
                                Name = productName,
                                Description = description
                            }
                        },
                        Quantity = 1
                    }
                },
                Mode = "payment",
                SuccessUrl = successUrl,
                CancelUrl = cancelUrl
            };

            var service = new SessionService();
            var session = service.Create(options);

            return Redirect(session.Url);
        }

        public async Task<IActionResult> Success(string amount, string productName, string description)
        {
            Console.WriteLine($"Success action called with amount: {amount}, productName: {productName}, description: {description}");

            if (string.IsNullOrEmpty(amount) || !decimal.TryParse(amount, NumberStyles.Number, CultureInfo.InvariantCulture, out var parsedAmount))
            {
                var errorViewModel = new ErrorViewModel
                {
                    Message = "Payment information is incomplete or invalid.",
                    RequestId = HttpContext.TraceIdentifier
                };

                Console.WriteLine("Payment information is incomplete or invalid.");
                return View("Error", errorViewModel);
            }

            var userEmail = User.Identity?.Name;
            if (string.IsNullOrEmpty(userEmail))
            {
                return RedirectToAction("Login", "Account");
            }

            // Retrieve the existing history record (if any) for this user and apartment
            var existingHistory = _context.Histories
                .FirstOrDefault(h => h.Email == userEmail && h.ApartmentName == productName);

            if (existingHistory != null)
            {
                // Update PaymentStatus to Paid and extend DueDate by 30 days
                existingHistory.PaymentStatus = PaymentStatus.Paid;
                existingHistory.PaymentDate = DateTime.Now;
                // Extend DueDate by 30 days if it has a value, otherwise set it to 30 days from now
                existingHistory.DueDate = existingHistory.DueDate.HasValue
                    ? existingHistory.DueDate.Value.AddDays(30)
                    : DateTime.Now.AddDays(30);


                // Log for debugging
                Console.WriteLine($"Existing record found. DueDate extended to: {existingHistory.DueDate}");
            }
            else
            {
                // Create a new History record if none exists
                var history = new History
                {
                    Email = userEmail,
                    ApartmentName = productName ?? "Unknown Apartment",
                    Location = description ?? "Unknown Location",
                    Type = ApartmentType.OneRoom,
                    Status = ApartmentStatus.Taken,
                    MonthlyRent = parsedAmount / 100,
                    PaymentStatus = PaymentStatus.Paid,
                    PaymentDate = DateTime.Now,
                    DueDate = DateTime.Now.AddDays(30), // Set initial due date 30 days from payment
                    IsRecurring = true
                };

                _context.Histories.Add(history);
                Console.WriteLine("New history record created with initial DueDate of 30 days from now.");
            }

            // Update the apartment status if applicable
            var apartmentId = ExtractApartmentIdFromDescription(description);
            var apartment = await _context.Apartments.FindAsync(apartmentId);
            if (apartment != null)
            {
                apartment.Status = ApartmentStatus.Taken;
                _context.Apartments.Update(apartment);
            }
            else
            {
                Console.WriteLine("Apartment not found.");
                return View("Error", new ErrorViewModel { Message = "Apartment not found." });
            }

            await _context.SaveChangesAsync();
            Console.WriteLine("Payment recorded successfully and due date updated.");

            return View("Success");
        }







        // Helper method to extract the apartment ID from the description
        private int ExtractApartmentIdFromDescription(string description)
        {
            var parts = description.Split('#');
            if (parts.Length > 1 && int.TryParse(parts[1], out var apartmentId))
            {
                return apartmentId; // Return the extracted apartment ID
            }

            throw new ArgumentException("Invalid description format");
        }











        public IActionResult Cancel()
        {
            return View("Cancel");
        }

        public IActionResult Error()
        {
            var errorViewModel = new ErrorViewModel
            {
                Message = "An error occurred.",
                RequestId = HttpContext.TraceIdentifier
            };
            return View(errorViewModel);
        }

        public IActionResult History()
        {
            // Retrieve the logged-in user's email
            var userEmail = User.Identity?.Name;

            // Check if the current user is an admin
            bool isAdmin = User.IsInRole("Admin");

            // Retrieve the history records
            List<History> historyRecords;

            if (isAdmin)
            {
                // Admin can see all records except those with status 'Free'
                historyRecords = _context.Histories
                                         .Where(h => h.Status != ApartmentStatus.Free)  // Filter out 'Free' status
                                         .ToList();
            }
            else
            {
                // Non-admin users only see their own records, excluding 'Free' status
                historyRecords = _context.Histories
                                         .Where(h => h.Email == userEmail && h.Status != ApartmentStatus.Free)  // Filter by user email and exclude 'Free' status
                                         .ToList();
            }

            return View(historyRecords);
        }





        [HttpPost]
        public async Task<IActionResult> MarkAsFree(string apartmentName, string leaveReason)
        {
            // Find the apartment by its name
            var apartment = await _context.Apartments
                .FirstOrDefaultAsync(a => a.ApartmentName == apartmentName);

            if (apartment != null)
            {
                // Mark the apartment as free
                apartment.Status = ApartmentStatus.Free;

                // Find and remove the corresponding history record
                var historyRecord = await _context.Histories
                    .FirstOrDefaultAsync(h => h.ApartmentName == apartmentName);

                if (historyRecord != null)
                {
                    _context.Histories.Remove(historyRecord);
                }

                // Save changes to the database
                await _context.SaveChangesAsync();

                // Send an email notifying the tenant about the vacating process
                var userEmail = User.Identity?.Name;
                if (!string.IsNullOrEmpty(userEmail))
                {
                    await SendVacatingEmail(userEmail, apartmentName, leaveReason);
                }
            }

            return RedirectToAction("Tesr", "Apartment");
        }

        private async Task SendVacatingEmail(string userEmail, string apartmentName, string leaveReason)
        {
            var smtpClient = new SmtpClient
            {
                Host = _configuration["EmailSettings:SmtpServer"],
                Port = int.Parse(_configuration["EmailSettings:SmtpPort"]),
                Credentials = new NetworkCredential(
                    _configuration["EmailSettings:SmtpUsername"],
                    _configuration["EmailSettings:SmtpPassword"]),
                EnableSsl = true,
            };

            var mailMessage = new MailMessage
            {
                From = new MailAddress(_configuration["EmailSettings:FromAddress"]),
                Subject = "Apartment Vacating Confirmation",
                Body = $"Dear tenant,\n\nYou have successfully vacated the apartment '{apartmentName}'.\nReason: {leaveReason}.\n\nThank you.",
                IsBodyHtml = false,
            };
            mailMessage.To.Add(userEmail);

            await smtpClient.SendMailAsync(mailMessage);
        }




           [HttpPost]
    public async Task<IActionResult> SendEvictionNotice(int historyId)
    {
        // Fetch the record from the database
        var history = await _context.Histories.FindAsync(historyId);

        if (history == null || history.PaymentStatus != PaymentStatus.Overdue)
        {
            return NotFound("Tenant record not found or not overdue.");
        }

        // Load SMTP settings from appsettings.json
        var smtpServer = _configuration["EmailSettings:SmtpServer"];
        var smtpPort = int.Parse(_configuration["EmailSettings:SmtpPort"]);
        var smtpUsername = _configuration["EmailSettings:SmtpUsername"];
        var smtpPassword = _configuration["EmailSettings:SmtpPassword"];
        var fromAddress = _configuration["EmailSettings:FromAddress"];

        var subject = "Notice of Eviction Due to Overdue Rent Payment";
        var messageBody = $@"
            Dear {history.Email},

            This is an official notice regarding your overdue rent payment for apartment '{history.ApartmentName}'. Since the rent has remained unpaid beyond the due date, we are issuing this notice of potential eviction. Please settle the payment immediately to avoid further action.

            Thank you,
            Queens Property Management";

        try
        {
            // Set up the SMTP client
            using (var client = new SmtpClient(smtpServer, smtpPort))
            {
                client.Credentials = new NetworkCredential(smtpUsername, smtpPassword);
                client.EnableSsl = true;

                var mailMessage = new MailMessage
                {
                    From = new MailAddress(fromAddress),
                    Subject = subject,
                    Body = messageBody,
                    IsBodyHtml = false
                };
                mailMessage.To.Add(history.Email);

                await client.SendMailAsync(mailMessage);
            }

            // Log success message and redirect back to admin view
            Console.WriteLine($"Eviction notice sent to {history.Email}.");
            return RedirectToAction("ManageOverdueTenants");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Failed to send eviction notice to {history.Email}. Error: {ex.Message}");
            return View("Error", new ErrorViewModel { Message = "Failed to send eviction notice." });
        }
    }

    // Optional: Add an action to list overdue tenants for admin
    public IActionResult ManageOverdueTenants()
    {
        var overdueTenants = _context.Histories
            .Where(h => h.PaymentStatus == PaymentStatus.Overdue)
            .ToList();

        return View(overdueTenants);
    }













        private async Task SendPaymentConfirmationEmail(string userEmail, string productName, string description, decimal amount)
        {
            try
            {
                string host = "smtp.gmail.com";
                int port = 587;
                bool enableSsl = true;

                using (MailMessage mm = new MailMessage("queensproperty41@gmail.com", userEmail))
                {
                    decimal amo;
                    amo = amount;

                    mm.Subject = "Payment Confirmation";
                    mm.Body = $"Dear Customer,\n\nThank you for your payment.\n\n" +
                              $"Details:\n" +
                              $"Property Name: {productName}\n" +
                              $"Description: {description}\n" +
                             $"Amount Paid: {amo.ToString("C", new System.Globalization.CultureInfo("en-ZA"))}\n\n" +

                              $"Best regards,\nQueens Court Properties";
                    mm.IsBodyHtml = false;

                    using (SmtpClient smtp = new SmtpClient(host, port))
                    {
                        smtp.EnableSsl = enableSsl;
                        smtp.Credentials = new NetworkCredential("queensproperty41@gmail.com", "sxoypddqavnsqjwm");
                        await smtp.SendMailAsync(mm);
                        Console.WriteLine("Payment confirmation email sent successfully.");
                    }
                }
            }
            catch (SmtpException ex)
            {
                Console.WriteLine($"SMTP Error: {ex.Message}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error: {ex.Message}");
            }
        }

    }
}