using MedicalPortal.Models;
using Microsoft.EntityFrameworkCore;

namespace MedicalPortal.Data
{
    public class MedicalDbContext : DbContext
    {
        public MedicalDbContext(DbContextOptions<MedicalDbContext> options) : base(options)
        {
        }

        public DbSet<Doctor> Doctors => Set<Doctor>();
        public DbSet<Therapist> Therapists => Set<Therapist>();
        public DbSet<Cardiologist> Cardiologists => Set<Cardiologist>();
        public DbSet<Dentist> Dentists => Set<Dentist>();
        public DbSet<Neurologist> Neurologists => Set<Neurologist>();
        public DbSet<Patient> Patients => Set<Patient>();
        public DbSet<Appointment> Appointments => Set<Appointment>();

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<Doctor>(entity =>
            {
                entity.HasDiscriminator<string>("DoctorType")
                    .HasValue<Therapist>("Therapist")
                    .HasValue<Cardiologist>("Cardiologist")
                    .HasValue<Dentist>("Dentist")
                    .HasValue<Neurologist>("Neurologist");

                entity.HasIndex(d => d.Login).IsUnique();
                entity.Ignore(d => d.Specialization);
            });

            modelBuilder.Entity<Appointment>(entity =>
            {
                entity.HasOne(a => a.Patient)
                    .WithMany(p => p.Appointments)
                    .HasForeignKey(a => a.PatientId)
                    .OnDelete(DeleteBehavior.Cascade);

                entity.HasOne(a => a.Doctor)
                    .WithMany(d => d.Appointments)
                    .HasForeignKey(a => a.DoctorId)
                    .OnDelete(DeleteBehavior.Cascade);
            });
        }
    }
}
