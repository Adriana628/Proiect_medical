using System;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ProiectMedicalLibrary.Data;
using ProiectMedicalLibrary.Models;

namespace Proiect_medical.Controllers
{
    [Authorize] 
    public class PatientsController : Controller
    {
        private readonly ApplicationDbContext _context;

        public PatientsController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: Patients
        [Authorize(Roles = "Doctor")] 
        public async Task<IActionResult> Index()
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier).Value;

            var patients = await _context.Patients
                .Include(p => p.Appointments)
                .Where(p => p.Appointments.Any(a => a.Doctor.UserId == userId))
                .ToListAsync();

            return View(patients);
        }

        // GET: Patients/MyDetails
        [Authorize(Roles = "Patient")] 
        public async Task<IActionResult> MyDetails()
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier).Value;

            var patient = await _context.Patients
                .FirstOrDefaultAsync(p => p.UserId == userId);

            if (patient == null)
            {
                return NotFound();
            }

            return View("Details", patient); 
        }

        // GET: Patients/Details/5
        [Authorize(Roles = "Doctor,Patient")] 
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var userId = User.FindFirst(ClaimTypes.NameIdentifier).Value;

            
            var patient = await _context.Patients
                .FirstOrDefaultAsync(m =>
                    m.Id == id && (
                        (User.IsInRole("Doctor") && m.Appointments.Any(a => a.Doctor.UserId == userId)) ||
                        (User.IsInRole("Patient") && m.UserId == userId)
                    ));

            if (patient == null)
            {
                return Forbid(); 
            }

            return View(patient);
        }

        // GET: Patients/Create
        [Authorize(Roles = "Doctor")] 
        public IActionResult Create()
        {
            return View();
        }

        // POST: Patients/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Doctor")] 
        public async Task<IActionResult> Create([Bind("Id,Name,Email,Phone")] Patient patient)
        {
            if (ModelState.IsValid)
            {
                _context.Add(patient);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(patient);
        }

        // GET: Patients/Edit/5
        [Authorize(Roles = "Doctor,Patient")] 
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var userId = User.FindFirst(ClaimTypes.NameIdentifier).Value;

           
            var patient = await _context.Patients
                .FirstOrDefaultAsync(p =>
                    p.Id == id && (
                        (User.IsInRole("Doctor") && p.Appointments.Any(a => a.Doctor.UserId == userId)) ||
                        (User.IsInRole("Patient") && p.UserId == userId)
                    ));

            if (patient == null)
            {
                return Forbid();
            }

            return View(patient);
        }

        // POST: Patients/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Doctor,Patient")] 
        public async Task<IActionResult> Edit(int id, [Bind("Id,Name,Email,Phone")] Patient patient)
        {
            if (id != patient.Id)
            {
                return NotFound();
            }

            var userId = User.FindFirst(ClaimTypes.NameIdentifier).Value;

            
            var existingPatient = await _context.Patients
                .FirstOrDefaultAsync(p =>
                    p.Id == id && (
                        (User.IsInRole("Doctor") && p.Appointments.Any(a => a.Doctor.UserId == userId)) ||
                        (User.IsInRole("Patient") && p.UserId == userId)
                    ));

            if (existingPatient == null)
            {
                return Forbid();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(patient);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!PatientExists(patient.Id))
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
            return View(patient);
        }

        private bool PatientExists(int id)
        {
            return _context.Patients.Any(e => e.Id == id);
        }
    }
}
