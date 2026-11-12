using MedicalPortal.Data;
using MedicalPortal.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace MedicalPortal.Controllers
{
    public class PatientsController : Controller
    {
        private readonly MedicalDbContext _context;

        public PatientsController(MedicalDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index(string? diagnosis, string? search, int? minAge, int? maxAge)
        {
            var availableDiagnoses = await _context.Patients
                .Select(p => p.Diagnosis)
                .Where(d => !string.IsNullOrWhiteSpace(d))
                .Distinct()
                .OrderBy(d => d)
                .ToListAsync();

            IQueryable<Patient> query = _context.Patients.Include(p => p.Appointments);

            if (minAge.HasValue)
            {
                query = query.Where(p => p.Age >= minAge.Value);
            }

            if (maxAge.HasValue)
            {
                query = query.Where(p => p.Age <= maxAge.Value);
            }

            var patients = await query.OrderBy(p => p.Name).ToListAsync();

            IEnumerable<Patient> filtered = patients;

            if (!string.IsNullOrWhiteSpace(diagnosis))
            {
                filtered = filtered.Where(p => p.Diagnosis.Contains(diagnosis, StringComparison.OrdinalIgnoreCase));
            }

            if (!string.IsNullOrWhiteSpace(search))
            {
                filtered = filtered.Where(p =>
                    p.Name.Contains(search, StringComparison.OrdinalIgnoreCase) ||
                    p.Diagnosis.Contains(search, StringComparison.OrdinalIgnoreCase));
            }

            var model = new PatientsFilterViewModel
            {
                Diagnosis = diagnosis,
                Search = search,
                MinAge = minAge,
                MaxAge = maxAge,
                Patients = filtered.ToList(),
                AvailableDiagnoses = availableDiagnoses
            };

            return View(model);
        }

        [HttpGet]
        public async Task<IActionResult> Details(int id)
        {
            var patient = await _context.Patients.FirstOrDefaultAsync(p => p.Id == id);
            if (patient == null)
            {
                return NotFound();
            }

            var appointments = await _context.Appointments
                .Include(a => a.Doctor)
                .Where(a => a.PatientId == id)
                .OrderByDescending(a => a.AppointmentDate)
                .ToListAsync();

            return View(new PatientDetailsViewModel
            {
                Patient = patient,
                Appointments = appointments
            });
        }

        [HttpGet]
        public IActionResult Create()
        {
            return View(new Patient());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Patient patient)
        {
            if (string.IsNullOrWhiteSpace(patient.Diagnosis))
            {
                patient.Diagnosis = "На обследовании";
            }

            if (!ModelState.IsValid)
            {
                return View(patient);
            }

            _context.Patients.Add(patient);
            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = $"Пациент добавлен: {patient.ShowPatientInfo()}";
            return RedirectToAction(nameof(Index));
        }

        [HttpGet]
        public async Task<IActionResult> Edit(int id)
        {
            var patient = await _context.Patients.FirstOrDefaultAsync(p => p.Id == id);
            if (patient == null)
            {
                return NotFound();
            }

            return View(patient);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, Patient patient)
        {
            if (id != patient.Id)
            {
                return BadRequest();
            }

            if (string.IsNullOrWhiteSpace(patient.Diagnosis))
            {
                patient.Diagnosis = "На обследовании";
            }

            if (!ModelState.IsValid)
            {
                return View(patient);
            }

            var existing = await _context.Patients.FirstOrDefaultAsync(p => p.Id == id);
            if (existing == null)
            {
                return NotFound();
            }

            existing.Name = patient.Name.Trim();
            existing.Age = patient.Age;
            existing.Diagnosis = patient.Diagnosis.Trim();
            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = $"Данные пациента «{existing.Name}» обновлены.";
            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            var patient = await _context.Patients.FirstOrDefaultAsync(p => p.Id == id);
            if (patient == null)
            {
                return NotFound();
            }

            _context.Patients.Remove(patient);
            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = $"Пациент «{patient.Name}» удалён.";
            return RedirectToAction(nameof(Index));
        }
    }
}
