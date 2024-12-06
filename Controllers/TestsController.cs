using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using MvcCoreUploadAndDisplayImage_Demo.Data;
using MvcCoreUploadAndDisplayImage_Demo.Models;

namespace MvcCoreUploadAndDisplayImage_Demo.Controllers
{
    public class TestsController : Controller
    {
        private readonly ApplicationDbContext _context;

        public TestsController(ApplicationDbContext context)
        {
            _context = context;
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

            // Query all records from the Tests table
            IQueryable<Test> managersQuery = _context.Tests;

            if (!isAdmin)
            {
                // If not an Admin, filter records by the logged-in user's email
                managersQuery = managersQuery.Where(m => m.Email == currentUserEmail);
            }

            // Fetch the filtered list of records
            var managers = await managersQuery.ToListAsync();

            // Pass the records to the view
            return View(managers);
        }

        // GET: Tests/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var test = await _context.Tests
                .FirstOrDefaultAsync(m => m.Id == id);
            if (test == null)
            {
                return NotFound();
            }

            return View(test);
        }

        // GET: Tests/Create
        // GET: Tests/Create
        public async Task<IActionResult> Create()
        {
            // Fetch the list of managers with status "Pending" and store it in ViewBag
            ViewBag.PendingManagers = await _context.Managers
                .Where(m => m.Status == ManagerStatus.Pending) // Adjust this condition based on your Manager model
                .ToListAsync();

            return View();
        }


        // POST: Tests/Create
        // POST: Tests/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Test test)
        {
            if (ModelState.IsValid)
            {
                // Save the email directly from the form submission
                // Email will be filled in by the dropdown selection
                _context.Add(test);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }

            // Repopulate the list of pending managers in case of a validation error
            var pendingManagers = await _context.Managers
                .Where(m => m.Status == ManagerStatus.Pending)
                .Select(m => new { m.Id, m.Email })
                .ToListAsync();

            ViewBag.PendingManagers = pendingManagers;

            return View(test);
        }




        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var test = await _context.Tests.FindAsync(id);
            if (test == null)
            {
                return NotFound();
            }

            return View(test); // Make sure the view exists at Views/Tests/Edit.cshtml
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, Test test)
        {
            // Check if ID matches
            if (id != test.Id)
            {
                return NotFound();
            }

            try
            {
                // Find the existing entity in the database
                var existingTest = await _context.Tests.FindAsync(id);
                if (existingTest == null)
                {
                    return NotFound();
                }

                // Update only the password field
                existingTest.Password = test.Password;

                // Save changes to the database
                _context.Update(existingTest);
                await _context.SaveChangesAsync();

                // Redirect to index or confirmation page
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                // Log exception for debugging
                Console.WriteLine(ex.Message);
                return View(test); // Stay on the same view in case of error
            }
        }



        private bool TestExists(int id)
        {
            return _context.Tests.Any(e => e.Id == id);
        }


        // GET: Tests/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var test = await _context.Tests
                .FirstOrDefaultAsync(m => m.Id == id);
            if (test == null)
            {
                return NotFound();
            }

            return View(test);
        }

        // POST: Tests/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var test = await _context.Tests.FindAsync(id);
            if (test != null)
            {
                _context.Tests.Remove(test);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool TestExists2(int id)
        {
            return _context.Tests.Any(e => e.Id == id);
        }
    }
}
