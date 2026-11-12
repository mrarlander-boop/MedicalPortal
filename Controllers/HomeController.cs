using System.Diagnostics;
using MedicalPortal.Data;
using MedicalPortal.Models;
using MedicalPortal.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace MedicalPortal.Controllers
{
    public class HomeController : Controller
    {
        private readonly MedicalDbContext _context;

        public HomeController(MedicalDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            var doctors = await _context.Doctors.OrderBy(d => d.Id).ToListAsync();
            var appointments = await _context.Appointments
                .Include(a => a.Doctor)
                .Include(a => a.Patient)
                .OrderByDescending(a => a.AppointmentDate)
                .ToListAsync();
            var patientsCount = await _context.Patients.CountAsync();

            var showcaseDoctors = DoctorFactory.Specializations
                .Select(spec => doctors.FirstOrDefault(d => d.Specialization == spec.Label))
                .Where(d => d != null)
                .Cast<Doctor>()
                .ToList();

            var sampleNeurologist = doctors.OfType<Neurologist>().FirstOrDefault()
                                    ?? new Neurologist("Морозов Дмитрий Игоревич", 11);
            var samplePatient = await _context.Patients.OrderBy(p => p.Id).FirstOrDefaultAsync()
                                ?? new Patient("Учебный Пациент", 30, "Мигрень");
            var sampleAppointment = appointments.FirstOrDefault()
                                    ?? new Appointment(samplePatient, sampleNeurologist, DateTime.Now);

            var model = new HomeDashboardViewModel
            {
                DoctorsCount = doctors.Count,
                PatientsCount = patientsCount,
                AppointmentsCount = appointments.Count,
                CompletedAppointmentsCount = appointments.Count(a => a.Status == "Завершён"),
                ShowcaseDoctors = showcaseDoctors,
                RecentAppointments = appointments.Take(5).ToList(),
                SamplePatient = samplePatient,
                SampleAppointment = sampleAppointment
            };

            return View(model);
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
