namespace Proiect_medical.Models
{
    public class Message
    {
        public int Id { get; set; } 
        public string? Content { get; set; } 
        public DateTime SentAt { get; set; } 

   
        public int? SenderDoctorId { get; set; } 
        public Doctor? SenderDoctor { get; set; } 

        public int? SenderPatientId { get; set; } 
        public Patient? SenderPatient { get; set; } 

        public int? ReceiverDoctorId { get; set; }
        public Doctor? ReceiverDoctor { get; set; } 

        public int? ReceiverPatientId { get; set; } 
        public Patient? ReceiverPatient { get; set; }
    }
}
