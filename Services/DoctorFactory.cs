using MedicalPortal.Models;

namespace MedicalPortal.Services
{
    public static class DoctorFactory
    {
        public static readonly IReadOnlyList<(string Key, string Label, string SuggestedDiagnosis, string BadgeCss)> Specializations =
        [
            ("Therapist", "Терапевт", "ОРВИ", "spec-therapist"),
            ("Cardiologist", "Кардиолог", "Артериальная гипертензия", "spec-cardiologist"),
            ("Dentist", "Стоматолог", "Кариес", "spec-dentist"),
            ("Neurologist", "Невролог", "Мигрень", "spec-neurologist")
        ];

        public static string GetBadgeCssClass(string? specialization)
        {
            var match = Specializations.FirstOrDefault(s =>
                s.Label.Equals(specialization, StringComparison.OrdinalIgnoreCase) ||
                s.Key.Equals(specialization, StringComparison.OrdinalIgnoreCase));

            return string.IsNullOrEmpty(match.BadgeCss) ? "spec-therapist" : match.BadgeCss;
        }

        public static string? GetDiscriminatorKey(string? specializationLabel)
        {
            if (string.IsNullOrWhiteSpace(specializationLabel))
            {
                return null;
            }

            var match = Specializations.FirstOrDefault(s =>
                s.Label.Equals(specializationLabel, StringComparison.OrdinalIgnoreCase) ||
                s.Key.Equals(specializationLabel, StringComparison.OrdinalIgnoreCase));

            return string.IsNullOrEmpty(match.Key) ? null : match.Key;
        }

        public static Doctor Create(
            string specializationKey,
            string name,
            int experience,
            string defaultDiagnosis,
            string login,
            string passwordHash)
        {
            return specializationKey switch
            {
                "Therapist" or "Терапевт" => new Therapist(name, experience, defaultDiagnosis, login, passwordHash),
                "Cardiologist" or "Кардиолог" => new Cardiologist(name, experience, defaultDiagnosis, login, passwordHash),
                "Dentist" or "Стоматолог" => new Dentist(name, experience, defaultDiagnosis, login, passwordHash),
                "Neurologist" or "Невролог" => new Neurologist(name, experience, defaultDiagnosis, login, passwordHash),
                _ => throw new ArgumentException($"Неизвестная специальность врача: {specializationKey}", nameof(specializationKey))
            };
        }
    }
}
