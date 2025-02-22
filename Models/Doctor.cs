using Microsoft.AspNetCore.Identity;

namespace Proiect_medical.Models
{
    public class Doctor
    {
        public int Id { get; set; } 
        public string Name { get; set; }
        public int SpecializationId { get; set; } 
        public Specialization? Specialization { get; set; } 

        public ICollection<Appointment>? Appointments { get; set; }

        public string UserId { get; set; }
        public IdentityUser User { get; set; }

    }
}
