using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace DataAccess.DBEntities
{
    public class PatientAllergy
    {
        [Key]
        public int Id { get; set; }
        public string? PatientId { get; set; }
        [ForeignKey("PatientId")]
        public Patient? Patient { get; set; }
        [Column(TypeName = "varchar(500)")]
        public string AllergyName { get; set; } = string.Empty;
    }
}
