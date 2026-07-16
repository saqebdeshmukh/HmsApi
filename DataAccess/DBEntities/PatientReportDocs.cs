using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace DataAccess.DBEntities
{
    public class PatientReportDocs
    {
        [Key]
        public string Id { get; set; } = Guid.NewGuid().ToString();
        public string PatientId { get; set; }
        [ForeignKey(nameof(PatientId))]
        public Patient Patient { get; set; }
        //public string? DocName { get; set; }
        public string? DocAddress { get; set; }
        public string? DocType { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}
