﻿using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Proiect_medical.Data;
using Proiect_medical.Models;

namespace Proiect_medical.Controllers
{
    [Authorize] // Toți utilizatorii autentificați pot accesa controllerul
    public class DoctorsController : Controller
    {
        private readonly ApplicationDbContext _context;

        public DoctorsController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: Doctors
        [AllowAnonymous] // Permite accesul la această metodă pentru toți utilizatorii, inclusiv cei neautentificați
        public async Task<IActionResult> Index()
        {
            var applicationDbContext = _context.Doctors.Include(d => d.Specialization);
            return View(await applicationDbContext.ToListAsync());
        }

        // GET: Doctors/Details/5
        [AllowAnonymous] // Permite accesul la această metodă pentru toți utilizatorii
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var doctor = await _context.Doctors
                .Include(d => d.Specialization)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (doctor == null)
            {
                return NotFound();
            }

            return View(doctor);
        }

        // GET: Doctors/Create
        [Authorize(Roles = "Doctor")] // Doar doctorii pot crea noi înregistrări
        public IActionResult Create()
        {
            ViewData["SpecializationId"] = new SelectList(_context.Specializations, "Id", "Id");
            return View();
        }

        // POST: Doctors/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Doctor")] // Doar doctorii pot crea noi înregistrări
        public async Task<IActionResult> Create([Bind("Id,Name,Email,Phone,SpecializationId")] Doctor doctor)
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier).Value; // Obține ID-ul utilizatorului conectat
            doctor.UserId = userId;

            if (ModelState.IsValid)
            {
                _context.Add(doctor);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(doctor);
        }

        // GET: Doctors/Edit/5
        [Authorize(Roles = "Doctor")] // Doar doctorii pot edita propriile înregistrări
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var doctor = await _context.Doctors.FindAsync(id);
            if (doctor == null)
            {
                return NotFound();
            }

            var userId = User.FindFirst(ClaimTypes.NameIdentifier).Value;
            if (doctor.UserId != userId)
            {
                return Forbid(); // Doctorii pot edita doar propriile informații
            }

            ViewData["SpecializationId"] = new SelectList(_context.Specializations, "Id", "Id", doctor.SpecializationId);
            return View(doctor);
        }

        // POST: Doctors/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Doctor")] // Doar doctorii pot edita propriile înregistrări
        public async Task<IActionResult> Edit(int id, [Bind("Id,Name,Email,Phone,SpecializationId")] Doctor doctor)
        {
            if (id != doctor.Id)
            {
                return NotFound();
            }

            var userId = User.FindFirst(ClaimTypes.NameIdentifier).Value;
            if (doctor.UserId != userId)
            {
                return Forbid(); // Doctorii pot edita doar propriile informații
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(doctor);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!DoctorExists(doctor.Id))
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
            ViewData["SpecializationId"] = new SelectList(_context.Specializations, "Id", "Id", doctor.SpecializationId);
            return View(doctor);
        }

        // GET: Doctors/Delete/5
        [Authorize(Roles = "Doctor")] // Doar doctorii pot șterge propriile înregistrări
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var doctor = await _context.Doctors
                .Include(d => d.Specialization)
                .FirstOrDefaultAsync(m => m.Id == id);

            var userId = User.FindFirst(ClaimTypes.NameIdentifier).Value;
            if (doctor == null || doctor.UserId != userId)
            {
                return Forbid(); // Doctorii pot șterge doar propriile înregistrări
            }

            return View(doctor);
        }

        // POST: Doctors/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Doctor")] // Doar doctorii pot șterge propriile înregistrări
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var doctor = await _context.Doctors.FindAsync(id);
            if (doctor != null)
            {
                var userId = User.FindFirst(ClaimTypes.NameIdentifier).Value;
                if (doctor.UserId != userId)
                {
                    return Forbid(); // Doctorii pot șterge doar propriile înregistrări
                }

                _context.Doctors.Remove(doctor);
                await _context.SaveChangesAsync();
            }

            return RedirectToAction(nameof(Index));
        }

        private bool DoctorExists(int id)
        {
            return _context.Doctors.Any(e => e.Id == id);
        }
    }
}
