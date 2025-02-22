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
    [Authorize] 
    public class AppointmentsController : Controller
    {
        private readonly ApplicationDbContext _context;

        public AppointmentsController(ApplicationDbContext context)
        {
            _context = context;
        }

        
        [Authorize] 
        public async Task<IActionResult> Index()
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            var currentDateTime = DateTime.Now; 

            if (User.IsInRole("Patient"))
            {
                
                var appointments = await _context.Appointments
                    .Include(a => a.Doctor)
                    .Include(a => a.Patient)
                    .Where(a => a.Patient.UserId == userId && a.Date >= currentDateTime)
                    .ToListAsync();

                return View(appointments);
            }
            else if (User.IsInRole("Doctor"))
            {
                
                var appointments = await _context.Appointments
                    .Include(a => a.Doctor)
                    .Include(a => a.Patient)
                    .Where(a => a.Doctor.UserId == userId && a.Date >= currentDateTime)
                    .ToListAsync();

                return View(appointments);
            }

            return Forbid(); 
        }


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
                return Forbid(); 
            }

            return View(appointment);
        }


        [Authorize(Roles = "Patient")]
        public IActionResult Create()
        {
            ViewData["DoctorId"] = new SelectList(_context.Doctors, "Id", "Name");
            ViewData["TimeSlots"] = GetAvailableTimeSlots(); 

            return View();
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Patient")]
        public async Task<IActionResult> Create([Bind("Id,Date,Notes,DoctorId")] Appointment appointment, string TimeSlot)
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            var patient = await _context.Patients.FirstOrDefaultAsync(p => p.UserId == userId);

            if (patient == null)
            {
                return Forbid();
            }

            appointment.PatientId = patient.Id;

            
            if (!DateTime.TryParse($"{appointment.Date:yyyy-MM-dd} {TimeSlot}", out DateTime appointmentDateTime))
            {
                ModelState.AddModelError("Date", "Ora selectată nu este validă.");
            }
            else
            {
                appointment.Date = appointmentDateTime;
            }

          
            bool patientHasConflict = await _context.Appointments.AnyAsync(a =>
                a.PatientId == appointment.PatientId && a.Date == appointment.Date);

            if (patientHasConflict)
            {
                ModelState.AddModelError("Date", "Ai deja o programare în acest interval orar!");
            }

            if (ModelState.IsValid)
            {
                _context.Add(appointment);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }

            ViewData["DoctorId"] = new SelectList(_context.Doctors, "Id", "Name", appointment.DoctorId);
            ViewData["TimeSlots"] = GetAvailableTimeSlots();
            return View(appointment);
        }



        private bool AppointmentExists(int id)
        {
            return _context.Appointments.Any(e => e.Id == id);
        }

        private List<SelectListItem> GetAvailableTimeSlots()
        {
            var slots = new List<SelectListItem>();
            TimeSpan startTime = new TimeSpan(8, 0, 0); 
            TimeSpan endTime = new TimeSpan(18, 0, 0); 
            TimeSpan interval = new TimeSpan(0, 30, 0); 

            for (var time = startTime; time < endTime; time = time.Add(interval))
            {
                slots.Add(new SelectListItem { Value = time.ToString(@"hh\:mm"), Text = time.ToString(@"hh\:mm") });
            }

            return slots;
        }

   
        [Authorize]
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var appointment = await _context.Appointments
                .Include(a => a.Patient)
                .Include(a => a.Doctor)
                .FirstOrDefaultAsync(m => m.Id == id);

            if (appointment == null)
            {
                return NotFound();
            }

            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            if (!User.IsInRole("Admin") &&
                !(User.IsInRole("Patient") && appointment.Patient?.UserId == userId) &&
                !(User.IsInRole("Doctor") && appointment.Doctor?.UserId == userId))
            {
                return Forbid();
            }

           
            ViewData["DoctorId"] = new SelectList(_context.Doctors, "Id", "Name", appointment.DoctorId);

           
            ViewData["TimeSlots"] = GetAvailableTimeSlots();

            
            ViewData["SelectedTime"] = appointment.Date.ToString("HH:mm");

            return View(appointment);
        }




        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize]
        public async Task<IActionResult> Edit(int id, [Bind("Id,Date,Notes,DoctorId")] Appointment appointment, string TimeSlot)
        {
            if (id != appointment.Id)
            {
                return NotFound();
            }

            var existingAppointment = await _context.Appointments
                .Include(a => a.Patient)
                .Include(a => a.Doctor)
                .FirstOrDefaultAsync(a => a.Id == id);

            if (existingAppointment == null)
            {
                return NotFound();
            }

            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            if (!User.IsInRole("Admin") &&
                !(User.IsInRole("Patient") && existingAppointment.Patient?.UserId == userId) &&
                !(User.IsInRole("Doctor") && existingAppointment.Doctor?.UserId == userId))
            {
                return Forbid();
            }

          
            if (!DateTime.TryParse($"{appointment.Date:yyyy-MM-dd} {TimeSlot}", out DateTime appointmentDateTime))
            {
                ModelState.AddModelError("Date", "Ora selectată nu este validă.");
            }
            else
            {
                existingAppointment.Date = appointmentDateTime;
            }

            existingAppointment.Notes = appointment.Notes;
            existingAppointment.DoctorId = appointment.DoctorId;

          
            bool doctorConflict = await _context.Appointments.AnyAsync(a =>
                a.DoctorId == existingAppointment.DoctorId &&
                a.Date == existingAppointment.Date &&
                a.Id != existingAppointment.Id); 

            if (doctorConflict)
            {
                ModelState.AddModelError("DoctorId", "Doctorul are deja o programare în acest interval orar.");
            }

            bool patientConflict = await _context.Appointments.AnyAsync(a =>
                a.PatientId == existingAppointment.PatientId &&
                a.Date == existingAppointment.Date &&
                a.Id != existingAppointment.Id); 

            if (patientConflict)
            {
                ModelState.AddModelError("Date", "Ai deja o programare în acest interval orar.");
            }

            if (!ModelState.IsValid)
            {
                ViewData["DoctorId"] = new SelectList(_context.Doctors, "Id", "Name", existingAppointment.DoctorId);
                ViewData["TimeSlots"] = GetAvailableTimeSlots();
                ViewData["SelectedTime"] = existingAppointment.Date.ToString("HH:mm");
                return View(existingAppointment);
            }

            try
            {
                _context.Update(existingAppointment);
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!_context.Appointments.Any(e => e.Id == appointment.Id))
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


        [Authorize]
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var appointment = await _context.Appointments
                .Include(a => a.Doctor)
                .Include(a => a.Patient)
                .FirstOrDefaultAsync(m => m.Id == id);

            if (appointment == null)
            {
                return NotFound();
            }

          
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (!User.IsInRole("Admin") &&
                !(User.IsInRole("Patient") && appointment.Patient.UserId == userId) &&
                !(User.IsInRole("Doctor") && appointment.Doctor.UserId == userId))
            {
                return Forbid();
            }

            return View(appointment);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        [Authorize]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var appointment = await _context.Appointments
                .Include(a => a.Patient) 
                .Include(a => a.Doctor)  
                .FirstOrDefaultAsync(a => a.Id == id);

            if (appointment == null)
            {
                return NotFound();
            }

            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            if (!User.IsInRole("Admin") &&
                !(User.IsInRole("Patient") && appointment.Patient?.UserId == userId) &&
                !(User.IsInRole("Doctor") && appointment.Doctor?.UserId == userId))
            {
                return Forbid();
            }

            _context.Appointments.Remove(appointment);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }


    }
}
