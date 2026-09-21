using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace DataAccess.DBEntities
{
    public class HealthReport
    {
        [Key]
        public string Id { get; set; } = Guid.NewGuid().ToString();
        public string? PatientId { get; set; }
        [ForeignKey("PatientId")]
        public Patient? Patient { get; set; }
        public string? MedicalRecordId { get; set; }
        [ForeignKey("MedicalRecordId")]
        public MedicalRecord? MedicalRecord { get; set; }
        public string? AppointmentId { get; set; }

        [ForeignKey("AppointmentId")]
        public Appointment? Appointment { get; set; }
        [StringLength(20)]
        public string? Weight { get; set; }

        [StringLength(20)]
        public string? Height { get; set; }

        [StringLength(50)]
        public string? BloodPressure { get; set; }

        [StringLength(50)]
        public string? SugarAfterMeal { get; set; }
        [StringLength(50)]
        public string? SugarBeforeMeal { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}
