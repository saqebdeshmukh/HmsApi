using System;
using System.Collections.Generic;
using System.Text;

namespace ViewModels
{
    public class DashboardViewModel
    {
        public int TotalPatients { get; set; }
        public int TotalDoctors { get; set; }
        public int TotalAppointments { get; set; }
        public int PendingAppointments { get; set; }
        public int CompletedAppointments { get; set; }
        public int TotalMedicalRecords { get; set; }
        public List<AppointmentViewModel> RecentAppointments { get; set; }
    }
    public class DoctorDashboardModel
    {
        public string DoctorName { get; set; }

        public int TotalAppointments { get; set; }
        public int CompletedAppointments { get; set; }
        public int PendingAppointments { get; set; }
        public int TotalPatientsSeenToday { get; set; }

        public List<AppointmentDto> TodayAppointments { get; set; }
        public List<AppointmentDto> UpcomingAppointments { get; set; }
    }
    public class ReceptionistDashboardModel
    {
        public string ReceptionistName { get; set; }
        public int TotalPatients { get; set; }
        public int TotalAppointments { get; set; }
        public int PendingAppointments { get; set; }
        public int CompletedAppointments { get; set; }
        public int TotalPatientsSeenToday { get; set; }
        public List<AppointmentDto> TodayAppointments { get; set; }
        public List<AppointmentDto> PatientsSeenToday { get; set; }
    }
    public class AppointmentDto
    {
        public string Id { get; set; }
        public DateTime AppointmentDate { get; set; }
        public string PatientName { get; set; }
        public string Status { get; set; }
        public string Phone { get; set; }
    }
}
