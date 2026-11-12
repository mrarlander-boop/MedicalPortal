using System.ComponentModel.DataAnnotations;

namespace MedicalPortal.Models
{
    public class HomeDashboardViewModel
    {
        public int DoctorsCount { get; set; }
        public int PatientsCount { get; set; }
        public int AppointmentsCount { get; set; }
        public int CompletedAppointmentsCount { get; set; }
        public List<Doctor> ShowcaseDoctors { get; set; } = new();
        public List<Appointment> RecentAppointments { get; set; } = new();
        public Patient SamplePatient { get; set; } = new();
        public Appointment SampleAppointment { get; set; } = new();
    }

    public class DoctorsFilterViewModel
    {
        public string? Specialization { get; set; }
        public string? Diagnosis { get; set; }
        public int? MinExperience { get; set; }
        public string? Search { get; set; }
        public List<Doctor> Doctors { get; set; } = new();
        public List<string> AvailableDiagnoses { get; set; } = new();
    }

    public class PatientsFilterViewModel
    {
        public string? Diagnosis { get; set; }
        public string? Search { get; set; }
        public int? MinAge { get; set; }
        public int? MaxAge { get; set; }
        public List<Patient> Patients { get; set; } = new();
        public List<string> AvailableDiagnoses { get; set; } = new();
    }

    public class PatientDetailsViewModel
    {
        public Patient Patient { get; set; } = null!;
        public List<Appointment> Appointments { get; set; } = new();
    }

    public class AppointmentsFilterViewModel
    {
        public int? DoctorId { get; set; }
        public string? Specialization { get; set; }
        public string? Diagnosis { get; set; }
        public string? Status { get; set; }
        public string? PatientSearch { get; set; }
        public DateTime? DateFrom { get; set; }
        public DateTime? DateTo { get; set; }
        public List<Appointment> Appointments { get; set; } = new();
        public List<Doctor> Doctors { get; set; } = new();
        public List<string> AvailableDiagnoses { get; set; } = new();
    }

    public class CreateAppointmentViewModel
    {
        public string PatientMode { get; set; } = "existing";

        [Display(Name = "Пациент")]
        public int? PatientId { get; set; }

        [Display(Name = "ФИО нового пациента")]
        public string? NewPatientName { get; set; }

        [Range(0, 130, ErrorMessage = "Возраст должен быть от 0 до 130")]
        [Display(Name = "Возраст")]
        public int? NewPatientAge { get; set; }

        [Required(ErrorMessage = "Выберите врача")]
        [Display(Name = "Врач")]
        public int? DoctorId { get; set; }

        [Required(ErrorMessage = "Укажите дату и время приёма")]
        [Display(Name = "Дата и время приёма")]
        public DateTime AppointmentDate { get; set; } = DateTime.Today.AddDays(1).AddHours(10);

        public List<Patient> Patients { get; set; } = new();
        public List<Doctor> Doctors { get; set; } = new();
    }

    public class EditAppointmentViewModel
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "Выберите врача")]
        [Display(Name = "Врач")]
        public int? DoctorId { get; set; }

        [Required(ErrorMessage = "Выберите пациента")]
        [Display(Name = "Пациент")]
        public int? PatientId { get; set; }

        [Required(ErrorMessage = "Укажите дату и время приёма")]
        [Display(Name = "Дата и время приёма")]
        public DateTime AppointmentDate { get; set; }

        [Display(Name = "Статус")]
        public string Status { get; set; } = "Запланирован";

        public List<Patient> Patients { get; set; } = new();
        public List<Doctor> Doctors { get; set; } = new();
    }

    public class LoginViewModel
    {
        [Required(ErrorMessage = "Введите логин")]
        [Display(Name = "Логин")]
        public string Login { get; set; } = string.Empty;

        [Required(ErrorMessage = "Введите пароль")]
        [DataType(DataType.Password)]
        [Display(Name = "Пароль")]
        public string Password { get; set; } = string.Empty;

        public string? ReturnUrl { get; set; }
        public List<Doctor> DemoDoctors { get; set; } = new();
    }

    public class RegisterDoctorViewModel
    {
        [Required(ErrorMessage = "Выберите специальность")]
        [Display(Name = "Специальность")]
        public string SpecializationKey { get; set; } = "Neurologist";

        [Required(ErrorMessage = "Введите имя врача")]
        [Display(Name = "ФИО врача")]
        public string Name { get; set; } = string.Empty;

        [Required(ErrorMessage = "Укажите стаж врача")]
        [Range(0, 70, ErrorMessage = "Стаж должен быть от 0 до 70 лет")]
        [Display(Name = "Стаж (лет)")]
        public int Experience { get; set; } = 5;

        [Required(ErrorMessage = "Укажите диагноз, который ставит врач")]
        [Display(Name = "Профильный диагноз врача")]
        public string DefaultDiagnosis { get; set; } = string.Empty;

        [Required(ErrorMessage = "Придумайте логин")]
        [Display(Name = "Логин аккаунта")]
        public string Login { get; set; } = string.Empty;

        [Required(ErrorMessage = "Введите пароль")]
        [MinLength(4, ErrorMessage = "Пароль должен содержать минимум 4 символа")]
        [DataType(DataType.Password)]
        [Display(Name = "Пароль")]
        public string Password { get; set; } = string.Empty;

        [Required(ErrorMessage = "Повторите пароль")]
        [Compare(nameof(Password), ErrorMessage = "Пароли не совпадают")]
        [DataType(DataType.Password)]
        [Display(Name = "Подтверждение пароля")]
        public string ConfirmPassword { get; set; } = string.Empty;
    }

    public class DoctorCabinetViewModel
    {
        public Doctor Doctor { get; set; } = null!;
        public string? StatusFilter { get; set; }
        public string? DiagnosisFilter { get; set; }
        public string? PatientSearch { get; set; }
        public List<Appointment> Appointments { get; set; } = new();
    }
}
