namespace Proiect_medical.Models
{
    public class Specialization
    {
        public int Id { get; set; } // Cheia primară
        public string? Name { get; set; } // Numele specializării (ex: Cardiologie, Ortopedie)

        // Relație one-to-many cu Doctor
        public ICollection<Doctor>? Doctors { get; set; }
    }
}
