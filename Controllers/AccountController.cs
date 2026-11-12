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
    public class AccountController : Controller
    {
        private static readonly string[] DemoLogins =
            ["therapist", "cardiologist", "dentist", "neurologist", "neurologist2"];

        private readonly MedicalDbContext _context;

        public AccountController(MedicalDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<IActionResult> Login(string? returnUrl = null)
        {
            return View(new LoginViewModel
            {
                ReturnUrl = returnUrl,
                DemoDoctors = await GetDemoDoctorsAsync()
            });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(LoginViewModel model)
        {
            model.DemoDoctors = await GetDemoDoctorsAsync();

            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var loginNormalized = model.Login.Trim();
            var doctor = await _context.Doctors.FirstOrDefaultAsync(d => d.Login == loginNormalized);
            if (doctor == null || !PasswordHelper.VerifyPassword(model.Password, doctor.PasswordHash))
            {
                ModelState.AddModelError(string.Empty, "Неверный логин или пароль.");
                return View(model);
            }

            await SignInDoctorAsync(doctor);

            if (!string.IsNullOrEmpty(model.ReturnUrl) && Url.IsLocalUrl(model.ReturnUrl))
            {
                return Redirect(model.ReturnUrl);
            }

            return RedirectToAction("Index", "Cabinet");
        }

        [HttpGet]
        public IActionResult RegisterDoctor()
        {
            return View(new RegisterDoctorViewModel());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> RegisterDoctor(RegisterDoctorViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var login = model.Login.Trim();
            bool loginExists = await _context.Doctors.AnyAsync(d => d.Login == login);
            if (loginExists)
            {
                ModelState.AddModelError(nameof(model.Login), "Врач с таким логином уже зарегистрирован.");
                return View(model);
            }

            var passwordHash = PasswordHelper.HashPassword(model.Password);
            var doctor = DoctorFactory.Create(
                model.SpecializationKey,
                model.Name.Trim(),
                model.Experience,
                model.DefaultDiagnosis.Trim(),
                login,
                passwordHash);

            _context.Doctors.Add(doctor);
            await _context.SaveChangesAsync();

            await SignInDoctorAsync(doctor);
            TempData["SuccessMessage"] = $"Создан врач {doctor.Name} ({doctor.Specialization}) с профильным диагнозом «{doctor.DefaultDiagnosis}».";
            return RedirectToAction("Index", "Cabinet");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Logout()
        {
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            return RedirectToAction("Index", "Home");
        }

        private async Task<List<Doctor>> GetDemoDoctorsAsync()
        {
            return await _context.Doctors
                .Where(d => DemoLogins.Contains(d.Login))
                .OrderBy(d => d.Id)
                .ToListAsync();
        }

        private async Task SignInDoctorAsync(Doctor doctor)
        {
            var claims = new List<Claim>
            {
                new(ClaimTypes.NameIdentifier, doctor.Id.ToString()),
                new(ClaimTypes.Name, doctor.Name),
                new("Login", doctor.Login),
                new("Specialization", doctor.Specialization)
            };

            var identity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
            var principal = new ClaimsPrincipal(identity);
            await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, principal);
        }
    }
}
