using BusinessLogic.AppointmentService;
using BusinessLogic.CommonService;
using BusinessLogic.DashboardService;
using BusinessLogic.DoctorService;
using BusinessLogic.GeneticDiseaseService;
using BusinessLogic.MedicalRecordService;
using BusinessLogic.PatientService;
using BusinessLogic.PrescriptionService;
using BusinessLogic.ReceptionistService;
using BusinessLogic.TokenService;
using BusinessLogic.UnitOfWork;
using BusinessLogic.UserService;
using DataAccess.Context;
using DataAccess.DBEntities;
using FluentValidation;
using HealthCareAPI.ExceptionHandling;
using HealthCareAPI.FilterValidations;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.FileProviders;
using Microsoft.IdentityModel.Tokens;
using System.IO;
using System.Runtime.InteropServices;
using System.Security.AccessControl;
using System.Security.Principal;
using System.Text;
//using FluentValidation.DependencyInjectionExtensions;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
//builder.Services.AddOpenApi();
builder.Services.AddValidatorsFromAssemblyContaining<UserValidations>();

builder.Services.AddControllers(options =>
{
    options.Filters.Add<FluentValidationFilter>();
}); 
builder.Services.AddDbContext<ApplicationContext>(options =>
{
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection"));

});
builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();
builder.Services.AddScoped<ITokenService, TokenService>();
builder.Services.AddScoped<IUserService, UserService>();
builder.Services.AddScoped<IDashboardService, DashboardService>();
builder.Services.AddScoped<IPatientService, PatientService>();
builder.Services.AddScoped<IGeneticDiseaseService, GeneticDiseaseService>();
builder.Services.AddScoped<BusinessLogic.MedicinesService.IMedicinesService, BusinessLogic.MedicinesService.MedicinesService>();
builder.Services.AddScoped<IDoctorService, DoctorService>();
builder.Services.AddScoped<IAppointmentService, AppointmentService>();
builder.Services.AddScoped<IPrescriptionService, PrescriptionService>();
builder.Services.AddScoped<IReceptionistService, ReceptionistService>();
builder.Services.AddScoped<IMedicalRecordService, MedicalRecordService>();
builder.Services.AddScoped<ICommonService, CommonService>();
builder.Services.AddScoped<IPasswordHasher<User>, PasswordHasher<User>>();
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll",
        policy =>
        {
            policy.AllowAnyOrigin()
                  .AllowAnyHeader()
                  .AllowAnyMethod();
        });
});


builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
.AddJwtBearer(options =>
{
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,

        ValidIssuer = builder.Configuration["Jwt:Issuer"],
        ValidAudience = builder.Configuration["Jwt:Audience"],
        IssuerSigningKey = new SymmetricSecurityKey(
            Encoding.UTF8.GetBytes(builder.Configuration["Jwt:Key"])
        )
    };
});
var app = builder.Build();
app.UseMiddleware<ExceptionMiddleware>();
// Ensure PatientFiles folder exists and has required permissions after publish
var patientFilesFolder = Path.Combine(Directory.GetCurrentDirectory(), "PatientFiles");
if (!Directory.Exists(patientFilesFolder))
{
    Directory.CreateDirectory(patientFilesFolder);
}
if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
{
    try
    {
        var dirInfo = new DirectoryInfo(patientFilesFolder);
        var dirSecurity = dirInfo.GetAccessControl();
        // Grant Modify access to Everyone so the app can read/write files when published.
        var everyone = new SecurityIdentifier(WellKnownSidType.WorldSid, null);
        var rule = new FileSystemAccessRule(everyone, FileSystemRights.Modify | FileSystemRights.ReadAndExecute | FileSystemRights.Write, InheritanceFlags.ContainerInherit | InheritanceFlags.ObjectInherit, PropagationFlags.None, AccessControlType.Allow);
        dirSecurity.AddAccessRule(rule);
        dirInfo.SetAccessControl(dirSecurity);
    }
    catch (Exception ex)
    {
        var logger = app.Services.GetRequiredService<ILogger<Program>>();
        logger.LogWarning(ex, "Unable to set folder permissions for PatientFiles.");
    }
}

app.UseStaticFiles(new StaticFileOptions
{
    FileProvider = new PhysicalFileProvider(patientFilesFolder),
    RequestPath = "/PatientFiles"
});
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;

    try
    {
        var context = services.GetRequiredService<ApplicationContext>();
        var hasher = services.GetRequiredService<IPasswordHasher<User>>();

        await context.Database.MigrateAsync();

        if (!context.Users.Any(u => u.Role == "Admin"))
        {
            var adminUser = new User
            {
                Id = Guid.NewGuid().ToString(),
                Email = "admin@healthcare.com",
                Role = "Admin",
                CreatedAt = DateTime.UtcNow
            };

            adminUser.Password = hasher.HashPassword(adminUser, "Admin@123");

            context.Users.Add(adminUser);
            await context.SaveChangesAsync();
        }

        if (!context.Doctors.Any())
        {
            var doctorUser = new User
            {
                Id = Guid.NewGuid().ToString(),
                Email = "doctor@healthcare.com",
                Role = "Doctor",
                CreatedAt = DateTime.UtcNow
            };

            doctorUser.Password = hasher.HashPassword(doctorUser, "Doctor@123");

            context.Users.Add(doctorUser);
            await context.SaveChangesAsync();

            var doctor = new Doctor
            {
                Id = Guid.NewGuid().ToString(),
                UserId = doctorUser.Id,
                FirstName = "Doctor",
                LastName = "1",
                Specialization = "General Practice",
                Phone = "+20 100 000 0001",
                Email = "doctor@healthcare.com",
                CreatedAt = DateTime.UtcNow
            };

            context.Doctors.Add(doctor);
            await context.SaveChangesAsync();
        }
    }
    catch (Exception ex)
    {
        var logger = services.GetRequiredService<ILogger<Program>>();
        logger.LogError(ex, "Seeding failed");
    }
}
// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    //app.MapOpenApi();
}

app.UseHttpsRedirection();
app.UseCors("AllowAll");
app.UseAuthentication();

app.UseAuthorization();

app.MapControllers();

app.Run();
