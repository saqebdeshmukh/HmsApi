using Azure;
using BusinessLogic.UnitOfWork;
using DataAccess.DBEntities;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Text;
using ViewModels;

namespace BusinessLogic.DashboardService
{
    public class DashboardService : IDashboardService
    {
        private readonly IUnitOfWork _unitOfWork;
        public DashboardService(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }
        public async Task<ResponseObj<DashboardViewModel>> AdminDashboard()
        {
            ResponseObj<DashboardViewModel> responseObj = new ResponseObj<DashboardViewModel>();
            var dashboard = new DashboardViewModel();
            
                dashboard.TotalPatients = await _unitOfWork.PatientRepository.GetWithoutTracking().CountAsync();
                dashboard.TotalDoctors = await _unitOfWork.DoctorRepository.GetWithoutTracking().CountAsync();
                dashboard.TotalAppointments = await _unitOfWork.AppointmentRepository.GetWithoutTracking().CountAsync();

                dashboard.PendingAppointments = await _unitOfWork.AppointmentRepository
            .GetWithoutTracking()
            .CountAsync(a => a.Status == "Scheduled");

                dashboard.CompletedAppointments = await _unitOfWork.AppointmentRepository
            .GetWithoutTracking()
            .CountAsync(a => a.Status == "Completed");

                dashboard.TotalMedicalRecords = await _unitOfWork.MedicalRecordRepository
            .GetWithoutTracking()
            .CountAsync();

                dashboard.RecentAppointments = await _unitOfWork.AppointmentRepository
            .GetWithoutTracking()
            //.Include(a => a.Patient)
            //.Include(a => a.Doctor)
            .OrderByDescending(a => a.AppointmentDate)
            .Take(5)
            .Select(a => new AppointmentViewModel
            {
                AppointmentDate = a.AppointmentDate,
                PatientName = a.Patient.FirstName + " " + a.Patient.LastName,
                DoctorName = a.Doctor.FirstName + " " + a.Doctor.LastName,
                Status = a.Status
            })
            .ToListAsync();
            

            responseObj.IsSuccess = true;
            responseObj.Message = "Dashboard data retrieved successfully";
            responseObj.Data = dashboard;
            return responseObj;
        }
        public async Task<ResponseObj<DoctorDashboardModel>> GetDoctorDashboard(string userId)
        {
            var response = new ResponseObj<DoctorDashboardModel>();

            var doctor = await _unitOfWork.DoctorRepository
                .GetWithoutTracking()
                .FirstOrDefaultAsync(d => d.UserId == userId);

            if (doctor == null)
            {
                response.IsSuccess = false;
                response.Message = "Doctor not found";
                return response;
            }

            var today = DateTime.Today;

            var dashboard = new DoctorDashboardModel
            {
                DoctorName = $"Dr. {doctor.FirstName} {doctor.LastName}",

                TodayAppointments = await _unitOfWork.AppointmentRepository
                    .GetWithoutTracking()
                    //.Include(a => a.Patient)
                    .Where(a => a.DoctorId == doctor.Id && a.AppointmentDate.Date == today)
                    .OrderBy(a => a.AppointmentDate)
                    .Select(a => new AppointmentDto
                    {
                        AppointmentDate = a.AppointmentDate,
                        PatientName = a.Patient.FirstName + " " + a.Patient.LastName,
                        Status = a.Status
                    })
                    .ToListAsync(),

                UpcomingAppointments = await _unitOfWork.AppointmentRepository
                    .GetWithoutTracking()
                    //.Include(a => a.Patient)
                    .Where(a => a.DoctorId == doctor.Id &&
                                a.AppointmentDate > DateTime.Now &&
                                a.Status == "Scheduled")
                    .OrderBy(a => a.AppointmentDate)
                    .Take(10)
                    .Select(a => new AppointmentDto
                    {
                        AppointmentDate = a.AppointmentDate,
                        PatientName = a.Patient.FirstName + " " + a.Patient.LastName,
                        Status = a.Status
                    })
                    .ToListAsync(),

                TotalPatientsSeenToday = await _unitOfWork.AppointmentRepository
                    .GetWithoutTracking()
                    .Where(a => a.DoctorId == doctor.Id &&
                                a.Status == "Completed" &&
                                a.AppointmentDate >= today &&
                                a.AppointmentDate <= DateTime.Now)
                    .Select(a => a.PatientId)
                    .Distinct()
                    .CountAsync(),

                TotalAppointments = await _unitOfWork.AppointmentRepository
                    .GetWithoutTracking()
                    .CountAsync(a => a.DoctorId == doctor.Id),

                CompletedAppointments = await _unitOfWork.AppointmentRepository
                    .GetWithoutTracking()
                    .CountAsync(a => a.DoctorId == doctor.Id && a.Status == "Completed"),

                PendingAppointments = await _unitOfWork.AppointmentRepository
                    .GetWithoutTracking()
                    .CountAsync(a => a.DoctorId == doctor.Id && a.Status == "Scheduled")
            };

            response.IsSuccess = true;
            response.Message = "Doctor dashboard retrieved successfully";
            response.Data = dashboard;

            return response;
        }
        public async Task<ResponseObj<ReceptionistDashboardModel>> GetReceptionistDashboard(string userId)
        {
            var response = new ResponseObj<ReceptionistDashboardModel>();

            var receptionist = await _unitOfWork.ReceptionistRepository
                .GetWithoutTracking()
                .FirstOrDefaultAsync(r => r.UserId == userId);

            if (receptionist == null)
            {
                response.IsSuccess = false;
                response.Message = "Receptionist not found";
                return response;
            }

            var today = DateTime.Today;

            var dashboard = new ReceptionistDashboardModel
            {
                ReceptionistName = $"{receptionist.FirstName} {receptionist.LastName}",

                TotalPatients = await _unitOfWork.PatientRepository
                    .GetWithoutTracking()
                    .CountAsync(),

                TotalAppointments = await _unitOfWork.AppointmentRepository
                    .GetWithoutTracking()
                    .CountAsync(),

                PendingAppointments = await _unitOfWork.AppointmentRepository
                    .GetWithoutTracking()
                    .CountAsync(a => a.Status == "Scheduled" &&
                                     a.AppointmentDate >= today &&
                                     a.AppointmentDate <= DateTime.Now),

                CompletedAppointments = await _unitOfWork.AppointmentRepository
                    .GetWithoutTracking()
                    .CountAsync(a => a.Status == "Completed"),

                TodayAppointments = await _unitOfWork.AppointmentRepository
                    .GetWithoutTracking()
                    //.Include(a => a.Patient)
                    .Where(a => a.AppointmentDate.Date == today &&
                                a.Status == "Scheduled")
                    .OrderBy(a => a.AppointmentDate)
                    .Select(a => new AppointmentDto
                    {
                        AppointmentDate = a.AppointmentDate,
                        PatientName = a.Patient.FirstName + " " + a.Patient.LastName,
                        Status = a.Status
                    })
                    .ToListAsync(),

                TotalPatientsSeenToday = await _unitOfWork.AppointmentRepository
                    .GetWithoutTracking()
                    .Where(a => a.Status == "Completed" &&
                                a.AppointmentDate >= today &&
                                a.AppointmentDate <= DateTime.Now)
                    .Select(a => a.PatientId)
                    .Distinct()
                    .CountAsync(),

                PatientsSeenToday = await _unitOfWork.AppointmentRepository
                    .GetWithoutTracking()
                    //.Include(a => a.Patient)
                    .Where(a => a.Status == "Completed" &&
                                a.AppointmentDate >= today &&
                                a.AppointmentDate <= DateTime.Now)
                    .Select(a => new AppointmentDto
                    {
                        AppointmentDate = a.AppointmentDate,
                        PatientName = a.Patient.FirstName + " " + a.Patient.LastName,
                        Status = a.Status
                    })
                    .ToListAsync()
            };

            response.IsSuccess = true;
            response.Message = "Receptionist dashboard retrieved successfully";
            response.Data = dashboard;

            return response;
        }
    }
}
