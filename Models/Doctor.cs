using System.ComponentModel.DataAnnotations;

namespace MedicalPortal.Models
{
    public abstract class Doctor
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "Введите имя врача")]
        [Display(Name = "Имя врача")]
        public string Name { get; set; } = string.Empty;

        [Range(0, 70, ErrorMessage = "Стаж должен быть от 0 до 70 лет")]
        [Display(Name = "Стаж (лет)")]
        public int Experience { get; set; }

        [Display(Name = "Специальность")]
        public abstract string Specialization { get; }

        [Required(ErrorMessage = "Укажите профильный диагноз врача")]
        [Display(Name = "Диагноз, который ставит врач")]
        public string DefaultDiagnosis { get; set; } = string.Empty;

        [Required(ErrorMessage = "Введите логин")]
        [Display(Name = "Логин аккаунта")]
        public string Login { get; set; } = string.Empty;

        [Required]
        public string PasswordHash { get; set; } = string.Empty;

        public List<Appointment> Appointments { get; set; } = new();

        protected Doctor() { }

        protected Doctor(string name, int experience)
        {
            Name = name;
            Experience = experience;
        }

        protected Doctor(string name, int experience, string defaultDiagnosis, string login, string passwordHash)
        {
            Name = name;
            Experience = experience;
            DefaultDiagnosis = defaultDiagnosis;
            Login = login;
            PasswordHash = passwordHash;
        }

        public abstract string Reception();
    }
}
