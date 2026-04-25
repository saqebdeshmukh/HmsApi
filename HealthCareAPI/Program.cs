using BusinessLogic.AppointmentService;
using BusinessLogic.DashboardService;
using BusinessLogic.DoctorService;
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
using Microsoft.IdentityModel.Tokens;
using System.Text;
//using FluentValidation.DependencyInjectionExtensions;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();
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
builder.Services.AddScoped<IDoctorService, DoctorService>();
builder.Services.AddScoped<IAppointmentService, AppointmentService>();
builder.Services.AddScoped<IPrescriptionService, PrescriptionService>();
builder.Services.AddScoped<IReceptionistService, ReceptionistService>();
builder.Services.AddScoped<IMedicalRecordService, MedicalRecordService>();
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
                CreatedAt = DateTime.Now
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
                CreatedAt = DateTime.Now
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
                CreatedAt = DateTime.Now
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
    app.MapOpenApi();
}

app.UseHttpsRedirection();
app.UseCors("AllowAll");
app.UseAuthentication();

app.UseAuthorization();

app.MapControllers();

app.Run();
