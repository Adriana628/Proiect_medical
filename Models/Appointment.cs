namespace Proiect_medical.Models
{
    public class Appointment
    {
        public int Id { get; set; } // Cheia primară
        public DateTime Date { get; set; }
        public string? Notes { get; set; } // Notițe despre consultație

        // Chei străine
        public int PatientId { get; set; }
        public Patient? Patient { get; set; } // Relație many-to-one cu Patient

        public int DoctorId { get; set; }
        public Doctor? Doctor { get; set; } // Relație many-to-one cu Doctor
    }
}
