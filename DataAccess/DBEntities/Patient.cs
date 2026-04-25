using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Numerics;
using System.Text;

namespace DataAccess.DBEntities
{
    public class Patient
    {
        [Key]
        public string Id { get; set; } = Guid.NewGuid().ToString();

        // User account is optional (created only if email/password provided)
        public string? UserId { get; set; }

        [ForeignKey(nameof(UserId))]
        public User? User { get; set; }

        [Required, StringLength(50)]
        public string FirstName { get; set; } = string.Empty;

        [Required, StringLength(50)]
        public string LastName { get; set; } = string.Empty;

        public DateTime DateOfBirth { get; set; }

        [StringLength(10)]
        public string Gender { get; set; } = string.Empty;

        [StringLength(20)]
        public string Phone { get; set; } = string.Empty;

        [StringLength(255)]
        public string Address { get; set; } = string.Empty;

        [StringLength(5)]
        public string BloodGroup { get; set; } = string.Empty;

        public DateTime CreatedAt { get; set; } = DateTime.Now;

        // Vitals
        [StringLength(20)]
        public string? Weight { get; set; }

        [StringLength(20)]
        public string? Height { get; set; }

        [StringLength(50)]
        public string? BloodPressure { get; set; }

        [StringLength(50)]
        public string? Sugar { get; set; }

        

        // Normalized name for duplicate detection (lowercase, diacritics removed)
        [StringLength(200)]
        public string? NormalizedName { get; set; }

        // Navigation
        public ICollection<Appointment>? Appointments { get; set; }
        public ICollection<MedicalRecord>? MedicalRecords { get; set; }
        public ICollection<Prescription>? Prescriptions { get; set; }
    }
}
