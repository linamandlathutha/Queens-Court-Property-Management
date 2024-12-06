using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Threading.Tasks;
using MvcCoreUploadAndDisplayImage_Demo.Data;
using MvcCoreUploadAndDisplayImage_Demo.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Identity.UI.Services;

[Authorize]
public class ReportApartmentController : Controller
{
    private readonly ApplicationDbContext _context;
    private readonly UserManager<IdentityUser> _userManager;
    private readonly IConfiguration _configuration;
    private readonly IEmailSender _emailSender;

    public ReportApartmentController(ApplicationDbContext context, UserManager<IdentityUser> userManager, IConfiguration configuration, IEmailSender emailSender)
    {
        _context = context;
        _userManager = userManager;
        _configuration = configuration;
        _emailSender = emailSender;
    }

    // GET: ReportApartment
    public async Task<IActionResult> Index()
    {
        // Get the current logged-in user's email
        var userEmail = User.Identity.Name;

        // Check if the user is in the 'Admin' role
        if (User.IsInRole("Admin"))
        {
            // Admin can see all reports
            return View(await _context.ReportApartments.ToListAsync());
        }
        else
        {
            // Regular users can see only their own reports based on email
            var userReports = await _context.ReportApartments
                                            .Where(r => r.Email == userEmail)  // Assuming UserEmail is a property in the ReportApartment model
                                            .ToListAsync();

            return View(userReports);
        }
    }


    // GET: Reportgggggggg/Details/5
    public async Task<IActionResult> Details(int? id)
    {
        if (id == null)
        {
            return NotFound();
        }

        var reportApartment = await _context.ReportApartments
            .FirstOrDefaultAsync(m => m.Id == id);
        if (reportApartment == null)
        {
            return NotFound();
        }

        return View(reportApartment);
    }



    // GET: ReportApartment/Create
    public async Task<IActionResult> Create()
    {
        // Get the logged-in user's email
        var user = await _userManager.GetUserAsync(User);
        var userEmail = user?.Email;

        // Fetch the user's profile by email
        var profile = await _context.Profiles.FirstOrDefaultAsync(p => p.Email == userEmail);

        // If profile exists, set the email for the ReportApartment model
        var reportApartment = new ReportApartment
        {
            Email = profile?.Email, // Populate email field
            ReportDate = DateTime.Now // Set default to current date and time
        };

        // Populate the ProblemList for the dropdown
        var problemsList = Enum.GetValues(typeof(ApartmentProblem))
                               .Cast<ApartmentProblem>()
                               .Select(p => new SelectListItem
                               {
                                   Value = p.ToString(),
                                   Text = p.ToString()
                               }).ToList();

        ViewBag.ProblemList = problemsList;

        return View(reportApartment);
    }




    // POST: ReportApartment/Create
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(ReportApartment reportApartment)
    {
        if (ModelState.IsValid)
        {
            // Add the report to the context
            _context.Add(reportApartment);
            await _context.SaveChangesAsync();

            // Send an email after successfully creating the report
            var emailSubject = "New Apartment Report Submitted";
            var emailMessage = $"A new report has been submitted for Apartment: {reportApartment.ApartmentName}\n" +
                               $"Problem: {reportApartment.Problem}\n" +
                               $"Description: {reportApartment.ReportDescription}\n" +
                               $"Reported by: {reportApartment.Email}\n" +
                               $"Report Date: {reportApartment.ReportDate.ToShortDateString()}";

            await _emailSender.SendEmailAsync(reportApartment.Email, emailSubject, emailMessage);

            return RedirectToAction(nameof(Index));
        }

        // If the model state is not valid, repopulate the ViewBag and return the view
        var problemsList = Enum.GetValues(typeof(ApartmentProblem))
                               .Cast<ApartmentProblem>()
                               .Select(p => new SelectListItem
                               {
                                   Value = p.ToString(),
                                   Text = p.ToString()
                               }).ToList();
        ViewBag.ProblemList = problemsList;

        return View(reportApartment);
    }




    // GET: ReportApartment/GetApartmentDetails
    public async Task<IActionResult> GetApartmentDetails(int Id) // Change to 'id' to match your model
    {
        // Find the apartment by the selected Id
        var apartment = await _context.Apartments
            .Where(a => a.Id == Id) // Use 'Id' instead of 'ApartmentId'
            .Select(a => new { Address = a.Location, Email = a.Email }) // Only select the necessary fields
            .FirstOrDefaultAsync();

        if (apartment == null)
        {
            return NotFound();
        }

        // Return the data as a JSON object
        return Json(apartment);
    }
























    // GET: ReportApartment/Edit/5
    public async Task<IActionResult> Edit(int? id)
    {
        if (id == null)
        {
            return NotFound();
        }

        var reportApartment = await _context.ReportApartments.FindAsync(id);
        if (reportApartment == null)
        {
            return NotFound();
        }
        return View(reportApartment);
    }

    // POST: ReportApartment/Edit/5
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, [Bind("Id,ReportId,ApartmentName,Address,ReportDescription,ReportDate,Email,IsResolved")] ReportApartment reportApartment)
    {
        if (id != reportApartment.Id)
        {
            return NotFound();
        }

        if (ModelState.IsValid)
        {
            try
            {
                _context.Update(reportApartment);
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!ReportApartmentExists(reportApartment.Id))
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
        return View(reportApartment);
    }

    // GET: Reportgggggggg/Delete/5
    public async Task<IActionResult> Delete(int? id)
    {
        if (id == null)
        {
            return NotFound();
        }

        var reportApartment = await _context.ReportApartments
            .FirstOrDefaultAsync(m => m.Id == id);
        if (reportApartment == null)
        {
            return NotFound();
        }

        return View(reportApartment);
    }

    // POST: Reportgggggggg/Delete/5
    [HttpPost, ActionName("Delete")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirmed(int id)
    {
        var reportApartment = await _context.ReportApartments.FindAsync(id);
        if (reportApartment != null)
        {
            _context.ReportApartments.Remove(reportApartment);
        }

        await _context.SaveChangesAsync();
        return RedirectToAction(nameof(Index));
    }

    private bool ReportApartmentExists(int id)
    {
        return _context.ReportApartments.Any(e => e.Id == id);
    }
}
