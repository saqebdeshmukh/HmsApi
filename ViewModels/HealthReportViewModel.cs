using System;
using System.Collections.Generic;
using System.Text;

namespace ViewModels
{
    public class HealthReportViewModel
    {
        public string Id { get; set; }
        public string? Weight { get; set; }
        public string? PatientName { get; set; }
        public string? Height { get; set; }
        public string? BP { get; set; }
        public string? SugarAfterMeal { get; set; }
        public string? SugarBeforeMeal { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}
