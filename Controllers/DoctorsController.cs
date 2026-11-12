using System.Security.Claims;
using MedicalPortal.Data;
using MedicalPortal.Models;
using MedicalPortal.Services;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace MedicalPortal.Controllers
{
    public class DoctorsController : Controller
    {
        private readonly MedicalDbContext _context;

        public DoctorsController(MedicalDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index(string? specialization, string? diagnosis, int? minExperience, string? search)
        {
            var availableDiagnoses = await _context.Doctors
                .Select(d => d.DefaultDiagnosis)
                .Where(d => !string.IsNullOrWhiteSpace(d))
                .Distinct()
                .OrderBy(d => d)
                .ToListAsync();

            IQueryable<Doctor> query = _context.Doctors;

            var discriminatorKey = DoctorFactory.GetDiscriminatorKey(specialization);
            if (!string.IsNullOrEmpty(discriminatorKey))
            {
                query = query.Where(d => EF.Property<string>(d, "DoctorType") == discriminatorKey);
            }

            if (minExperience.HasValue)
            {
                query = query.Where(d => d.Experience >= minExperience.Value);
            }

            var doctors = await query.OrderBy(d => d.Name).ToListAsync();

            IEnumerable<Doctor> filtered = doctors;
            if (!string.IsNullOrWhiteSpace(diagnosis))
            {
                filtered = filtered.Where(d => d.DefaultDiagnosis.Contains(diagnosis, StringComparison.OrdinalIgnoreCase));
            }

            if (!string.IsNullOrWhiteSpace(search))
            {
                filtered = filtered.Where(d =>
                    d.Name.Contains(search, StringComparison.OrdinalIgnoreCase) ||
                    d.DefaultDiagnosis.Contains(search, StringComparison.OrdinalIgnoreCase));
            }

            var model = new DoctorsFilterViewModel
            {
                Specialization = specialization,
                Diagnosis = diagnosis,
                MinExperience = minExperience,
                Search = search,
                Doctors = filtered.ToList(),
                AvailableDiagnoses = availableDiagnoses
            };

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            var doctor = await _context.Doctors.FirstOrDefaultAsync(d => d.Id == id);
            if (doctor == null)
            {
                return NotFound();
            }

            var currentId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            _context.Doctors.Remove(doctor);
            await _context.SaveChangesAsync();

            if (currentId == id.ToString())
            {
                await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            }

            TempData["SuccessMessage"] = $"Врач {doctor.Name} удалён.";
            return RedirectToAction(nameof(Index));
        }
    }
}
