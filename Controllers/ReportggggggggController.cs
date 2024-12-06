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
    public class ReportggggggggController : Controller
    {
        private readonly ApplicationDbContext _context;

        public ReportggggggggController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: Reportgggggggg
        public async Task<IActionResult> Index()
        {
            return View(await _context.ReportApartments.ToListAsync());
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

        // GET: Reportgggggggg/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: Reportgggggggg/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,ReportId,ApartmentName,Address,ReportDescription,ReportDate,Email,IsResolved")] ReportApartment reportApartment)
        {
            if (ModelState.IsValid)
            {
                _context.Add(reportApartment);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(reportApartment);
        }

        // GET: Reportgggggggg/Edit/5
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

        // POST: Reportgggggggg/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
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
}
