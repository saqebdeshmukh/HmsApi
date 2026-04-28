using DataAccess.DBEntities;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;

namespace DataAccess.Context
{
    public class ApplicationContext : DbContext
    {
        public ApplicationContext(DbContextOptions<ApplicationContext> options) : base(options)
        {
        }
        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            if (!optionsBuilder.IsConfigured)
            {
                optionsBuilder.UseSqlServer(
                //"data source=localhost;Initial Catalog=HRModule;User ID=sa; Password=saJed1004;MultipleActiveResultSets=true;TrustServerCertificate=True");
                //"data source=localhost;Initial Catalog=WorkshopMgmtV2;User ID=sa; Password=A2zsa!@#4;MultipleActiveResultSets=true;TrustServerCertificate=True");
                "Server=(localdb)\\MSSQLLocalDB;Database=HealthcareNewDB;Trusted_Connection=True;MultipleActiveResultSets=true;TrustServerCertificate=True");
            }
        }
        public DbSet<User> Users { get; set; }
        public DbSet<Patient> Patients { get; set; }
        public DbSet<Doctor> Doctors { get; set; }
        public DbSet<Receptionist> Receptionists { get; set; }
        public DbSet<Appointment> Appointments { get; set; }
        public DbSet<MedicalRecord> MedicalRecords { get; set; }
        public DbSet<Prescription> Prescriptions { get; set; }
        public DbSet<PrescriptionItem> PrescriptionItems { get; set; }
        public DbSet<DoctorAvailabilitySlot> DoctorAvailabilitySlots { get; set; }
        public DbSet<RefreshToken> RefreshTokens { get; set; }
        public DbSet<HealthReport> HealthReports { get; set; }
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            ConfigureTables(modelBuilder);
            ConfigureUserRelations(modelBuilder);
            ConfigurePatientRelations(modelBuilder);
            ConfigureDoctorRelations(modelBuilder);
            ConfigureAppointmentRelations(modelBuilder);
            ConfigureMedicalRelations(modelBuilder);
            ConfigureIndexes(modelBuilder);
            ConfigureProperties(modelBuilder);
        }

        // =========================
        // TABLE NAMES
        // =========================
        private void ConfigureTables(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<User>().ToTable("Users");
            modelBuilder.Entity<Patient>().ToTable("Patients");
            modelBuilder.Entity<Doctor>().ToTable("Doctors");
            modelBuilder.Entity<Receptionist>().ToTable("Receptionists");
            modelBuilder.Entity<Appointment>().ToTable("Appointments");
            modelBuilder.Entity<MedicalRecord>().ToTable("MedicalRecords");
            modelBuilder.Entity<Prescription>().ToTable("Prescriptions");
            modelBuilder.Entity<PrescriptionItem>().ToTable("PrescriptionItems");
            modelBuilder.Entity<DoctorAvailabilitySlot>().ToTable("DoctorAvailabilitySlots");
        }

        // =========================
        // USER RELATIONS (1:1)
        // =========================
        private void ConfigureUserRelations(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<User>()
                .HasOne(u => u.Patient)
                .WithOne(p => p.User)
                .HasForeignKey<Patient>(p => p.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<User>()
                .HasOne(u => u.Doctor)
                .WithOne(d => d.User)
                .HasForeignKey<Doctor>(d => d.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<User>()
                .HasOne(u => u.Receptionist)
                .WithOne(r => r.User)
                .HasForeignKey<Receptionist>(r => r.UserId)
                .OnDelete(DeleteBehavior.Cascade);
        }

        // =========================
        // PATIENT RELATIONS
        // =========================
        private void ConfigurePatientRelations(ModelBuilder modelBuilder)
        {
           

            // Patient → Appointments
            modelBuilder.Entity<Patient>()
                .HasMany(p => p.Appointments)
                .WithOne(a => a.Patient)
                .HasForeignKey(a => a.PatientId)
                .OnDelete(DeleteBehavior.Restrict);

            // Patient → MedicalRecords
            modelBuilder.Entity<Patient>()
                .HasMany(p => p.MedicalRecords)
                .WithOne(m => m.Patient)
                .HasForeignKey(m => m.PatientId)
                .OnDelete(DeleteBehavior.Restrict);

            // Patient → Prescriptions
            modelBuilder.Entity<Patient>()
                .HasMany(p => p.Prescriptions)
                .WithOne(pr => pr.Patient)
                .HasForeignKey(pr => pr.PatientId)
                .OnDelete(DeleteBehavior.Restrict);
        }

        // =========================
        // DOCTOR RELATIONS
        // =========================
        private void ConfigureDoctorRelations(ModelBuilder modelBuilder)
        {
            // Doctor → Appointments
            modelBuilder.Entity<Doctor>()
                .HasMany(d => d.Appointments)
                .WithOne(a => a.Doctor)
                .HasForeignKey(a => a.DoctorId)
                .OnDelete(DeleteBehavior.Restrict);

            // Doctor → Availability Slots
            modelBuilder.Entity<Doctor>()
                .HasMany(d => d.AvailabilitySlots)
                .WithOne(s => s.Doctor)
                .HasForeignKey(s => s.DoctorId)
                .OnDelete(DeleteBehavior.Cascade);

            // Doctor → MedicalRecords
            modelBuilder.Entity<Doctor>()
                .HasMany(d => d.MedicalRecords)
                .WithOne(m => m.Doctor)
                .HasForeignKey(m => m.DoctorId)
                .OnDelete(DeleteBehavior.Restrict);

            // Doctor → Prescriptions
            modelBuilder.Entity<Doctor>()
                .HasMany(d => d.Prescriptions)
                .WithOne(p => p.Doctor)
                .HasForeignKey(p => p.DoctorId)
                .OnDelete(DeleteBehavior.Restrict);
        }

        // =========================
        // APPOINTMENT RELATIONS
        // =========================
        private void ConfigureAppointmentRelations(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Appointment>()
                .HasOne(a => a.MedicalRecord)
                .WithOne(m => m.Appointment)
                .HasForeignKey<MedicalRecord>(m => m.AppointmentId)
                .OnDelete(DeleteBehavior.Cascade);
        }

        // =========================
        // MEDICAL RELATIONS
        // =========================
        private void ConfigureMedicalRelations(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<MedicalRecord>()
                .HasMany(m => m.Prescriptions)
                .WithOne(p => p.MedicalRecord)
                .HasForeignKey(p => p.MedicalRecordId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<Prescription>()
                .HasMany(p => p.Items)
                .WithOne(i => i.Prescription)
                .HasForeignKey(i => i.PrescriptionId)
                .OnDelete(DeleteBehavior.Cascade);
        }

        // =========================
        // INDEXES & CONSTRAINTS
        // =========================
        private void ConfigureIndexes(ModelBuilder modelBuilder)
        {
            // Prevent duplicate patients
            modelBuilder.Entity<Patient>()
                .HasIndex(p => new { p.NormalizedName, p.DateOfBirth, p.Phone })
                .IsUnique()
                .HasDatabaseName("IX_Patients_NormalizedName_DOB_Phone");

            // Doctor availability uniqueness
            modelBuilder.Entity<DoctorAvailabilitySlot>()
                .HasIndex(s => new { s.DoctorId, s.StartDateTime })
                .IsUnique();
        }

        // =========================
        // PROPERTY CONFIGURATION
        // =========================
        private void ConfigureProperties(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Doctor>()
                .Property(d => d.ConsultationFee)
                .HasColumnType("decimal(18,2)");
        }        
    }
}
