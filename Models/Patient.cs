using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations;

namespace Proiect_medical.Models
{
    public class Patient
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "Name is required")]
        public string Name { get; set; }

        // Asociază pacientul cu un utilizator din Identity
        public string UserId { get; set; } // ID-ul utilizatorului ASP.NET Identity
        public IdentityUser User { get; set; } // Obiectul utilizatorului
        public ICollection<Appointment>? Appointments { get; set; }
    }
}
