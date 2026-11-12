using MedicalPortal.Data;
using MedicalPortal.Models;
using MedicalPortal.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace MedicalPortal.Controllers
{
    public class AppointmentsController : Controller
    {
        private readonly MedicalDbContext _context;

        public AppointmentsController(MedicalDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index(
            int? doctorId,
            string? specialization,
            string? diagnosis,
            string? status,
            string? patientSearch,
            DateTime? dateFrom,
            DateTime? dateTo)
        {
            var doctors = await _context.Doctors.OrderBy(d => d.Name).ToListAsync();

            var availableDiagnoses = await _context.Appointments
                .Include(a => a.Patient)
                .Select(a => !string.IsNullOrWhiteSpace(a.AssignedDiagnosis) ? a.AssignedDiagnosis : (a.Patient != null ? a.Patient.Diagnosis : ""))
                .Where(d => !string.IsNullOrWhiteSpace(d))
                .Distinct()
                .OrderBy(d => d)
                .ToListAsync();

            IQueryable<Appointment> query = _context.Appointments
                .Include(a => a.Doctor)
                .Include(a => a.Patient);

            if (doctorId.HasValue)
            {
                query = query.Where(a => a.DoctorId == doctorId.Value);
            }

            var discriminatorKey = DoctorFactory.GetDiscriminatorKey(specialization);
            if (!string.IsNullOrEmpty(discriminatorKey))
            {
                query = query.Where(a => a.Doctor != null && EF.Property<string>(a.Doctor, "DoctorType") == discriminatorKey);
            }

            if (!string.IsNullOrWhiteSpace(status))
            {
                query = query.Where(a => a.Status == status);
            }

            if (dateFrom.HasValue)
            {
                var start = dateFrom.Value.Date;
                query = query.Where(a => a.AppointmentDate >= start);
            }

            if (dateTo.HasValue)
            {
                var endExclusive = dateTo.Value.Date.AddDays(1);
                query = query.Where(a => a.AppointmentDate < endExclusive);
            }

            var appointments = await query.OrderByDescending(a => a.AppointmentDate).ToListAsync();

            IEnumerable<Appointment> filtered = appointments;

            if (!string.IsNullOrWhiteSpace(diagnosis))
            {
                filtered = filtered.Where(a =>
                    (!string.IsNullOrWhiteSpace(a.AssignedDiagnosis) &&
                     a.AssignedDiagnosis.Contains(diagnosis, StringComparison.OrdinalIgnoreCase)) ||
                    (a.Patient != null &&
                     a.Patient.Diagnosis.Contains(diagnosis, StringComparison.OrdinalIgnoreCase)));
            }

            if (!string.IsNullOrWhiteSpace(patientSearch))
            {
                filtered = filtered.Where(a =>
                    a.Patient != null &&
                    a.Patient.Name.Contains(patientSearch, StringComparison.OrdinalIgnoreCase));
            }

            var model = new AppointmentsFilterViewModel
            {
                DoctorId = doctorId,
                Specialization = specialization,
                Diagnosis = diagnosis,
                Status = status,
                PatientSearch = patientSearch,
                DateFrom = dateFrom,
                DateTo = dateTo,
                Appointments = filtered.ToList(),
                Doctors = doctors,
                AvailableDiagnoses = availableDiagnoses
            };

            return View(model);
        }

        [HttpGet]
        public async Task<IActionResult> Create(int? doctorId = null)
        {
            var model = new CreateAppointmentViewModel
            {
                DoctorId = doctorId,
                Patients = await _context.Patients.OrderBy(p => p.Name).ToListAsync(),
                Doctors = await _context.Doctors.OrderBy(d => d.Name).ToListAsync()
            };

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(CreateAppointmentViewModel model)
        {
            model.Patients = await _context.Patients.OrderBy(p => p.Name).ToListAsync();
            model.Doctors = await _context.Doctors.OrderBy(d => d.Name).ToListAsync();

            Doctor? doctor = model.DoctorId.HasValue
                ? await _context.Doctors.FirstOrDefaultAsync(d => d.Id == model.DoctorId.Value)
                : null;
            if (model.DoctorId.HasValue && doctor == null)
            {
                ModelState.AddModelError(nameof(model.DoctorId), "Выбранный врач не найден.");
            }

            if (model.AppointmentDate < DateTime.Now.AddMinutes(-5))
            {
                ModelState.AddModelError(nameof(model.AppointmentDate), "Нельзя записаться на прошедшие дату и время.");
            }
            else if (doctor != null)
            {
                bool hasConflict = await HasTimeConflictAsync(doctor.Id, model.AppointmentDate);
                if (hasConflict)
                {
                    ModelState.AddModelError(nameof(model.AppointmentDate), "На это время (или в пределах 30 минут) к выбранному врачу уже есть запись.");
                }
            }

            Patient? existingPatient = null;
            bool isNewPatient = string.Equals(model.PatientMode, "new", StringComparison.OrdinalIgnoreCase);

            if (isNewPatient)
            {
                if (string.IsNullOrWhiteSpace(model.NewPatientName))
                {
                    ModelState.AddModelError(nameof(model.NewPatientName), "Введите ФИО нового пациента.");
                }
                if (!model.NewPatientAge.HasValue)
                {
                    ModelState.AddModelError(nameof(model.NewPatientAge), "Укажите возраст нового пациента.");
                }
            }
            else
            {
                if (!model.PatientId.HasValue)
                {
                    ModelState.AddModelError(nameof(model.PatientId), "Выберите существующего пациента.");
                }
                else
                {
                    existingPatient = await _context.Patients.FirstOrDefaultAsync(p => p.Id == model.PatientId.Value);
                    if (existingPatient == null)
                    {
                        ModelState.AddModelError(nameof(model.PatientId), "Выбранный пациент не найден.");
                    }
                }
            }

            if (!ModelState.IsValid || doctor == null)
            {
                return View(model);
            }

            if (isNewPatient)
            {
                existingPatient = new Patient(model.NewPatientName!.Trim(), model.NewPatientAge!.Value, "На обследовании");
                _context.Patients.Add(existingPatient);
                await _context.SaveChangesAsync();
            }

            var appointment = new Appointment(existingPatient!, doctor, model.AppointmentDate);
            _context.Appointments.Add(appointment);
            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = $"Запись создана! {appointment.ShowAppointment()}";
            return RedirectToAction(nameof(Index));
        }

        [HttpGet]
        public async Task<IActionResult> Edit(int id)
        {
            var appointment = await _context.Appointments.FirstOrDefaultAsync(a => a.Id == id);
            if (appointment == null)
            {
                return NotFound();
            }

            var model = new EditAppointmentViewModel
            {
                Id = appointment.Id,
                DoctorId = appointment.DoctorId,
                PatientId = appointment.PatientId,
                AppointmentDate = appointment.AppointmentDate,
                Status = appointment.Status,
                Doctors = await _context.Doctors.OrderBy(d => d.Name).ToListAsync(),
                Patients = await _context.Patients.OrderBy(p => p.Name).ToListAsync()
            };

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, EditAppointmentViewModel model)
        {
            if (id != model.Id)
            {
                return BadRequest();
            }

            model.Doctors = await _context.Doctors.OrderBy(d => d.Name).ToListAsync();
            model.Patients = await _context.Patients.OrderBy(p => p.Name).ToListAsync();

            var existing = await _context.Appointments.FirstOrDefaultAsync(a => a.Id == id);
            if (existing == null)
            {
                return NotFound();
            }

            if (model.DoctorId.HasValue && model.Status != "Отменён")
            {
                bool hasConflict = await HasTimeConflictAsync(model.DoctorId.Value, model.AppointmentDate, id);
                if (hasConflict)
                {
                    ModelState.AddModelError(nameof(model.AppointmentDate), "На это время (или в пределах 30 минут) к выбранному врачу уже есть другая запись.");
                }
            }

            if (!ModelState.IsValid)
            {
                return View(model);
            }

            existing.DoctorId = model.DoctorId!.Value;
            existing.PatientId = model.PatientId!.Value;
            existing.AppointmentDate = model.AppointmentDate;
            existing.Status = model.Status;
            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = $"Запись №{existing.Id} обновлена.";
            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Cancel(int id)
        {
            var appointment = await _context.Appointments.FirstOrDefaultAsync(a => a.Id == id);
            if (appointment == null)
            {
                return NotFound();
            }

            appointment.Status = "Отменён";
            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = $"Запись №{appointment.Id} отменена.";
            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            var appointment = await _context.Appointments.FirstOrDefaultAsync(a => a.Id == id);
            if (appointment == null)
            {
                return NotFound();
            }

            _context.Appointments.Remove(appointment);
            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = $"Запись №{id} удалена.";
            return RedirectToAction(nameof(Index));
        }

        private async Task<bool> HasTimeConflictAsync(int doctorId, DateTime targetDate, int? excludeAppointmentId = null)
        {
            var windowStart = targetDate.AddMinutes(-29);
            var windowEnd = targetDate.AddMinutes(29);

            return await _context.Appointments.AnyAsync(a =>
                a.DoctorId == doctorId &&
                a.Status != "Отменён" &&
                (!excludeAppointmentId.HasValue || a.Id != excludeAppointmentId.Value) &&
                a.AppointmentDate >= windowStart &&
                a.AppointmentDate <= windowEnd);
        }
    }
}
