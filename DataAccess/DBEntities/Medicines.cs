using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace DataAccess.DBEntities
{
    public class Medicines
    {
        [Key]
        public string Id { get; set; } = Guid.NewGuid().ToString();
        public string? Name { get; set; }
        public string? Dosage { get; set; }
        public int Frequency { get; set; }
        public string? MealTiming { get; set; }
        public string? Duration { get; set; }
        public DateTime? CreatedAt { get; set; }
    }
}
