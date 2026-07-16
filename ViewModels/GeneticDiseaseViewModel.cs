using System;
using System.Collections.Generic;
using System.Text;

namespace ViewModels
{
    public class GeneticDiseaseViewModel
    {
        public int? GeneticDiseaseId { get; set; }
        public string? DiseaseName { get; set; }
        public string? Medicines { get; set; }
        public string? ColorCode { get; set; }
    }
}
