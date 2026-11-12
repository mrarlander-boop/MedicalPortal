using MedicalPortal.Models;
using MedicalPortal.Services;

namespace MedicalPortal.Data
{
    public static class DbInitializer
    {
        public static void Initialize(MedicalDbContext context)
        {
            context.Database.EnsureCreated();

            if (context.Doctors.Any())
            {
                return;
            }

            var passwordHash = PasswordHelper.HashPassword("1234");

            var therapist = new Therapist(
                "Смирнова Елена Викторовна",
                12,
                "ОРВИ",
                "therapist",
                passwordHash);

            var cardiologist = new Cardiologist(
                "Козлов Андрей Сергеевич",
                15,
                "Артериальная гипертензия",
                "cardiologist",
                passwordHash);

            var dentist = new Dentist(
                "Волкова Ольга Дмитриевна",
                9,
                "Кариес",
                "dentist",
                passwordHash);

            var neurologist = new Neurologist(
                "Морозов Дмитрий Игоревич",
                11,
                "Мигрень",
                "neurologist",
                passwordHash);

            var neurologist2 = new Neurologist(
                "Орлова Наталья Павловна",
                7,
                "Остеохондроз",
                "neurologist2",
                passwordHash);

            context.Doctors.AddRange(therapist, cardiologist, dentist, neurologist, neurologist2);
            context.SaveChanges();

            var patient1 = new Patient("Иванов Иван Петрович", 34, "ОРВИ");
            var patient2 = new Patient("Петрова Анна Сергеевна", 48, "Артериальная гипертензия");
            var patient3 = new Patient("Сидоров Максим Алексеевич", 27, "Кариес");
            var patient4 = new Patient("Кузнецова Мария Игоревна", 31, "Мигрень");
            var patient5 = new Patient("Николаев Артём Владимирович", 42, "На обследовании");

            context.Patients.AddRange(patient1, patient2, patient3, patient4, patient5);
            context.SaveChanges();

            var today = DateTime.Today;
            var appointments = new List<Appointment>
            {
                new Appointment(patient1, therapist, today.AddHours(9).AddMinutes(30))
                {
                    AssignedDiagnosis = therapist.DefaultDiagnosis,
                    Status = "Завершён",
                    ReceptionReport = therapist.Reception()
                },
                new Appointment(patient2, cardiologist, today.AddHours(11))
                {
                    AssignedDiagnosis = cardiologist.DefaultDiagnosis,
                    Status = "Завершён",
                    ReceptionReport = cardiologist.Reception()
                },
                new Appointment(patient3, dentist, today.AddHours(13))
                {
                    AssignedDiagnosis = dentist.DefaultDiagnosis,
                    Status = "Запланирован",
                    ReceptionReport = string.Empty
                },
                new Appointment(patient4, neurologist, today.AddDays(1).AddHours(10))
                {
                    AssignedDiagnosis = neurologist.DefaultDiagnosis,
                    Status = "Завершён",
                    ReceptionReport = neurologist.Reception()
                },
                new Appointment(patient5, neurologist2, today.AddDays(1).AddHours(14).AddMinutes(30))
                {
                    AssignedDiagnosis = string.Empty,
                    Status = "Запланирован",
                    ReceptionReport = string.Empty
                }
            };

            context.Appointments.AddRange(appointments);
            context.SaveChanges();
        }
    }
}
