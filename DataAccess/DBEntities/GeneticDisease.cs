using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace DataAccess.DBEntities
{
    public class GeneticDisease
    {
        [Key]
        public int Id { get; set; }
        public string? DiseaseName { get; set; }
        public string? ColorCode { get; set; }
    }
}
