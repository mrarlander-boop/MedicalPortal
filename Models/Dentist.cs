namespace MedicalPortal.Models
{
    public class Dentist : Doctor
    {
        public Dentist() { }

        public Dentist(string name, int experience) : base(name, experience)
        {
            DefaultDiagnosis = "Кариес";
        }

        public Dentist(string name, int experience, string defaultDiagnosis, string login, string passwordHash)
            : base(name, experience, defaultDiagnosis, login, passwordHash)
        {
        }

        public override string Specialization => "Стоматолог";

        public override string Reception() => "Стоматолог проводит осмотр полости рта";
    }
}
