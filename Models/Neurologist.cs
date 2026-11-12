namespace MedicalPortal.Models
{
    public class Neurologist : Doctor
    {
        public Neurologist() { }

        public Neurologist(string name, int experience) : base(name, experience)
        {
            DefaultDiagnosis = "Мигрень";
        }

        public Neurologist(string name, int experience, string defaultDiagnosis, string login, string passwordHash)
            : base(name, experience, defaultDiagnosis, login, passwordHash)
        {
        }

        public override string Specialization => "Невролог";

        public override string Reception() => "Невролог проводит проверку нервной системы";
    }
}
