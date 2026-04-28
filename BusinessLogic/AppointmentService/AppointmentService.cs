using BusinessLogic.UnitOfWork;
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
        public AppointmentService(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }
        public async Task<ResponseObj<string>> CreateAppointment(AppointmentRequestModel appointmentRequestModel)
        {
            ResponseObj<string> responseObj = new ResponseObj<string>();
            var now = DateTime.Now;
            var dayStart = now.Date;
            var dayEnd = dayStart.AddDays(1);
            var appointmentExist = await _unitOfWork.AppointmentRepository.GetWithoutTracking()
                .Where(a => a.PatientId == appointmentRequestModel.PatientId
                && a.DoctorId == appointmentRequestModel.DoctorId
                && a.AppointmentDate >= dayStart && a.AppointmentDate < dayEnd
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
                AppointmentDate = DateTime.Now,
                Status = "Scheduled",
                Reason = appointmentRequestModel.Reason,
                Notes = appointmentRequestModel.Notes,
                CreatedAt = DateTime.Now
            };
            await _unitOfWork.AppointmentRepository.Add(appointment);
            await _unitOfWork.Complete();
            responseObj.IsSuccess = true;
            responseObj.Message = "Appointment created successfully.";
            return responseObj;
        }
        public async Task<ResponseObj<DoctorDetailAppointmentsViewModel>> GetAppointments()
        {
            ResponseObj<DoctorDetailAppointmentsViewModel> responseObj = new ResponseObj<DoctorDetailAppointmentsViewModel>();
            DoctorDetailAppointmentsViewModel appointments = new DoctorDetailAppointmentsViewModel();
            var today = DateTime.Today;
            var query = _unitOfWork.AppointmentRepository
                    .GetWithoutTracking().AsQueryable();
            appointments.Appointments = await query
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
            appointments.TodayAppointments = await query
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

            appointments.CompletedAppointments = await query
                .Where(a => a.AppointmentDate.Date == today &&
                            a.Status == "Completed")
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
            appointments.UpcomingAppointments = await query
                .Where(a => a.AppointmentDate.Date == today &&
                            a.Status == "Scheduled")
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

            responseObj.IsSuccess = true;
            responseObj.Message = "Appointments retrieved successfully.";
            responseObj.Data = appointments;

            return responseObj;
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
                    Age = DateTime.UtcNow.Year - a.Patient.DateOfBirth.Year,
                    CreatedAt = a.CreatedAt
                })
                .FirstOrDefaultAsync();
            var healthRpt = await _unitOfWork.HealthReportRepository.GetWithoutTracking().Where(h => h.PatientId == appointment.PatientId).OrderByDescending(d => d.CreatedAt).FirstOrDefaultAsync();
            if(healthRpt != null)
            {
                appointment.Weight = healthRpt.Weight != null ? healthRpt.Weight : "0";
                appointment.Height = healthRpt.Height != null ? healthRpt.Height : "0";
                appointment.DiastolicBp = healthRpt.DiastolicBp != null ? healthRpt.DiastolicBp : "0";
                appointment.SystolicBp = healthRpt.SystolicBp != null ? healthRpt.SystolicBp : "0";
                appointment.SugarBeforeMeal = healthRpt.DiastolicBp != null ? healthRpt.SugarBeforeMeal : "0";
                appointment.SugarAfterMeal = healthRpt.SugarAfterMeal != null ? healthRpt.SugarAfterMeal : "0";
            }
            if (appointment == null)
            {
                responseObj.IsSuccess = false;
                responseObj.Message = "Appointment not found.";
                return responseObj;
            }
            responseObj.IsSuccess = true;
            responseObj.Message = "Appointment details retrieved successfully.";
            responseObj.Data = appointment;
            return responseObj;
        }
        public async Task<ResponseObj<string>> UpdateAppointmentStatus(string appointmentId, string status)
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

            DateTime today = DateTime.Today;
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
