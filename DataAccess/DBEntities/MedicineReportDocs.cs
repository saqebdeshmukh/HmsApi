using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace DataAccess.DBEntities
{
    public class MedicineReportDocs
    {
        [Key]
        public string Id { get; set; } = Guid.NewGuid().ToString();
        public string MedicineId { get; set; } = string.Empty;
        [ForeignKey(nameof(MedicineId))]
        public Medicines Medicine { get; set; } = null!;
        public string? DocAddress { get; set; }
        public string? DocType { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}
