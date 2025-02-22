using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Proiect_medical.Models
{
    public class Subscription
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public string Name { get; set; }

        [Required]
        [Column(TypeName = "decimal(18,2)")] 
        public decimal Price { get; set; }

        [Required]
        public string Description { get; set; }

        [Required]
        [Column("DurationInMonths")] 
        public int DurationInMonths { get; set; } 

        [Required]
        public bool IsActive { get; set; } 
    }
}
