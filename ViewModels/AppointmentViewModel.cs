using System;
using System.Collections.Generic;
using System.Text;

namespace ViewModels
{
    public class AppointmentViewModel
    {
        public string Id { get; set; }
        public string PatientId { get; set; } = string.Empty;
        public string DoctorId { get; set; } = string.Empty;
        public string DoctorName { get; set; } = string.Empty;
        public string PatientName { get; set; } = string.Empty;
        public DateTime AppointmentDate { get; set; }
        public string Status { get; set; } = string.Empty;
        public string Reason { get; set; } = string.Empty;
        public string Notes { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; } = DateTime.Now;
    }
    public class AppointmentRequestModel
    {
        public string PatientId { get; set; } = string.Empty;
        public string DoctorId { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
        public string Reason { get; set; } = string.Empty;
        public string Notes { get; set; } = string.Empty;
    }
    public class DoctorDetailAppointmentsViewModel
    {
        public List<AppointmentDto> Appointments { get; set; }
        public List<AppointmentDto> TodayAppointments { get; set; }
        public List<AppointmentDto> CompletedAppointments { get; set; }
        public List<AppointmentDto> UpcomingAppointments { get; set; }
    }
    public class AppointmentDetailsViewModel
    {
        public string Id { get; set; }
        public string DoctorId { get; set; }
        public string DoctorName { get; set; }
        public string DoctorSpecialization { get; set; }
        public string DoctorPhone { get; set; }
        public string DoctorEmail { get; set; }
        public double ConsultationFee { get; set; }
        public string PatientId { get; set; }
        public string PatientName { get; set; }
        public string PatientPhone { get; set; }
        public string PatientGender { get; set; }
        public string BloodGroup { get; set; }
        public string? Weight { get; set; }
        public string? Height { get; set; }
        public string? SystolicBp { get; set; }
        public string? DiastolicBp { get; set; }
        public string? SugarAfterMeal { get; set; }
        public string? SugarBeforeMeal { get; set; }
        public int Age { get; set; }
        public DateTime AppointmentDate { get; set; }
        public string Status { get; set; }
        public DateTime CreatedAt { get; set; }
       

    }
}
