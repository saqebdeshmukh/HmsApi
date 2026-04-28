using System;
using System.Collections.Generic;
using System.Text;

namespace ViewModels
{
    public class HealthReportViewModel
    {
        public string Id { get; set; }
        public string? Weight { get; set; }
        public string? Height { get; set; }
        public string? SystolicBp { get; set; }
        public string? DiastolicBp { get; set; }
        public string? SugarAfterMeal { get; set; }
        public string? SugarBeforeMeal { get; set; }
    }
}
