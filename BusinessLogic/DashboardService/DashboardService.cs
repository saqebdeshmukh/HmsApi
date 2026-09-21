using Azure;
using BusinessLogic.CommonService;
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
        private readonly ICommonService _commonService;
        public DashboardService(IUnitOfWork unitOfWork, ICommonService commonService)
        {
            _unitOfWork = unitOfWork;
            _commonService = commonService;
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
            .CountAsync(a => (a.Status != AppointmentStatus.Completed || a.Status != AppointmentStatus.Canceled) && a.AppointmentDate == _commonService.GetCurrentIndianTime().Date);

                dashboard.CompletedAppointments = await _unitOfWork.AppointmentRepository
            .GetWithoutTracking()
            .CountAsync(a => a.Status == AppointmentStatus.Completed && a.AppointmentDate == _commonService.GetCurrentIndianTime().Date);

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

            var today = _commonService.GetCurrentIndianTime().Date;

            var dashboard = new DoctorDashboardModel
            {
                DoctorName = $"Dr. {doctor.FirstName} {doctor.LastName}",

                TodayAppointments = await _unitOfWork.AppointmentRepository
                    .GetWithoutTracking()
                    //.Include(a => a.Patient)
                    .Where(a => a.DoctorId == doctor.Id && a.AppointmentDate.Date == today)
                    .OrderBy(a => a.AppointmentDate)
                    .Take(10)
                    .Select(a => new AppointmentDto
                    {
                        Id = a.Id,
                        AppointmentDate = a.AppointmentDate,
                        PatientName = a.Patient.FirstName + " " + a.Patient.LastName,
                        Status = a.Status
                    })
                    .ToListAsync(),

                UpcomingAppointments = await _unitOfWork.AppointmentRepository
                    .GetWithoutTracking()
                    //.Include(a => a.Patient)
                    .Where(a => a.DoctorId == doctor.Id &&
                                a.AppointmentDate > _commonService.GetCurrentIndianTime() &&
                                (a.Status == AppointmentStatus.Scheduled || a.Status == AppointmentStatus.MedicalRecordCreated))
                    .OrderBy(a => a.AppointmentDate)
                    .Take(10)
                    .Select(a => new AppointmentDto
                    {
                        Id = a.Id,
                        AppointmentDate = a.AppointmentDate,
                        PatientName = a.Patient.FirstName + " " + a.Patient.LastName,
                        Status = a.Status
                    })
                    .ToListAsync(),

                TotalPatientsSeenToday = await _unitOfWork.AppointmentRepository
                    .GetWithoutTracking()
                    .Where(a => a.DoctorId == doctor.Id &&
                                a.Status == AppointmentStatus.Completed &&
                                a.AppointmentDate >= today &&
                                a.AppointmentDate <= _commonService.GetCurrentIndianTime())
                    .Select(a => a.PatientId)
                    .Distinct()
                    .CountAsync(),

                TotalAppointments = await _unitOfWork.AppointmentRepository
                    .GetWithoutTracking()
                    .CountAsync(a => a.DoctorId == doctor.Id && a.AppointmentDate.Date == today),

                CompletedAppointments = await _unitOfWork.AppointmentRepository
                    .GetWithoutTracking()
                    .CountAsync(a => a.DoctorId == doctor.Id && a.Status == AppointmentStatus.Completed && a.AppointmentDate.Date == today),

                PendingAppointments = await _unitOfWork.AppointmentRepository
                    .GetWithoutTracking()
                    .CountAsync(a => a.DoctorId == doctor.Id && a.AppointmentDate == today && (a.Status == AppointmentStatus.Scheduled || a.Status == AppointmentStatus.MedicalRecordCreated))
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

            var today = _commonService.GetCurrentIndianTime().Date;

            var dashboard = new ReceptionistDashboardModel
            {
                ReceptionistName = $"{receptionist.FirstName} {receptionist.LastName}",

                TotalPatients = await _unitOfWork.PatientRepository
                    .GetWithoutTracking()
                    .CountAsync(),

                TotalAppointments = await _unitOfWork.AppointmentRepository
                    .GetWithoutTracking()
                    .CountAsync(a => a.AppointmentDate.Date == today),

                PendingAppointments = await _unitOfWork.AppointmentRepository
                    .GetWithoutTracking()
                    .CountAsync(a => (a.Status == AppointmentStatus.Scheduled || a.Status == AppointmentStatus.MedicalRecordCreated) &&
                                     a.AppointmentDate == today),

                CompletedAppointments = await _unitOfWork.AppointmentRepository
                    .GetWithoutTracking()
                    .CountAsync(a => a.Status == AppointmentStatus.Completed && a.AppointmentDate.Date == today),

                TodayAppointments = await _unitOfWork.AppointmentRepository
                    .GetWithoutTracking()
                    //.Include(a => a.Patient)
                    .Where(a => a.AppointmentDate.Date == today &&
                                a.Status == AppointmentStatus.Scheduled)
                    .OrderBy(a => a.AppointmentDate)
                    .Select(a => new AppointmentDto
                    {
                        Id=a.Id,
                        AppointmentDate = a.AppointmentDate,
                        PatientName = a.Patient.FirstName + " " + a.Patient.LastName,
                        Status = a.Status
                    })
                    .ToListAsync(),

                TotalPatientsSeenToday = await _unitOfWork.AppointmentRepository
                    .GetWithoutTracking()
                    .Where(a => a.Status == AppointmentStatus.Completed &&
                                a.AppointmentDate.Date == today)
                    .Select(a => a.PatientId)
                    .Distinct()
                    .CountAsync(),

                PatientsSeenToday = await _unitOfWork.AppointmentRepository
                    .GetWithoutTracking()
                    //.Include(a => a.Patient)
                    .Where(a => a.Status == AppointmentStatus.Completed &&
                                a.AppointmentDate.Date == today)
                    .Select(a => new AppointmentDto
                    {
                        Id = a.Id,
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
        public async Task<ResponseObj<RedirectToContinueModel>> GetIdToRedirect(string appointmentId)
        {
            var response = new ResponseObj<RedirectToContinueModel>();
            var appointment = await _unitOfWork.AppointmentRepository
                .GetWithoutTracking()
                .FirstOrDefaultAsync(a => a.Id == appointmentId);
            if (appointment == null)
            {
                response.IsSuccess = false;
                response.Message = "Appointment not found";
                return response;
            }
            RedirectToContinueModel redirectToContinueModel = new RedirectToContinueModel();
            redirectToContinueModel.Id = appointmentId;
            redirectToContinueModel.Status = appointment.Status;
            if (appointment.Status == AppointmentStatus.MedicalRecordCreated)
            {
                var medicalRecord = await _unitOfWork.MedicalRecordRepository
                    .GetWithoutTracking().OrderByDescending(mr => mr.CreatedAt)
                    .FirstOrDefaultAsync(mr => mr.AppointmentId == appointmentId);
                if(medicalRecord != null)
                {
                    redirectToContinueModel.Id = medicalRecord.Id;
                }
            }

            response.IsSuccess = true;
            response.Message = "Redirect ID retrieved successfully";
            response.Data = redirectToContinueModel;
            return response;
        }
        public async Task<ResponseObj<List<DashboardPrescriptionViewModel>>> GetInternalPrescriptionsforDashboard()
        {
            ResponseObj<List<DashboardPrescriptionViewModel>> responseObj = new ResponseObj<List<DashboardPrescriptionViewModel>>();
            var prescriptions = await _unitOfWork.PrescritpionRepository
                .GetWithoutTracking().Where(x=> x.IsGiven == false && x.Type == "Internal")
                .Take(5)
                .Select(p => new DashboardPrescriptionViewModel
                {
                    Id = p.Id,
                    PatientName = p.Patient.FirstName + " " + p.Patient.LastName,
                    PrescriptionDate = p.PrescriptionDate
                })
                .ToListAsync();
            responseObj.Data = prescriptions;
            responseObj.IsSuccess = true;
            return responseObj;
        }
    }
}
