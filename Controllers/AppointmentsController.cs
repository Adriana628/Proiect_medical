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
using Newtonsoft.Json; 
using System.Text;
using Proiect_medical.Models;


namespace Proiect_medical.Controllers
{
    [Authorize] 
    public class AppointmentsController : Controller
    {
         private readonly HttpClient _httpClient;
        private readonly string _baseUrl = "https://localhost:7108/api/Appointments"; // URL-ul API-ului

    public AppointmentsController(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

        // GET: Appointments
        [Authorize]
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

        //    return Forbid(); 
        //}
        public async Task<IActionResult> Index()
        {
            var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;

           
            var response = await _httpClient.GetAsync(_baseUrl);

            if (response.IsSuccessStatusCode)
            {
                var appointments = JsonConvert.DeserializeObject<List<Appointment>>(
                    await response.Content.ReadAsStringAsync());

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
        //        return Forbid(); 
        //    }

        //    return View(appointment);
        //}

        public async Task<IActionResult> Details(int? id)
        {
           
            if (id == null)
            {
                return View("Error", new ErrorViewModel { RequestId = "ID-ul programării este null." });
            }

            try
            {
              
                var response = await _httpClient.GetAsync($"{_baseUrl}/{id.Value}");
                if (!response.IsSuccessStatusCode)
                {
                    return View("Error", new ErrorViewModel { RequestId = $"Programarea cu ID {id} nu a fost găsită." });
                }

                
                var appointment = JsonConvert.DeserializeObject<Appointment>(await response.Content.ReadAsStringAsync());

                
                if (appointment == null || appointment.Doctor == null || appointment.Patient == null)
                {
                    Console.WriteLine($"Doctor sau pacient lipsă pentru programarea cu ID {id}");
                    return View("Error", new ErrorViewModel
                    {
                        RequestId = $"Lipsesc date pentru doctor/pacient la programarea cu ID {id}."
                    });
                }

               
                var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

                
                if ((User.IsInRole("Patient") && appointment.Patient?.UserId != userId) ||
                    (User.IsInRole("Doctor") && appointment.Doctor?.UserId != userId))
                {
                    return Forbid(); // Acces interzis
                }

               
                return View(appointment);
            }
            catch (Exception ex)
            {
              
                Console.WriteLine($"Eroare la obținerea detaliilor pentru programarea cu ID {id}: {ex.Message}");

                
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
        //[Authorize(Roles = "Doctor")] 
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
        //    ViewData["DoctorId"] = new SelectList(_context.Doctors, "Id", "Name"); 
        //    return View();
        //}

        [Authorize(Roles = "Patient")]
        public async Task<IActionResult> Create()
        {
            if (User.IsInRole("Doctor"))
            {
                
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

        //    appointment.PatientId = patient.Id;

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

        public async Task<IActionResult> Create([Bind("Date,Notes,DoctorId")] Appointment appointment, string TimeSlot)
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId))
            {
                return Unauthorized("⚠️ Utilizatorul nu este autentificat!");
            }

            // Găsește pacientul logat
            var response = await _httpClient.GetAsync("https://localhost:7108/api/Patients");
            if (!response.IsSuccessStatusCode)
            {
                return BadRequest("⚠️ Nu s-au putut încărca pacienții.");
            }

            var patients = JsonConvert.DeserializeObject<List<Patient>>(await response.Content.ReadAsStringAsync());
            var loggedInPatient = patients.FirstOrDefault(p => p.UserId == userId);

            if (loggedInPatient == null)
            {
                return Unauthorized("⚠️ Pacientul logat nu există în sistem!");
            }

            // **NU mai setăm PatientId manual! API-ul îl va prelua singur**
            // appointment.PatientId = loggedInPatient.Id;

            // Concatenează data și ora
            if (DateTime.TryParse($"{appointment.Date.ToShortDateString()} {TimeSlot}", out DateTime fullDateTime))
            {
                appointment.Date = fullDateTime;
            }
            else
            {
                ModelState.AddModelError("TimeSlot", "Ora selectată este invalidă.");
            }

            if (!ModelState.IsValid)
            {
                await PopulateDropdowns(appointment.DoctorId);
                return View(appointment);
            }

            // Trimite datele către API (FĂRĂ PatientId)
            var json = JsonConvert.SerializeObject(appointment);
            var apiResponse = await _httpClient.PostAsync($"{_baseUrl}", new StringContent(json, Encoding.UTF8, "application/json"));

            if (apiResponse.IsSuccessStatusCode)
            {
                return RedirectToAction(nameof(Index));
            }

            ModelState.AddModelError("", "Eroare la crearea programării.");
            await PopulateDropdowns(appointment.DoctorId);
            return View(appointment);
        }




        private async Task PopulateDropdowns(int? selectedDoctorId = null)
        {
            // Cerere către API-ul pentru lista de doctori
            var doctorResponse = await _httpClient.GetAsync("https://localhost:7108/api/Doctors");

            if (doctorResponse.IsSuccessStatusCode)
            {
                var doctors = JsonConvert.DeserializeObject<List<Doctor>>(await doctorResponse.Content.ReadAsStringAsync());
                if (doctors != null && doctors.Any())
                {
                    ViewData["DoctorId"] = new SelectList(doctors, "Id", "Name", selectedDoctorId);
                }
                else
                {
                    ViewData["DoctorId"] = new SelectList(new List<Doctor> { new Doctor { Id = 0, Name = "Nu sunt doctori disponibili" } }, "Id", "Name");
                }
            }
            else
            {
                ViewData["DoctorId"] = new SelectList(new List<Doctor> { new Doctor { Id = 0, Name = "Eroare la încărcarea doctorilor" } }, "Id", "Name");
            }

            // Cerere către API-ul pentru lista de pacienți
            var patientResponse = await _httpClient.GetAsync("https://localhost:7108/api/Patients");

            if (patientResponse.IsSuccessStatusCode)
            {
                var patients = JsonConvert.DeserializeObject<List<Patient>>(await patientResponse.Content.ReadAsStringAsync());
                ViewData["PatientId"] = new SelectList(patients, "Id", "Name");
            }
            else
            {
                ViewData["PatientId"] = new SelectList(new List<Patient>(), "Id", "Name");
            }
        }


        // Metodă pentru a încărca lista doctorilor
        private async Task LoadDoctorsInViewData()
        {
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








        // POST: Appointments/Create
        //[HttpPost]
        //[ValidateAntiForgeryToken]
        //[Authorize(Roles = "Doctor")] 
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
        //[Authorize(Roles = "Doctor")] 
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

            
            var response = await _httpClient.GetAsync($"{_baseUrl}/{id.Value}");
            if (!response.IsSuccessStatusCode)
            {
                return NotFound();
            }

            var appointment = JsonConvert.DeserializeObject<Appointment>(await response.Content.ReadAsStringAsync());

            
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
