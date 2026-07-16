using System;
using System.Collections.Generic;
using System.Text;

namespace ViewModels
{
    public class MedicinesRequestModel
    {
        public MedicinesDto? Medicine { get; set; }
        public string? ExcelBase64 { get; set; }
        public string? ExcelFileName { get; set; }
    }

    public class MedicinesDto
    {
        public string? Id { get; set; }
        public string? Name { get; set; }
        public string? Dosage { get; set; }
        public int Frequency { get; set; }
        public string? MealTiming { get; set; }
        public string? Duration { get; set; }
        public DateTime? CreatedAt { get; set; }
    }
}
