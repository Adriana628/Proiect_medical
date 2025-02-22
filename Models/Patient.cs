using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations;

namespace Proiect_medical.Models
{
    public class Patient
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "Name is required")]
        public string Name { get; set; }

    
        public string UserId { get; set; } 
        public IdentityUser User { get; set; } 
        public ICollection<Appointment>? Appointments { get; set; }

        public int? SubscriptionId { get; set; }
        public Subscription Subscription { get; set; }

    }
}
