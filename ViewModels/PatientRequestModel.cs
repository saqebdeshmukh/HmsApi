using System;
using System.Collections.Generic;
using System.Text;

namespace ViewModels
{
    public class PatientRequestModel
    {
        public string? UserId { get; set; }
        public string FirstName { get; set; } = string.Empty;
        public string LastName { get; set; } = string.Empty;
        public DateTime DateOfBirth { get; set; }
        public string Gender { get; set; } = string.Empty;
        public string Phone { get; set; } = string.Empty;
        public string Address { get; set; } = string.Empty;
        public string BloodGroup { get; set; } = string.Empty;
        public string? Weight { get; set; }
        public string? Height { get; set; }
        public string? SystolicBp { get; set; }
        public string? DiastolicBp { get; set; }
        public string? SugarAfterMeal { get; set; }
        public string? SugarBeforeMeal { get; set; }
        public string? DoctorId { get; set; }
        public bool BookAppointment { get; set; } = false;
    }
    public class PatientViewModel
    {
        public string? Id { get; set; }
        public string? UserId { get; set; }
        public string FirstName { get; set; } = string.Empty;
        public string LastName { get; set; } = string.Empty;
        public DateTime DateOfBirth { get; set; }
        public DateTime CreatedAt { get; set; }
        public string Gender { get; set; } = string.Empty;
        public string Phone { get; set; } = string.Empty;
        public string Address { get; set; } = string.Empty;
        public string BloodGroup { get; set; } = string.Empty;
        public string? Weight { get; set; }
        public string? Height { get; set; }
        public string? SystolicBp { get; set; }
        public string? DiastolicBp { get; set; }
        public string? SugarAfterMeal { get; set; }
        public string? SugarBeforeMeal { get; set; }
        public string? DoctorId { get; set; }
        public string? DoctorName { get; set; }
    }
    public class PatientDetailsViewModel
    {
        public string? Id { get; set; }
        public string PatientName { get; set; }
        public string DoctorName { get; set; }
        public DateTime DateOfBirth { get; set; }
        public DateTime CreatedAt { get; set; }
        public string? Weight { get; set; }
        public string? Height { get; set; }
        public string? SystolicBp { get; set; }
        public string? DiastolicBp { get; set; }
        public string? SugarAfterMeal { get; set; }
        public string? SugarBeforeMeal { get; set; }
        public string Gender { get; set; } = string.Empty;
        public string Phone { get; set; } = string.Empty;
        public string Address { get; set; } = string.Empty;
        public string BloodGroup { get; set; } = string.Empty;
        public List<PrintViewModel> Prescriptions { get; set; }
        public List<AppointmentDto> Appointments { get; set; }
        public List<MedicalRecordResponseModel> MedicalRecords { get; set; }
    }
}
