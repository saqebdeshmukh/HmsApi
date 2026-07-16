using System;
using System.Collections.Generic;
using System.Text;
using ViewModels;

namespace BusinessLogic.AppointmentService
{
    public interface IAppointmentService
    {
        Task<ResponseObj<string>> CreateAppointment(AppointmentRequestModel appointmentRequestModel);
        Task<PagedResponse<AppointmentDto>> GetAppointments(PaginationRequest request);
        Task<ResponseObj<List<AppointmentDto>>> GetTodayAppointments();
        Task<ResponseObj<List<AppointmentDto>>> GetUpcomingAppointments();
        Task<ResponseObj<List<AppointmentDto>>> GetCompletedAppointments();
        Task<ResponseObj<AppointmentDetailsViewModel>> AppointmentDetails(string appointmentId);
        Task<ResponseObj<List<AppointmentDto>>> GetAppointmentsByPatientId(string patientId);
        Task<ResponseObj<string>> UpdateAppointmentStatus(string appointmentId, AppointmentStatus status);
        Task<ResponseObj<string>> DeleteAppointment(string appointmentId);
        Task<ResponseObj<List<AppointmentViewModel>>> Calendar(int? Month, int? Year, string role, string userId);
    }
}
