using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Proiect_medical.Data;
using Proiect_medical.Models;
using System.Security.Claims;

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

      

        [Authorize(Policy = "DoctorPolicy")]
        public async Task<IActionResult> Index()
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier).Value;

            var patients = await _context.Patients
                .Include(p => p.Appointments)
                    .ThenInclude(a => a.Doctor)
                .Where(p => p.Appointments.Any(a => a.Doctor.UserId == userId))
                .ToListAsync();

            return View(patients);
        }


        // GET: Patients/MyDetails
        [Authorize(Roles = "Doctor,Patient")] 
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

       
        [Authorize(Roles = "Doctor,Patient")]
        public async Task<IActionResult> Details(int id)
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            if (userId is null)
                return Forbid();

            var patient = await _context.Patients
                .Include(p => p.Subscription)
                .Include(p => p.Appointments)
                .ThenInclude(a => a.Doctor)
                .FirstOrDefaultAsync(p =>
                    (User.IsInRole("Doctor") && p.Appointments.Any(a => a.Doctor.UserId == userId)) ||
                    (User.IsInRole("Patient") && p.UserId == userId)
                );

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

        
        [Authorize(Roles = "Doctor,Patient")]
        public async Task<IActionResult> Edit(int id)
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier).Value;

           
            var patient = await _context.Patients
                .Include(p => p.Appointments)
                .ThenInclude(a => a.Doctor)
                .FirstOrDefaultAsync(p =>
                    p.Id == id &&
                    ((User.IsInRole("Doctor") && p.Appointments.Any(a => a.Doctor.UserId == userId)) ||
                    (User.IsInRole("Patient") && p.UserId == userId))
                );

            if (patient == null)
            {
                return Forbid();
            }

            return View(patient);
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Doctor,Patient")]
        public async Task<IActionResult> Edit(int id, [Bind("Id,Name")] Patient patient)
        {

            if (id != patient.Id)
            {
                Console.WriteLine("❌ ID mismatch");
                return NotFound();
            }

            var userId = User.FindFirst(ClaimTypes.NameIdentifier).Value;

            var existingPatient = await _context.Patients
                .Include(p => p.Appointments)
                .ThenInclude(a => a.Doctor)
                .FirstOrDefaultAsync(p =>
                    p.Id == id &&
                    ((User.IsInRole("Doctor") && p.Appointments.Any(a => a.Doctor.UserId == userId)) ||
                    (User.IsInRole("Patient") && p.UserId == userId))
                );

            if (existingPatient == null)
            {
                Console.WriteLine("Access denied to edit this patient");
                return Forbid();
            }

            try
            {
                existingPatient.Name = patient.Name; 

                Console.WriteLine("Updating patient in database...");
                _context.Patients.Update(existingPatient);
                await _context.SaveChangesAsync();
                Console.WriteLine("Patient updated successfully!");
            }
            catch (DbUpdateConcurrencyException ex)
            {
                Console.WriteLine($" Database update error: {ex.Message}");
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






        [Authorize(Policy = "DoctorPolicy")]
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null) return NotFound();

            var userId = User.FindFirst(ClaimTypes.NameIdentifier).Value;

            var patient = await _context.Patients
                .Include(p => p.Appointments)
                .ThenInclude(a => a.Doctor)
                .FirstOrDefaultAsync(p =>
                    p.Id == id && p.Appointments.Any(a => a.Doctor.UserId == userId)
                );

            if (patient == null)
            {
                return Forbid();
            }

            return View(patient);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        [Authorize(Policy = "DoctorPolicy")]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier).Value;

            var patient = await _context.Patients
                .Include(p => p.Appointments)
                .FirstOrDefaultAsync(p =>
                    p.Id == id && p.Appointments.Any(a => a.Doctor.UserId == userId)
                );

            if (patient != null)
            {
                _context.Appointments.RemoveRange(patient.Appointments); 
                _context.Patients.Remove(patient);
                await _context.SaveChangesAsync();
            }
            else
            {
                return Forbid();
            }

            return RedirectToAction(nameof(Index));
        }



        private bool PatientExists(int id)
        {
            return _context.Patients.Any(e => e.Id == id);
        }
    }
}
