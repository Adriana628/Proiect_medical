using System.Linq;
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
    [Authorize] // Restricționează accesul doar pentru utilizatorii autentificați
    public class AppointmentsController : Controller
    {
        private readonly ApplicationDbContext _context;

        public AppointmentsController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: Appointments
        [Authorize] // Asigură-te că doar utilizatorii autentificați pot accesa
        public async Task<IActionResult> Index()
        {
            var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;

            if (User.IsInRole("Patient"))
            {
                // Pacienții văd doar programările proprii
                var appointments = await _context.Appointments
                    .Include(a => a.Doctor)
                    .Include(a => a.Patient)
                    .Where(a => a.Patient.UserId == userId)
                    .ToListAsync();

                return View(appointments);
            }
            else if (User.IsInRole("Doctor"))
            {
                // Doctorii văd doar programările pacienților asociați
                var appointments = await _context.Appointments
                    .Include(a => a.Doctor)
                    .Include(a => a.Patient)
                    .Where(a => a.Doctor.UserId == userId)
                    .ToListAsync();

                return View(appointments);
            }

            return Forbid(); // Orice alt utilizator primește Access Denied
        }





        // GET: Appointments/Details/5

        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            var appointment = await _context.Appointments
                .Include(a => a.Doctor)
                .Include(a => a.Patient)
                .FirstOrDefaultAsync(a =>
                    a.Id == id && (
                        (User.IsInRole("Patient") && a.Patient.UserId == userId) ||
                        (User.IsInRole("Doctor") && a.Doctor.UserId == userId)
                    ));

            if (appointment == null)
            {
                return Forbid(); // Acces refuzat dacă nu este asociată utilizatorului
            }

            return View(appointment);
        }


        //public async Task<IActionResult> Details(int? id)
        //{
        //    if (id == null)
        //    {
        //        return NotFound();
        //    }

        //    var userId = User.FindFirst(ClaimTypes.NameIdentifier).Value;

        //    var appointment = await _context.Appointments
        //        .Include(a => a.Doctor)
        //        .Include(a => a.Patient)
        //        .FirstOrDefaultAsync(m =>
        //            m.Id == id && (
        //                (User.IsInRole("Doctor") && m.Doctor.UserId == userId) ||
        //                (User.IsInRole("Patient") && m.Patient.UserId == userId)
        //            ));

        //    if (appointment == null)
        //    {
        //        return Forbid();
        //    }

        //    return View(appointment);
        //}

        // GET: Appointments/Create
        //[Authorize(Roles = "Doctor")] // Doar doctorii pot crea programări
        //public IActionResult Create()
        //{
        //    ViewData["PatientId"] = new SelectList(_context.Patients, "Id", "Name");
        //    return View();
        //}

        // GET: Appointments/Create
        // GET: Appointments/Create
        [Authorize(Roles = "Patient")]
        public IActionResult Create()
        {
            ViewData["DoctorId"] = new SelectList(_context.Doctors, "Id", "Name"); // Select doctor
            return View();
        }

        // POST: Appointments/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Patient")]
        public async Task<IActionResult> Create([Bind("Id,Date,Notes,DoctorId")] Appointment appointment)
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            // Găsește pacientul conectat
            var patient = await _context.Patients.FirstOrDefaultAsync(p => p.UserId == userId);

            if (patient == null)
            {
                return Forbid();
            }

            appointment.PatientId = patient.Id; // Asociază programarea cu pacientul conectat

            if (ModelState.IsValid)
            {
                _context.Add(appointment);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }

            ViewData["DoctorId"] = new SelectList(_context.Doctors, "Id", "Name", appointment.DoctorId);
            return View(appointment);
        }




        // POST: Appointments/Create
        //[HttpPost]
        //[ValidateAntiForgeryToken]
        //[Authorize(Roles = "Doctor")] // Doar doctorii pot crea programări
        //public async Task<IActionResult> Create([Bind("Id,Date,Notes,PatientId")] Appointment appointment)
        //{
        //    var userId = User.FindFirst(ClaimTypes.NameIdentifier).Value;

        //    // Asociază programarea cu doctorul conectat
        //    var doctor = await _context.Doctors.FirstOrDefaultAsync(d => d.UserId == userId);

        //    if (doctor == null)
        //    {
        //        return Forbid();
        //    }

        //    appointment.DoctorId = doctor.Id;

        //    if (ModelState.IsValid)
        //    {
        //        _context.Add(appointment);
        //        await _context.SaveChangesAsync();
        //        return RedirectToAction(nameof(Index));
        //    }

        //    ViewData["PatientId"] = new SelectList(_context.Patients, "Id", "Name", appointment.PatientId);
        //    return View(appointment);
        //}

        //[HttpPost]
        //[ValidateAntiForgeryToken]
        //[Authorize(Roles = "Doctor")] // Doar doctorii pot crea programări
        //public async Task<IActionResult> Create([Bind("Id,Date,Notes,PatientId")] Appointment appointment)
        //{
        //    var userId = User.FindFirst(ClaimTypes.NameIdentifier).Value;
        //    Console.WriteLine($"Doctor User ID: {userId}");
        //    // Asociază programarea cu doctorul conectat
        //    var doctor = await _context.Doctors.FirstOrDefaultAsync(d => d.UserId == userId);

        //    if (doctor == null)
        //    {
        //        return Forbid();
        //    }

        //    appointment.DoctorId = doctor.Id;

        //    if (ModelState.IsValid)
        //    {
        //        _context.Add(appointment);
        //        await _context.SaveChangesAsync();

        //        // Redirecționează utilizatorul la lista programărilor proprii
        //        if (User.IsInRole("Doctor"))
        //        {
        //            return RedirectToAction("Index", "Appointments");
        //        }
        //        else if (User.IsInRole("Patient"))
        //        {
        //            return RedirectToAction("Index", "Appointments");
        //        }
        //        else
        //        {
        //            return Forbid();
        //        }

        //    }

        //    ViewData["PatientId"] = new SelectList(_context.Patients, "Id", "Name", appointment.PatientId);
        //    return View(appointment);
        //}


        private bool AppointmentExists(int id)
        {
            return _context.Appointments.Any(e => e.Id == id);
        }
    }
}
