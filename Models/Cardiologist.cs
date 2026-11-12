namespace MedicalPortal.Models
{
    public class Cardiologist : Doctor
    {
        public Cardiologist() { }

        public Cardiologist(string name, int experience) : base(name, experience)
        {
            DefaultDiagnosis = "Артериальная гипертензия";
        }

        public Cardiologist(string name, int experience, string defaultDiagnosis, string login, string passwordHash)
            : base(name, experience, defaultDiagnosis, login, passwordHash)
        {
        }

        public override string Specialization => "Кардиолог";

        public override string Reception() => "Кардиолог проверяет работу сердца и ЭКГ";
    }
}
