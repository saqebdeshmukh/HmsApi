using BusinessLogic.CommonService;
using BusinessLogic.UnitOfWork;
using BusinessLogic.ExtensionMethods;
using DataAccess.DBEntities;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Data;
using System.Numerics;
using System.Text;
using ViewModels;
using static System.Net.Mime.MediaTypeNames;

namespace BusinessLogic.AppointmentService
{
    public class AppointmentService : IAppointmentService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ICommonService _commonService;
        public AppointmentService(IUnitOfWork unitOfWork, ICommonService commonService)
        {
            _unitOfWork = unitOfWork;
            _commonService = commonService;
        }
        public async Task<ResponseObj<string>> CreateAppointment(AppointmentRequestModel appointmentRequestModel)
        {
            ResponseObj<string> responseObj = new ResponseObj<string>();
            var now = _commonService.GetCurrentIndianTime();

            var appointmentExist = await _unitOfWork.AppointmentRepository.GetWithoutTracking()
                .Where(a => a.PatientId == appointmentRequestModel.PatientId
                && a.DoctorId == appointmentRequestModel.DoctorId && a.Status == AppointmentStatus.Scheduled
                && (a.AppointmentDate.Date == now.Date || a.AppointmentDate.Date == appointmentRequestModel.AppointmentDate.Date)
                ).Include(d => d.Doctor)
                .FirstOrDefaultAsync();
            if (appointmentExist != null)
            {
                responseObj.IsSuccess = false;
                responseObj.Message = $"Appointment already exists with" +
                    $" Dr. {appointmentExist.Doctor.FirstName + " " + appointmentExist.Doctor.LastName} for this patient and doctor.";
                return responseObj;
            }
            Appointment appointment = new Appointment
            {
                Id = Guid.NewGuid().ToString(),
                PatientId = appointmentRequestModel.PatientId,
                DoctorId = appointmentRequestModel.DoctorId,
                AppointmentDate = appointmentRequestModel.AppointmentDate == _commonService.GetCurrentIndianTime().Date ? _commonService.GetCurrentIndianTime() : appointmentRequestModel.AppointmentDate,
                Status = AppointmentStatus.Scheduled,
                Reason = appointmentRequestModel.Reason,
                Notes = appointmentRequestModel.Notes,
                CreatedAt = _commonService.GetCurrentIndianTime()
            };
            await _unitOfWork.AppointmentRepository.Add(appointment);
            bool hasHealthData =
    appointmentRequestModel.Weight != "0" ||
    appointmentRequestModel.Height != "0" ||
    appointmentRequestModel.BloodPressure != "0" ||
    appointmentRequestModel.SugarBeforeMeal != "0" ||
    appointmentRequestModel.SugarAfterMeal != "0";

            if (hasHealthData)
            {
                HealthReport healthReport = new HealthReport
                {
                    PatientId = appointmentRequestModel.PatientId,
                    Weight = appointmentRequestModel.Weight,
                    Height = appointmentRequestModel.Height,
                    BloodPressure = appointmentRequestModel.BloodPressure,
                    SugarBeforeMeal = appointmentRequestModel.SugarBeforeMeal,
                    SugarAfterMeal = appointmentRequestModel.SugarAfterMeal,
                    CreatedAt = _commonService.GetCurrentIndianTime()
                };

                await _unitOfWork.HealthReportRepository.Add(healthReport);
            }
            await _unitOfWork.Complete();
            responseObj.IsSuccess = true;
            responseObj.Message = "Appointment created successfully.";
            return responseObj;
        }
        public async Task<PagedResponse<AppointmentDto>> GetAppointments(PaginationRequest request)
        {
            var query = _unitOfWork.AppointmentRepository
                .GetWithoutTracking();

            query = query.ApplySearch(
                request.Search,
                x => x.Patient.FirstName,
                x => x.Patient.LastName,
                x => x.Patient.Phone);

            if (request.Filters != null &&
                request.Filters.TryGetValue("status", out var status) &&
                !string.IsNullOrWhiteSpace(status))
            {
                var today = _commonService.GetCurrentIndianTime().Date;

                switch (status.ToLower())
                {
                    case "upcoming":
                        query = query.Where(x =>
                            x.AppointmentDate.Date > today &&
                            x.Status == AppointmentStatus.Scheduled);
                        break;

                    case "completed":
                        query = query.Where(x =>
                            x.Status == AppointmentStatus.Completed);
                        break;

                    case "cancelled":
                    case "canceled":
                        query = query.Where(x =>
                            x.Status == AppointmentStatus.Canceled);
                        break;

                    case "scheduled":
                        query = query.Where(x =>
                            x.Status == AppointmentStatus.Scheduled);
                        break;
                }
            }

            var totalCount = await query.CountAsync();

            if (!string.IsNullOrWhiteSpace(request.SortColumn))
            {
                if (request.SortColumn.Equals("patientName", StringComparison.OrdinalIgnoreCase))
                {
                    query = request.SortDirection?.Equals("desc", StringComparison.OrdinalIgnoreCase) == true
                        ? query.OrderByDescending(x => x.Patient.FirstName)
                               .ThenByDescending(x => x.Patient.LastName)
                        : query.OrderBy(x => x.Patient.FirstName)
                               .ThenBy(x => x.Patient.LastName);
                }
                else if (request.SortColumn.Equals("phone", StringComparison.OrdinalIgnoreCase))
                {
                    query = request.SortDirection?.Equals("desc", StringComparison.OrdinalIgnoreCase) == true
                        ? query.OrderByDescending(x => x.Patient.Phone)
                        : query.OrderBy(x => x.Patient.Phone);
                }
                else
                {
                    query = query.ApplySorting(request.SortColumn, request.SortDirection);
                }
            }
            else
            {
                query = query.OrderBy(x => x.AppointmentDate);
            }

            var appointments = await query
                .ApplyPagination(request.PageNumber, request.PageSize)
                .Select(a => new AppointmentDto
                {
                    Id = a.Id,
                    AppointmentDate = a.AppointmentDate,
                    PatientName = a.Patient.FirstName + " " + a.Patient.LastName,
                    Phone = a.Patient.Phone,
                    Status = a.Status
                })
                .ToListAsync();

            return new PagedResponse<AppointmentDto>
            {
                IsSuccess = true,
                Message = "Appointments retrieved successfully.",
                Data = appointments,
                TotalCount = totalCount,
                PageNumber = request.PageNumber,
                PageSize = request.PageSize
            };
        }
        public async Task<ResponseObj<List<AppointmentDto>>> GetTodayAppointments()
        {
            var today = _commonService.GetCurrentIndianTime().Date;
            var appointments = await _unitOfWork.AppointmentRepository.GetWithoutTracking()
                .Where(a => a.AppointmentDate.Date == today)
                .OrderBy(a => a.AppointmentDate)
                .Select(a => new AppointmentDto
                {
                    Id = a.Id,
                    AppointmentDate = a.AppointmentDate,
                    PatientName = a.Patient.FirstName + " " + a.Patient.LastName,
                    Status = a.Status,
                    Phone = a.Patient.Phone
                })
                .ToListAsync();
            return new ResponseObj<List<AppointmentDto>>
            {
                IsSuccess = true,
                Message = "Today's appointments retrieved successfully.",
                Data = appointments
            };
        }
        public async Task<ResponseObj<List<AppointmentDto>>> GetUpcomingAppointments()
        {
            var today = _commonService.GetCurrentIndianTime().Date;
            var appointments = await _unitOfWork.AppointmentRepository.GetWithoutTracking()
                .Where(a => a.AppointmentDate.Date > today && a.Status == AppointmentStatus.Scheduled)
                .OrderBy(a => a.AppointmentDate)
                .Select(a => new AppointmentDto
                {
                    Id = a.Id,
                    AppointmentDate = a.AppointmentDate,
                    PatientName = a.Patient.FirstName + " " + a.Patient.LastName,
                    Status = a.Status,
                    Phone = a.Patient.Phone
                })
                .ToListAsync();
            return new ResponseObj<List<AppointmentDto>>
            {
                IsSuccess = true,
                Message = "Today's upcoming appointments retrieved successfully.",
                Data = appointments
            };
        }
        public async Task<ResponseObj<List<AppointmentDto>>> GetCompletedAppointments()
        {
            var today = _commonService.GetCurrentIndianTime().Date;
            var appointments = await _unitOfWork.AppointmentRepository.GetWithoutTracking()
                .Where(a => a.Status == AppointmentStatus.Completed)
                .OrderBy(a => a.AppointmentDate)
                .Select(a => new AppointmentDto
                {
                    Id = a.Id,
                    AppointmentDate = a.AppointmentDate,
                    PatientName = a.Patient.FirstName + " " + a.Patient.LastName,
                    Status = a.Status,
                    Phone = a.Patient.Phone
                })
                .ToListAsync();
            return new ResponseObj<List<AppointmentDto>>
            {
                IsSuccess = true,
                Message = "Completed appointments retrieved successfully.",
                Data = appointments
            };
        }
        public async Task<ResponseObj<AppointmentDetailsViewModel>> AppointmentDetails(string appointmentId)
        {
            ResponseObj<AppointmentDetailsViewModel> responseObj = new ResponseObj<AppointmentDetailsViewModel>();

            var appointment = await _unitOfWork.AppointmentRepository.GetWithoutTracking()
                .Where(a => a.Id == appointmentId)
                .Select(a => new AppointmentDetailsViewModel
                {
                    Id = a.Id,
                    AppointmentDate = a.AppointmentDate,
                    Status = a.Status,
                    PatientId = a.PatientId,
                    DoctorId = a.DoctorId,
                    PatientName = a.Patient.FirstName + " " + a.Patient.LastName,
                    DoctorName = a.Doctor.FirstName + " " + a.Doctor.LastName,
                    DoctorSpecialization = a.Doctor.Specialization,
                    DoctorPhone = a.Doctor.Phone,
                    DoctorEmail = a.Doctor.Email,
                    ConsultationFee = a.Doctor.ConsultationFee,
                    PatientPhone = a.Patient.Phone,
                    PatientGender = a.Patient.Gender,
                    BloodGroup = a.Patient.BloodGroup,
                    Age = _commonService.GetCurrentIndianTime().Year - a.Patient.DateOfBirth.Year,
                    CreatedAt = a.CreatedAt,
                    GeneticFrom = a.Patient.GeneticFrom,
                    GeneticDiseases = a.Patient.PatientDiseases.Select(g => new GeneticDiseaseViewModel
                    {
                        DiseaseName = g.GeneticDisease.DiseaseName,
                        Medicines = g.Medicines,
                        ColorCode = g.GeneticDisease.ColorCode
                    }).ToList(),
                })
                .FirstOrDefaultAsync();
            if (appointment == null)
            {
                responseObj.IsSuccess = false;
                responseObj.Message = "Appointment not found.";
                return responseObj;
            }
            var healthRpt = await _unitOfWork.HealthReportRepository.GetWithoutTracking()
               .Where(h => h.PatientId == appointment.PatientId && h.CreatedAt.Date == _commonService.GetCurrentIndianTime().Date).OrderByDescending(d => d.CreatedAt).FirstOrDefaultAsync();
            if (healthRpt != null)
            {
                appointment.Weight = healthRpt.Weight != null ? healthRpt.Weight : "0";
                appointment.Height = healthRpt.Height != null ? healthRpt.Height : "0";
                appointment.BloodPressure = healthRpt.BloodPressure != null ? healthRpt.BloodPressure : "0";
                appointment.SugarBeforeMeal = healthRpt.SugarBeforeMeal != null ? healthRpt.SugarBeforeMeal : "0";
                appointment.SugarAfterMeal = healthRpt.SugarAfterMeal != null ? healthRpt.SugarAfterMeal : "0";
            }
            responseObj.IsSuccess = true;
            responseObj.Message = "Appointment details retrieved successfully.";
            responseObj.Data = appointment;
            return responseObj;
        }
        public async Task<ResponseObj<List<AppointmentDto>>> GetAppointmentsByPatientId(string patientId)
        {
            var appointments = await _unitOfWork.AppointmentRepository.GetWithoutTracking()
                .Where(a => a.PatientId == patientId)
                .Select(a => new AppointmentDto
                {
                    Id = a.Id,
                    AppointmentDate = a.AppointmentDate,
                    Status = a.Status,
                    DoctorName = a.Doctor.FirstName + " " + a.Doctor.LastName
                })
                .ToListAsync();
            return new ResponseObj<List<AppointmentDto>>
            {
                IsSuccess = true,
                Message = "Appointments retrieved successfully.",
                Data = appointments
            };
        }
        public async Task<ResponseObj<string>> UpdateAppointmentStatus(string appointmentId, AppointmentStatus status)
        {
            ResponseObj<string> responseObj = new ResponseObj<string>();
            var appointment = await _unitOfWork.AppointmentRepository.GetWithTracking()
                .Where(a => a.Id == appointmentId)
                .FirstOrDefaultAsync();
            if (appointment == null)
            {
                responseObj.IsSuccess = false;
                responseObj.Message = "Appointment not found.";
                return responseObj;
            }
            appointment.Status = status;
            await _unitOfWork.AppointmentRepository.Update(appointment);
            await _unitOfWork.Complete();
            responseObj.IsSuccess = true;
            responseObj.Message = "Appointment status updated successfully.";
            return responseObj;
        }
        public async Task<ResponseObj<string>> DeleteAppointment(string appointmentId)
        {
            ResponseObj<string> responseObj = new ResponseObj<string>();
            var appointment = await _unitOfWork.AppointmentRepository.GetWithTracking()
                .Where(a => a.Id == appointmentId)
                .FirstOrDefaultAsync();
            if (appointment == null)
            {
                responseObj.IsSuccess = false;
                responseObj.Message = "Appointment not found.";
                return responseObj;
            }
            await _unitOfWork.AppointmentRepository.Remove(appointment);
            await _unitOfWork.Complete();
            responseObj.IsSuccess = true;
            responseObj.Message = "Appointment deleted successfully.";
            return responseObj;
        }
        public async Task<ResponseObj<List<AppointmentViewModel>>> Calendar(int? Month, int? Year, string role, string userId)
        {
            ResponseObj<List<AppointmentViewModel>> responseObj = new();

            DateTime today = _commonService.GetCurrentIndianTime().Date;
            int selectedMonth = Month ?? today.Month;
            int selectedYear = Year ?? today.Year;

            // ✅ Step 1: Base Query (ENTITY, not projection)
            var query = _unitOfWork.AppointmentRepository
                .GetWithoutTracking()
                .AsQueryable();

            // ✅ Step 2: Doctor Filter (apply BEFORE select)
            if (role == "Doctor")
            {
                var doctor = await _unitOfWork.DoctorRepository
                    .GetWithoutTracking()
                    .FirstOrDefaultAsync(d => d.UserId == userId);

                if (doctor != null)
                {
                    query = query.Where(x => x.DoctorId == doctor.Id);
                }
            }

            // ✅ Step 3: Filter by month/year (efficient SQL)
            query = query.Where(x =>
                x.AppointmentDate.Month == selectedMonth &&
                x.AppointmentDate.Year == selectedYear);

            // ✅ Step 4: Projection (ONLY needed fields)
            var appointments = await query
                .Select(x => new AppointmentViewModel
                {
                    Id = x.Id,
                    DoctorId = x.DoctorId, // 🔥 IMPORTANT
                    PatientName = x.Patient.FirstName + " " + x.Patient.LastName,
                    DoctorName = x.Doctor.FirstName + " " + x.Doctor.LastName,
                    AppointmentDate = x.AppointmentDate,
                    Status = x.Status
                })
                .ToListAsync();

            responseObj.IsSuccess = true;
            responseObj.Data = appointments;

            return responseObj;
        }
    }
}
