using BusinessLogic.UnitOfWork;
using DataAccess.DBEntities;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Numerics;
using System.Text;
using ViewModels;

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
                    PatientId=a.PatientId,
                    DoctorId=a.DoctorId,
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
    }
}
