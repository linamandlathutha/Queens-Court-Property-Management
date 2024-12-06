using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using MvcCoreUploadAndDisplayImage_Demo.Data;
using MvcCoreUploadAndDisplayImage_Demo.Models;

namespace MvcCoreUploadAndDisplayImage_Demo.Controllers
{
    public class ProfilesController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<IdentityUser> _userManager; // Add UserManager

        public ProfilesController(ApplicationDbContext context,UserManager<IdentityUser>userManager)
        {
            _userManager = userManager;
            _context = context;
        }


        // GET: Profiles
        public async Task<IActionResult> Index()
        {
            var userId = _userManager.GetUserId(User);
            var profile = await _context.Profiles.FirstOrDefaultAsync(p => p.UserId == userId);

            if (profile == null)
            {
                return RedirectToAction(nameof(Create));
            }

            return View(profile); // Only showing the user's profile
        }

        // GET: Profiles/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            var userId = _userManager.GetUserId(User);

            var profile = await _context.Profiles.FirstOrDefaultAsync(p => p.ProfileId == id && p.UserId == userId);
            if (profile == null)
            {
                return NotFound();
            }

            return View(profile);
        }


        // GET: Profiles/Create
        public async Task<IActionResult> Create()
        {
            var userId = _userManager.GetUserId(User);
            var userEmail = _userManager.GetUserName(User); // Get the logged-in user's email
            var existingProfile = await _context.Profiles.FirstOrDefaultAsync(p => p.UserId == userId);

            if (existingProfile != null)
            {
                return RedirectToAction(nameof(Index)); // Redirect to the profile if it already exists
            }

            // Pass the email to the view
            var model = new Profile { Email = userEmail };
            return View(model);
        }


        // POST: Profiles/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("ProfileId,FirstName,LastName,PhoneNumber,Email")] Profile profile)
        {
            var userId = _userManager.GetUserId(User);
            var userEmail = _userManager.GetUserName(User); // Get the logged-in user's email
            var existingProfile = await _context.Profiles.FirstOrDefaultAsync(p => p.UserId == userId);

            if (existingProfile != null)
            {
                return RedirectToAction(nameof(Index)); // Redirect to the profile if it already exists
            }

            if (ModelState.IsValid)
            {
                profile.UserId = userId; // Associate the profile with the logged-in user
                profile.Email = userEmail; // Ensure the email is set from the logged-in user
                _context.Add(profile);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(profile);
        }



        // GET: Profiles/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var profile = await _context.Profiles.FindAsync(id);
            if (profile == null)
            {
                return NotFound();
            }
            return View(profile);
        }

        // POST: Profiles/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("ProfileId,FirstName,LastName,PhoneNumber,Email")] Profile profile)
        {
            if (id != profile.ProfileId)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(profile);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!ProfileExists(profile.ProfileId))
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
            return View(profile);
        }

        // GET: Profiles/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var profile = await _context.Profiles
                .FirstOrDefaultAsync(m => m.ProfileId == id);
            if (profile == null)
            {
                return NotFound();
            }

            return View(profile);
        }
            
        // POST: Profiles/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var profile = await _context.Profiles.FindAsync(id);
            if (profile != null)
            {
                _context.Profiles.Remove(profile);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool ProfileExists(int id)
        {
            return _context.Profiles.Any(e => e.ProfileId == id);
        }
    }
}
