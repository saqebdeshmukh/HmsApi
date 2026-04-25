using System;
using System.Collections.Generic;
using System.Text;
using ViewModels;

namespace BusinessLogic.AppointmentService
{
    public interface IAppointmentService
    {
        Task<ResponseObj<string>> CreateAppointment(AppointmentRequestModel appointmentRequestModel);
        Task<ResponseObj<DoctorDetailAppointmentsViewModel>> GetAppointments();
        Task<ResponseObj<AppointmentDetailsViewModel>> AppointmentDetails(string appointmentId);
        Task<ResponseObj<string>> UpdateAppointmentStatus(string appointmentId, string status);
        Task<ResponseObj<string>> DeleteAppointment(string appointmentId);
    }
}
