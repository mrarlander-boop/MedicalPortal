using System.ComponentModel.DataAnnotations;

namespace MedicalPortal.Models
{
    public class Patient
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "Введите имя пациента")]
        [Display(Name = "Имя пациента")]
        public string Name { get; set; } = string.Empty;

        [Range(0, 130, ErrorMessage = "Возраст должен быть от 0 до 130")]
        [Display(Name = "Возраст")]
        public int Age { get; set; }

        [Display(Name = "Диагноз")]
        public string Diagnosis { get; set; } = "На обследовании";

        public List<Appointment> Appointments { get; set; } = new();

        public Patient() { }

        public Patient(string name, int age, string diagnosis)
        {
            Name = name;
            Age = age;
            Diagnosis = diagnosis;
        }

        public string ShowPatientInfo() =>
            $"Пациент: {Name}, Возраст: {Age} лет, Диагноз: {Diagnosis}";
    }
}
