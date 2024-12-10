namespace Proiect_medical.Models
{
    public class Message
    {
        public int Id { get; set; } // Cheia primară
        public string? Content { get; set; } // Conținutul mesajului
        public DateTime SentAt { get; set; } // Data și ora trimiterii mesajului

        // Chei străine
        public int? SenderDoctorId { get; set; } // ID-ul medicului care trimite mesajul (dacă este medic)
        public Doctor? SenderDoctor { get; set; } // Relație many-to-one cu Doctor

        public int? SenderPatientId { get; set; } // ID-ul pacientului care trimite mesajul (dacă este pacient)
        public Patient? SenderPatient { get; set; } // Relație many-to-one cu Patient

        public int? ReceiverDoctorId { get; set; } // ID-ul medicului care primește mesajul (dacă este medic)
        public Doctor? ReceiverDoctor { get; set; } // Relație many-to-one cu Doctor

        public int? ReceiverPatientId { get; set; } // ID-ul pacientului care primește mesajul (dacă este pacient)
        public Patient? ReceiverPatient { get; set; } // Relație many-to-one cu Patient
    }
}
