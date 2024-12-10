using Microsoft.AspNetCore.Identity;

namespace Proiect_medical.Models
{
    public class Doctor
    {
        public int Id { get; set; } // Cheia primară
        public string Name { get; set; }
        public int SpecializationId { get; set; } // Cheie străină către Specialization
        public Specialization? Specialization { get; set; } // Relație one-to-one cu Specialization

        // Relație one-to-many cu Appointment
        public ICollection<Appointment>? Appointments { get; set; }

        // Asociază doctorul cu un utilizator ASP.NET Identity
        public string UserId { get; set; }
        public IdentityUser User { get; set; }

    }
}
