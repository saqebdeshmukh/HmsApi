using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace DataAccess.DBEntities
{
    public class PrescriptionItem
    {
        [Key]
        public string Id { get; set; } = Guid.NewGuid().ToString();

        [Required]
        public string PrescriptionId { get; set; } = string.Empty;

        [ForeignKey(nameof(PrescriptionId))]
        public Prescription? Prescription { get; set; } = null!;

        [Required, StringLength(200)]
        public string MedicineName { get; set; } = string.Empty;

        [StringLength(200)]
        public string? Dosage { get; set; } = string.Empty;
        public int Frequency { get; set; }
        [StringLength(100)]
        public string? MealTiming { get; set; }

        [StringLength(100)]
        public string? Duration { get; set; } = string.Empty;
    }
}
