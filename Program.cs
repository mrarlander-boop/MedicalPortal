using System.Text.Encodings.Web;
using System.Text.Unicode;
using MedicalPortal.Data;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

builder.Services.Configure<Microsoft.Extensions.WebEncoders.WebEncoderOptions>(options =>
{
    options.TextEncoderSettings = new TextEncoderSettings(UnicodeRanges.All);
});

builder.Services.AddControllersWithViews(options =>
{
    var provider = options.ModelBindingMessageProvider;
    provider.SetValueIsInvalidAccessor(value => $"Недопустимое значение «{value}».");
    provider.SetValueMustBeANumberAccessor(name => $"Поле «{name}» должно быть числом.");
    provider.SetMissingBindRequiredValueAccessor(name => $"Поле «{name}» обязательно для заполнения.");
    provider.SetAttemptedValueIsInvalidAccessor((value, name) => $"Значение «{value}» недопустимо для поля «{name}».");
    provider.SetNonPropertyAttemptedValueIsInvalidAccessor(value => $"Недопустимое значение «{value}».");
    provider.SetValueMustNotBeNullAccessor(name => $"Поле «{name}» обязательно для заполнения.");
});

builder.Services.AddDbContext<MedicalDbContext>(options =>
    options.UseSqlite(builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options =>
    {
        options.LoginPath = "/Account/Login";
        options.LogoutPath = "/Account/Logout";
        options.AccessDeniedPath = "/Account/Login";
    });

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var dbContext = scope.ServiceProvider.GetRequiredService<MedicalDbContext>();
    DbInitializer.Initialize(dbContext);
}

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
    app.UseHttpsRedirection();
}

app.UseStaticFiles();

app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();
