using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using ProiectMedicalLibrary.Data;
using ProiectMedicalLibrary.Models;
using System.Net.Http;
using System.Net.Http.Json;
using Newtonsoft.Json; // Pentru a converti obiectele din/în JSON
using System.Text;
using Proiect_medical.Models;


namespace Proiect_medical.Controllers
{
    [Authorize] // Restricționează accesul doar pentru utilizatorii autentificați
    public class AppointmentsController : Controller
    {
        //private readonly ApplicationDbContext _context;

        //public AppointmentsController(ApplicationDbContext context)
        //{
        //    _context = context;
        //}
         private readonly HttpClient _httpClient;
    private readonly string _baseUrl = "https://localhost:7108/api/Appointments"; // URL-ul API-ului

    public AppointmentsController(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

        // GET: Appointments
        [Authorize] // Asigură-te că doar utilizatorii autentificați pot accesa
        //public async Task<IActionResult> Index()
        //{
        //    var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;

        //    if (User.IsInRole("Patient"))
        //    {
        //        // Pacienții văd doar programările proprii
        //        var appointments = await _context.Appointments
        //            .Include(a => a.Doctor)
        //            .Include(a => a.Patient)
        //            .Where(a => a.Patient.UserId == userId)
        //            .ToListAsync();

        //        return View(appointments);
        //    }
        //    else if (User.IsInRole("Doctor"))
        //    {
        //        // Doctorii văd doar programările pacienților asociați
        //        var appointments = await _context.Appointments
        //            .Include(a => a.Doctor)
        //            .Include(a => a.Patient)
        //            .Where(a => a.Doctor.UserId == userId)
        //            .ToListAsync();

        //        return View(appointments);
        //    }

        //    return Forbid(); // Orice alt utilizator primește Access Denied
        //}
        public async Task<IActionResult> Index()
        {
            var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;

            // Obține programările de la API
            var response = await _httpClient.GetAsync(_baseUrl);

            if (response.IsSuccessStatusCode)
            {
                var appointments = JsonConvert.DeserializeObject<List<Appointment>>(
                    await response.Content.ReadAsStringAsync());

                // Filtrează programările în funcție de rolul utilizatorului
                if (User.IsInRole("Patient"))
                {
                    appointments = appointments
                        .Where(a => a.Patient?.UserId == userId)
                        .ToList();
                }
                else if (User.IsInRole("Doctor"))
                {
                    appointments = appointments
                        .Where(a => a.Doctor?.UserId == userId)
                        .ToList();
                }

                return View(appointments);
            }

            return StatusCode((int)response.StatusCode);
        }






        // GET: Appointments/Details/5

        //public async Task<IActionResult> Details(int? id)
        //{
        //    if (id == null)
        //    {
        //        return NotFound();
        //    }

        //    var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

        //    var appointment = await _context.Appointments
        //        .Include(a => a.Doctor)
        //        .Include(a => a.Patient)
        //        .FirstOrDefaultAsync(a =>
        //            a.Id == id && (
        //                (User.IsInRole("Patient") && a.Patient.UserId == userId) ||
        //                (User.IsInRole("Doctor") && a.Doctor.UserId == userId)
        //            ));

        //    if (appointment == null)
        //    {
        //        return Forbid(); // Acces refuzat dacă nu este asociată utilizatorului
        //    }

        //    return View(appointment);
        //}

        public async Task<IActionResult> Details(int? id)
        {
            // Verifică dacă ID-ul este null
            if (id == null)
            {
                return View("Error", new ErrorViewModel { RequestId = "ID-ul programării este null." });
            }

            try
            {
                // Apelează API-ul pentru a obține detaliile programării
                var response = await _httpClient.GetAsync($"{_baseUrl}/{id.Value}");
                if (!response.IsSuccessStatusCode)
                {
                    return View("Error", new ErrorViewModel { RequestId = $"Programarea cu ID {id} nu a fost găsită." });
                }

                // Deserializarea obiectului programare
                var appointment = JsonConvert.DeserializeObject<Appointment>(await response.Content.ReadAsStringAsync());

                // Verifică dacă programarea, doctorul și pacientul sunt complete
                if (appointment == null || appointment.Doctor == null || appointment.Patient == null)
                {
                    Console.WriteLine($"Doctor sau pacient lipsă pentru programarea cu ID {id}");
                    return View("Error", new ErrorViewModel
                    {
                        RequestId = $"Lipsesc date pentru doctor/pacient la programarea cu ID {id}."
                    });
                }

                // Obține userId-ul utilizatorului conectat
                var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

                // Verifică dacă utilizatorul are acces la programare
                if ((User.IsInRole("Patient") && appointment.Patient?.UserId != userId) ||
                    (User.IsInRole("Doctor") && appointment.Doctor?.UserId != userId))
                {
                    return Forbid(); // Acces interzis
                }

                // Returnează vizualizarea cu detaliile programării
                return View(appointment);
            }
            catch (Exception ex)
            {
                // Logare pentru debugging
                Console.WriteLine($"Eroare la obținerea detaliilor pentru programarea cu ID {id}: {ex.Message}");

                // Returnează pagina de eroare cu detalii
                return View("Error", new ErrorViewModel
                {
                    RequestId = $"Eroare internă la obținerea detaliilor programării: {ex.Message}"
                });
            }
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
        //[Authorize(Roles = "Patient")]
        //public IActionResult Create()
        //{
        //    ViewData["DoctorId"] = new SelectList(_context.Doctors, "Id", "Name"); // Select doctor
        //    return View();
        //}

        [Authorize(Roles = "Patient")]
        public async Task<IActionResult> Create()
        {
            if (User.IsInRole("Doctor"))
            {
                // Obține lista de pacienți pentru doctori
                var response = await _httpClient.GetAsync("https://localhost:7108/api/Patients");
                if (response.IsSuccessStatusCode)
                {
                    var patients = JsonConvert.DeserializeObject<List<Patient>>(await response.Content.ReadAsStringAsync());
                    ViewData["PatientId"] = new SelectList(patients, "Id", "Name");
                }
                else
                {
                    ViewData["PatientId"] = new SelectList(new List<Patient>(), "Id", "Name");
                }
            }
            else if (User.IsInRole("Patient"))
            {
                // Obține lista de doctori pentru pacienți
                var response = await _httpClient.GetAsync("https://localhost:7108/api/Doctors");
                if (response.IsSuccessStatusCode)
                {
                    var doctors = JsonConvert.DeserializeObject<List<Doctor>>(await response.Content.ReadAsStringAsync());
                    ViewData["DoctorId"] = new SelectList(doctors, "Id", "Name");
                }
                else
                {
                    ViewData["DoctorId"] = new SelectList(new List<Doctor>(), "Id", "Name");
                }
            }

            return View();
        }








        // POST: Appointments/Create

        //[Authorize(Roles = "Patient")]
        //public async Task<IActionResult> Create([Bind("Id,Date,Notes,DoctorId")] Appointment appointment)
        //{
        //    var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

        //    // Găsește pacientul conectat
        //    var patient = await _context.Patients.FirstOrDefaultAsync(p => p.UserId == userId);

        //    if (patient == null)
        //    {
        //        return Forbid();
        //    }

        //    appointment.PatientId = patient.Id; // Asociază programarea cu pacientul conectat

        //    if (ModelState.IsValid)
        //    {
        //        _context.Add(appointment);
        //        await _context.SaveChangesAsync();
        //        return RedirectToAction(nameof(Index));
        //    }

        //    ViewData["DoctorId"] = new SelectList(_context.Doctors, "Id", "Name", appointment.DoctorId);
        //    return View(appointment);
        //}

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Patient")]
        public async Task<IActionResult> Create([Bind("Id,Date,Notes,DoctorId,PatientId")] Appointment appointment)
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            // Asociază medicul conectat cu programarea
            if (User.IsInRole("Doctor"))
            {
                var doctorResponse = await _httpClient.GetAsync("https://localhost:7108/api/Doctors");
                if (doctorResponse.IsSuccessStatusCode)
                {
                    var doctors = JsonConvert.DeserializeObject<List<Doctor>>(await doctorResponse.Content.ReadAsStringAsync());
                    var doctor = doctors.FirstOrDefault(d => d.UserId == userId);

                    if (doctor == null)
                    {
                        ModelState.AddModelError("", "Medicul conectat nu a fost găsit.");
                        return View(appointment);
                    }

                    appointment.DoctorId = doctor.Id;
                }
            }

            // Creează programarea folosind API-ul
            var json = JsonConvert.SerializeObject(appointment);
            var response = await _httpClient.PostAsync(
                _baseUrl,
                new StringContent(json, Encoding.UTF8, "application/json"));

            if (response.IsSuccessStatusCode)
            {
                return RedirectToAction(nameof(Index));
            }

            ModelState.AddModelError("", "Eroare la salvarea programării.");
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

        // GET: Appointments/Edit/5
        [HttpGet]
      
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            // Obține programarea de la API
            var response = await _httpClient.GetAsync($"{_baseUrl}/{id.Value}");
            if (!response.IsSuccessStatusCode)
            {
                return NotFound();
            }

            var appointment = JsonConvert.DeserializeObject<Appointment>(await response.Content.ReadAsStringAsync());

            // Obține lista doctorilor
            var doctorResponse = await _httpClient.GetAsync("https://localhost:7108/api/Doctors");
            if (doctorResponse.IsSuccessStatusCode)
            {
                var doctors = JsonConvert.DeserializeObject<List<Doctor>>(await doctorResponse.Content.ReadAsStringAsync());
                ViewData["DoctorId"] = new SelectList(doctors, "Id", "Name", appointment.DoctorId);
            }

            // Obține lista pacienților
            var patientResponse = await _httpClient.GetAsync("https://localhost:7108/api/Patients");
            if (patientResponse.IsSuccessStatusCode)
            {
                var patients = JsonConvert.DeserializeObject<List<Patient>>(await patientResponse.Content.ReadAsStringAsync());
                ViewData["PatientId"] = new SelectList(patients, "Id", "Name", appointment.PatientId);
            }

            return View(appointment);
        }





        // POST: Appointments/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]

   
        public async Task<IActionResult> Edit(int id, [Bind("Id,Date,Notes,DoctorId,PatientId")] Appointment appointment)
        {
            if (id != appointment.Id)
            {
                return BadRequest();
            }

            var json = JsonConvert.SerializeObject(appointment);
            var response = await _httpClient.PutAsync($"{_baseUrl}/{id}",
                new StringContent(json, Encoding.UTF8, "application/json"));

            if (response.IsSuccessStatusCode)
            {
                return RedirectToAction(nameof(Index));
            }

            // Repopulează listele în cazul unei erori
            var doctorResponse = await _httpClient.GetAsync("https://localhost:7108/api/Doctors");
            if (doctorResponse.IsSuccessStatusCode)
            {
                var doctors = JsonConvert.DeserializeObject<List<Doctor>>(await doctorResponse.Content.ReadAsStringAsync());
                ViewData["DoctorId"] = new SelectList(doctors, "Id", "Name", appointment.DoctorId);
            }

            var patientResponse = await _httpClient.GetAsync("https://localhost:7108/api/Patients");
            if (patientResponse.IsSuccessStatusCode)
            {
                var patients = JsonConvert.DeserializeObject<List<Patient>>(await patientResponse.Content.ReadAsStringAsync());
                ViewData["PatientId"] = new SelectList(patients, "Id", "Name", appointment.PatientId);
            }

            ModelState.AddModelError("", "Eroare la actualizarea programării.");
            return View(appointment);
        }







        // GET: Appointments/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var response = await _httpClient.GetAsync($"{_baseUrl}/{id.Value}");
            if (!response.IsSuccessStatusCode)
            {
                return NotFound();
            }

            var appointment = JsonConvert.DeserializeObject<Appointment>(await response.Content.ReadAsStringAsync());
            return View(appointment);
        }



        // POST: Appointments/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]

   
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var response = await _httpClient.DeleteAsync($"{_baseUrl}/{id}");
            if (response.IsSuccessStatusCode)
            {
                return RedirectToAction(nameof(Index));
            }

            ModelState.AddModelError("", "Eroare la ștergerea programării.");
            return RedirectToAction(nameof(Delete), new { id });
        }





        private async Task<List<Doctor>> GetDoctors()
        {
            var response = await _httpClient.GetAsync("https://localhost:7108/api/Doctors");
            if (response.IsSuccessStatusCode)
            {
                return JsonConvert.DeserializeObject<List<Doctor>>(await response.Content.ReadAsStringAsync());
            }

            return new List<Doctor>();
        }

        private async Task<bool> AppointmentExists(int id)
        {
            // Apelează API-ul pentru a verifica existența programării
            var response = await _httpClient.GetAsync($"{_baseUrl}/{id}");
            return response.IsSuccessStatusCode;
        }

    }

}
