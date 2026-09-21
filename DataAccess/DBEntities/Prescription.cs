using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace DataAccess.DBEntities
{
    public class Prescription
    {
        [Key]
        public string Id { get; set; } = Guid.NewGuid().ToString();

        // MedicalRecordId may be null for receptionist-created internal prescriptions
        public string? MedicalRecordId { get; set; }

        [ForeignKey(nameof(MedicalRecordId))]
        public MedicalRecord? MedicalRecord { get; set; }

        [Required]
        public string PatientId { get; set; } = string.Empty;

        [ForeignKey(nameof(PatientId))]
        public Patient Patient { get; set; } = null!;
        public string DoctorId { get; set; } = string.Empty;

        [ForeignKey(nameof(DoctorId))]
        public Doctor Doctor { get; set; } = null!;

        [Required, StringLength(100)]
        public string MedicineName { get; set; } = string.Empty;

        [StringLength(100)]
        public string Dosage { get; set; } = string.Empty;

        [StringLength(50)]
        public string Duration { get; set; } = string.Empty;
        public string Instructions { get; set; } = string.Empty;
        public bool IsGiven { get; set; } = false;
        public DateTime PrescriptionDate { get; set; } = DateTime.UtcNow;
        // Type: "MedicalStore" (printed by doctor) or "Internal" (admin/receptionist dispensed)
        public string Type { get; set; } = "MedicalStore";

        // One-to-many: PrescriptionItems
        public ICollection<PrescriptionItem>? Items { get; set; }
    }
}
