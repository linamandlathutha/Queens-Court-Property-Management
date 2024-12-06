using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MvcCoreUploadAndDisplayImage_Demo.Data;
using MvcCoreUploadAndDisplayImage_Demo.Models;
using System.Security.Claims;


public class ReservationsController : Controller
{
    private readonly ApplicationDbContext _context;
    private readonly IEmailSender _emailSender;

    public ReservationsController(ApplicationDbContext context, IEmailSender emailSender)
    {
        _context = context;
        _emailSender = emailSender;
    }

    public async Task<IActionResult> Index()
    {
        try
        {
            // Get the currently logged-in user's email
            var userEmail = User.Identity.Name;

            if (string.IsNullOrEmpty(userEmail))
            {
            }

            // Check if the user is in the Manager or Admin roles
            var isManagerOrAdmin = User.IsInRole("Manager") || User.IsInRole("Admin");

            IEnumerable<Reservation> reservations;

            if (isManagerOrAdmin)
            {
                // If the user is a Manager or Admin, show all reservations
                reservations = await _context.Reservations
                    .Include(r => r.Apartment)
                    .Include(r => r.Profile)
                    .ToListAsync();
            }
            else
            {
                // Otherwise, show reservations for the logged-in user's profile
                var userProfile = await _context.Profiles
                    .FirstOrDefaultAsync(p => p.Email == userEmail);

                if (userProfile == null)
                {
                    return RedirectToAction("Create", "Profiles");
                }

                reservations = await _context.Reservations
                    .Include(r => r.Apartment)
                    .Include(r => r.Profile)
                    .Where(r => r.ProfileId == userProfile.ProfileId)
                    .ToListAsync();
            }

            return View(reservations);
        }
        catch (DbUpdateException dbEx)
        {
            // Log the exception (you could use a logging framework like Serilog, NLog, etc.)
            // LogError(dbEx);

            // Handle database-specific exceptions
            ModelState.AddModelError("", "There was an error accessing the database. Please try again later.");
            return RedirectToAction("Create", "Profiles");
        }
        catch (Exception ex)
        {
            // Log the general exception
            // LogError(ex);

            // Handle other general exceptions
            ModelState.AddModelError("", "An unexpected error occurred. Please try again.");
            return RedirectToAction("Create", "Profiles");
        }
    }




    public async Task<IActionResult> ResendEmail(int id)
    {
        var reservation = await _context.Reservations
            .Include(r => r.Profile)
            .Include(r => r.Apartment)
            .FirstOrDefaultAsync(r => r.ReservationId == id);

        if (reservation == null)
        {
            return NotFound();
        }

        var emailSubject = "Your Reservation Details";

        // Assuming ReservationDate and ViewingDate are DateTime
        var reservationDate = reservation.ReservationDate;
        var viewingDate = reservation.ViewingDate;

        // Calculate TimeSpan if needed (duration between reservation and viewing)
        

        // Format email body
        var emailBody = $@"
        <p>Dear {reservation.Profile.FirstName} {reservation.Profile.LastName},</p>
        <p>Here are your reservation details:</p>
        <p><strong>Apartment Name:</strong> {reservation.Apartment.ApartmentName}</p>
        <p><strong>Customer Name:</strong> {reservation.CustomerName}</p>
        <p><strong>Email:</strong> {reservation.CustomerEmail}</p>
        <p><strong>Phone Number:</strong> {reservation.PhoneNumber}</p>
        <p><strong>Reservation Date:</strong> {reservationDate.ToString("d MMM yyyy HH:mm")}</p>
            <p> Viewing Date: {reservation.ViewingDate}</p>
    
        <p>Regards,</p>
        <p>Apartment Management</p>";

        await _emailSender.SendEmailAsync(
            reservation.CustomerEmail,    // To email address
            emailSubject,                 // Subject
            emailBody);                   // Body

        return RedirectToAction(nameof(Index)); // Redirect back to the list or any other page
    }

