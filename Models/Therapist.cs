namespace MedicalPortal.Models
{
    public class Therapist : Doctor
    {
        public Therapist() { }

        public Therapist(string name, int experience) : base(name, experience)
        {
            DefaultDiagnosis = "ОРВИ";
        }

        public Therapist(string name, int experience, string defaultDiagnosis, string login, string passwordHash)
            : base(name, experience, defaultDiagnosis, login, passwordHash)
        {
        }

        public override string Specialization => "Терапевт";

        public override string Reception() => "Терапевт проводит общий осмотр пациента";
    }
}
