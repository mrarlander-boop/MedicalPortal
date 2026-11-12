using System.ComponentModel.DataAnnotations;

namespace MedicalPortal.Models
{
    public class Appointment
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "Выберите пациента")]
        [Display(Name = "Пациент")]
        public int PatientId { get; set; }
        public Patient? Patient { get; set; }

        [Required(ErrorMessage = "Выберите врача")]
        [Display(Name = "Врач")]
        public int DoctorId { get; set; }
        public Doctor? Doctor { get; set; }

        [Required(ErrorMessage = "Укажите дату приёма")]
        [Display(Name = "Дата приёма")]
        public DateTime AppointmentDate { get; set; } = DateTime.Today.AddHours(10);

        [Display(Name = "Поставленный диагноз")]
        public string AssignedDiagnosis { get; set; } = string.Empty;

        [Display(Name = "Статус приёма")]
        public string Status { get; set; } = "Запланирован";

        [Display(Name = "Результат приёма")]
        public string ReceptionReport { get; set; } = string.Empty;

        public Appointment() { }

        public Appointment(Patient patient, Doctor doctor, DateTime appointmentDate)
        {
            Patient = patient;
            PatientId = patient.Id;
            Doctor = doctor;
            DoctorId = doctor.Id;
            AppointmentDate = appointmentDate;
        }

        public string ShowAppointment()
        {
            string patientName = Patient?.Name ?? "Не указан";
            string doctorName = Doctor?.Name ?? "Не указан";
            string spec = Doctor?.Specialization ?? "Не указана";
            string diag = !string.IsNullOrWhiteSpace(AssignedDiagnosis)
                ? AssignedDiagnosis
                : (Patient?.Diagnosis ?? "Не поставлен");

            return $"Запись №{Id}: {AppointmentDate:dd.MM.yyyy HH:mm} | Врач: {doctorName} ({spec}) | Пациент: {patientName} | Диагноз: {diag} | Статус: {Status}";
        }
    }
}