    // Helper method to format TimeSpan
    private string FormatTimeSpan(TimeSpan timeSpan)
    {
        if (timeSpan.TotalDays >= 1)
            return $"{(int)timeSpan.TotalDays} day(s), {timeSpan.Hours} hour(s), {timeSpan.Minutes} minute(s)";
        if (timeSpan.TotalHours >= 1)
            return $"{timeSpan.Hours} hour(s), {timeSpan.Minutes} minute(s)";
        return $"{timeSpan.Minutes} minute(s)";
    }



    // GET: Reservations/Create
    public IActionResult Create(int? apartmentId)
    {
        if (apartmentId == null)
        {
            return NotFound();
        }

        var model = new ReservationViewModel
        {
            ApartmentId = apartmentId.Value,
            ReservationDate = DateTime.Now // Set default reservation date
        };

        return View(model);
    }

    // POST: Reservations/Create
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(ReservationViewModel model)
    {
        if (ModelState.IsValid)
        {
            // Get the logged-in user's ID
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            if (userId == null)
            {
                // Handle the case where the user is not logged in (optional)
                return Unauthorized(); // Or redirect to login page
            }





            var currentDateTime = DateTime.Now;
            var viewingTimeStart = new TimeSpan(9, 0, 0);   // 9:00 AM
            var viewingTimeEnd = new TimeSpan(16, 30, 0);   // 4:30 PM 

            // Ensure the viewing time is within the allowed range
            if (model.ViewingDate < viewingTimeStart || model.ViewingDate > viewingTimeEnd)
            {
                ModelState.AddModelError("ViewingDate", "Viewing time must be between 9:00 AM and 4:30 PM.");
                return View(model);
            }
            // Ensure the reservation date is not in the past
            if (model.ReservationDate.Date < currentDateTime.Date)
            {
                ModelState.AddModelError("ReservationDate", "Reservation date cannot be in the past.");
                return View(model);
            }

            // Ensure the viewing time is not in the past if the reservation is for today
            if (model.ReservationDate.Date == currentDateTime.Date && model.ViewingDate < currentDateTime.TimeOfDay)
            {
                ModelState.AddModelError("ViewingDate", "Viewing time cannot be in the past.");
                return View(model);
            }

            var userProfile = await _context.Profiles
                .FirstOrDefaultAsync(p => p.Email == User.Identity.Name);

            if (userProfile == null)
            {
                ModelState.AddModelError("", "Profile not found for the current user.");
                return View(model);
            }

            var reservation = new Reservation
            {
                ApartmentId = model.ApartmentId,
                ProfileId = userProfile.ProfileId,
                CustomerName = $"{userProfile.FirstName} {userProfile.LastName}",
                CustomerEmail = userProfile.Email,
                PhoneNumber = userProfile.PhoneNumber,
                ReservationDate = model.ReservationDate,
                ViewingDate = model.ViewingDate,
                UserId = userId
            };

            _context.Reservations.Add(reservation);
            await _context.SaveChangesAsync();

            // Prepare email content
            var emailSubject = "Your Reservation Details";
            var emailBody = $@"
        Dear {userProfile.FirstName} {userProfile.LastName},

        Thank you for your reservation. Below are your reservation details:

        <p> Apartment ID: {reservation.ApartmentId}</p>
        <p> Customer Name: {reservation.CustomerName}</p>
        <p> Email: {reservation.CustomerEmail}</p>
        <p> Phone Number: {reservation.PhoneNumber}</p>
        <p> Reservation Date: {reservation.ReservationDate}</p>
        <p> Viewing Time: {reservation.ViewingDate}</p>

        Regards,
        Apartment Management";

            // Send email
            await _emailSender.SendEmailAsync(
                userProfile.Email,        // To email address
                emailSubject,             // Subject
                emailBody);               // Body

            return RedirectToAction(nameof(Index)); // Redirect to list or another page
        }

        return View(model);
    }


}
