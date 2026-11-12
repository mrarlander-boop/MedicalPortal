using System.Security.Claims;
using MedicalPortal.Data;
using MedicalPortal.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace MedicalPortal.Controllers
{
    [Authorize]
    public class CabinetController : Controller
    {
        private readonly MedicalDbContext _context;

        public CabinetController(MedicalDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index(string? statusFilter, string? diagnosisFilter, string? patientSearch)
        {
            var doctor = await GetCurrentDoctorAsync();
            if (doctor == null)
            {
                return RedirectToAction("Login", "Account");
            }

            IQueryable<Appointment> query = _context.Appointments
                .Include(a => a.Patient)
                .Include(a => a.Doctor)
                .Where(a => a.DoctorId == doctor.Id);

            if (!string.IsNullOrWhiteSpace(statusFilter))
            {
                query = query.Where(a => a.Status == statusFilter);
            }

            var appointments = await query.OrderBy(a => a.AppointmentDate).ToListAsync();

            IEnumerable<Appointment> filtered = appointments;

            if (!string.IsNullOrWhiteSpace(diagnosisFilter))
            {
                filtered = filtered.Where(a =>
                    (!string.IsNullOrWhiteSpace(a.AssignedDiagnosis) &&
                     a.AssignedDiagnosis.Contains(diagnosisFilter, StringComparison.OrdinalIgnoreCase)) ||
                    (a.Patient != null &&
                     a.Patient.Diagnosis.Contains(diagnosisFilter, StringComparison.OrdinalIgnoreCase)));
            }

            if (!string.IsNullOrWhiteSpace(patientSearch))
            {
                filtered = filtered.Where(a =>
                    a.Patient != null &&
                    a.Patient.Name.Contains(patientSearch, StringComparison.OrdinalIgnoreCase));
            }

            var model = new DoctorCabinetViewModel
            {
                Doctor = doctor,
                StatusFilter = statusFilter,
                DiagnosisFilter = diagnosisFilter,
                PatientSearch = patientSearch,
                Appointments = filtered.ToList()
            };

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdateDefaultDiagnosis(string defaultDiagnosis)
        {
            var doctor = await GetCurrentDoctorAsync();
            if (doctor == null)
            {
                return RedirectToAction("Login", "Account");
            }

            if (string.IsNullOrWhiteSpace(defaultDiagnosis))
            {
                TempData["ErrorMessage"] = "Диагноз не может быть пустым.";
                return RedirectToAction(nameof(Index));
            }

            doctor.DefaultDiagnosis = defaultDiagnosis.Trim();
            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = $"Ваш профильный диагноз обновлён: «{doctor.DefaultDiagnosis}».";
            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ConductReception(int appointmentId, string? diagnosis)
        {
            var doctor = await GetCurrentDoctorAsync();
            if (doctor == null)
            {
                return RedirectToAction("Login", "Account");
            }

            var appointment = await _context.Appointments
                .Include(a => a.Patient)
                .FirstOrDefaultAsync(a => a.Id == appointmentId && a.DoctorId == doctor.Id);

            if (appointment == null)
            {
                return NotFound();
            }

            if (appointment.Status == "Отменён")
            {
                TempData["ErrorMessage"] = "Нельзя провести отменённый приём.";
                return RedirectToAction(nameof(Index));
            }

            string finalDiagnosis = string.IsNullOrWhiteSpace(diagnosis)
                ? doctor.DefaultDiagnosis
                : diagnosis.Trim();

            string receptionMessage = doctor.Reception();
            appointment.ReceptionReport = receptionMessage;
            appointment.AssignedDiagnosis = finalDiagnosis;
            appointment.Status = "Завершён";

            if (appointment.Patient != null)
            {
                appointment.Patient.Diagnosis = finalDiagnosis;
            }

            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = $"Приём завершён! Вывод врача: «{receptionMessage}». Пациенту поставлен диагноз: «{finalDiagnosis}».";
            return RedirectToAction(nameof(Index));
        }

        private async Task<Doctor?> GetCurrentDoctorAsync()
        {
            var idClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (!int.TryParse(idClaim, out int doctorId))
            {
                return null;
            }

            return await _context.Doctors.FirstOrDefaultAsync(d => d.Id == doctorId);
        }
    }
}
