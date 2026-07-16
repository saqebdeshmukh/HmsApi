using System;
using System.Collections.Generic;
using System.Text;
using ViewModels;

namespace BusinessLogic.DashboardService
{
    public interface IDashboardService
    {
        Task<ResponseObj<DashboardViewModel>> AdminDashboard();
        Task<ResponseObj<DoctorDashboardModel>> GetDoctorDashboard(string userId);
        Task<ResponseObj<ReceptionistDashboardModel>> GetReceptionistDashboard(string userId);
        Task<ResponseObj<RedirectToContinueModel>> GetIdToRedirect(string appointmentId);
    }
}
