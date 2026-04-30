using BusinessLogic.UnitOfWork;
using DataAccess.DBEntities;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Text;
using ViewModels;

namespace BusinessLogic.PatientService
{
    public class PatientService : IPatientService
    {
        private readonly IUnitOfWork _unitOfWork;

        public PatientService(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }
        public async Task<ResponseObj<string>> CreatePatient(PatientRequestModel patientRequestModel)
        {
            var response = new ResponseObj<string>();
            var Existpatient = await _unitOfWork.PatientRepository.GetWithTracking().Where(p => p.Phone == patientRequestModel.Phone && p.FirstName == patientRequestModel.FirstName && p.LastName == patientRequestModel.LastName).FirstOrDefaultAsync();
            if (Existpatient != null)
            {
                response.IsSuccess = false;
                response.Message = "Patient already exists.";
                return response;
            }
            Patient patient = new Patient();
            patient.Id = Guid.NewGuid().ToString();
            patient.FirstName = patientRequestModel.FirstName;
            patient.LastName = patientRequestModel.LastName;
            patient.UserId = patientRequestModel.UserId;
            patient.Phone = patientRequestModel.Phone;
            patient.Address = patientRequestModel.Address;
            patient.BloodGroup = patientRequestModel.BloodGroup;
            HealthReport healthReport = new HealthReport();
            healthReport.PatientId = patient.Id;
            healthReport.Weight = patientRequestModel.Weight;
            healthReport.Height = patientRequestModel.Height;
            healthReport.DiastolicBp = patientRequestModel.DiastolicBp;
            healthReport.SystolicBp = patientRequestModel.SystolicBp;
            healthReport.SugarAfterMeal = patientRequestModel.SugarAfterMeal;
            healthReport.SugarBeforeMeal = patientRequestModel.SugarBeforeMeal;
            healthReport.CreatedAt = DateTime.UtcNow;
            healthReport.Height = patientRequestModel.Height;
            healthReport.Weight = patientRequestModel.Weight;
            patient.DateOfBirth = patientRequestModel.DateOfBirth;
            patient.Gender = patientRequestModel.Gender;
            patient.CreatedAt = DateTime.UtcNow;
            //patient.DoctorId = patientRequestModel.DoctorId;
            await _unitOfWork.HealthReportRepository.Add(healthReport);
            await _unitOfWork.PatientRepository.Add(patient);
            await _unitOfWork.Complete();
            if (patientRequestModel.BookAppointment)
            {
                Appointment appointment = new Appointment
                {
                    Id = Guid.NewGuid().ToString(),
                    PatientId = patient.Id,
                    DoctorId = patientRequestModel.DoctorId,
                    AppointmentDate = DateTime.UtcNow,
                    Status = "Scheduled",
                    CreatedAt = DateTime.UtcNow
                };
                await _unitOfWork.AppointmentRepository.Add(appointment);
                await _unitOfWork.Complete();
            }
            response.IsSuccess = true;
            response.Message = "Patient created successfully.";
            return response;
        }
        public async Task<ResponseObj<PatientViewModel>> GetPatientById(string Id)
        {
            var response = new ResponseObj<PatientViewModel>();

            var patient = await _unitOfWork.PatientRepository
                .GetWithoutTracking()
                .Where(p => p.Id == Id)
                .Select(p => new
                {
                    Patient = p,

                    // Subquery to get latest health report for today
                    HealthReport = _unitOfWork.HealthReportRepository
                        .GetWithoutTracking()
                        .Where(h => h.PatientId == p.Id && h.CreatedAt.Date == DateTime.Today)
                        .OrderByDescending(h => h.CreatedAt)
                        .FirstOrDefault()
                })
                .Select(x => new PatientViewModel
                {
                    Id = x.Patient.Id,
                    FirstName = x.Patient.FirstName,
                    LastName = x.Patient.LastName,
                    UserId = x.Patient.UserId,
                    Phone = x.Patient.Phone,
                    Address = x.Patient.Address,
                    BloodGroup = x.Patient.BloodGroup,
                    DateOfBirth = x.Patient.DateOfBirth,
                    Gender = x.Patient.Gender,

                    // Safe null handling
                    Weight = x.HealthReport != null ? x.HealthReport.Weight ?? "0" : "0",
                    Height = x.HealthReport != null ? x.HealthReport.Height ?? "0" : "0",
                    DiastolicBp = x.HealthReport != null ? x.HealthReport.DiastolicBp ?? "0" : "0",
                    SystolicBp = x.HealthReport != null ? x.HealthReport.SystolicBp ?? "0" : "0",
                    SugarBeforeMeal = x.HealthReport != null ? x.HealthReport.SugarBeforeMeal ?? "0" : "0",
                    SugarAfterMeal = x.HealthReport != null ? x.HealthReport.SugarAfterMeal ?? "0" : "0"
                })
                .FirstOrDefaultAsync();

            if (patient == null)
            {
                return new ResponseObj<PatientViewModel>
                {
                    IsSuccess = false,
                    Message = "Patient not found."
                };
            }

            return new ResponseObj<PatientViewModel>
            {
                IsSuccess = true,
                Message = "Patient retrieved successfully.",
                Data = patient
            };
        }
        public async Task<ResponseObj<string>> UpdatePatient(PatientRequestModel patientRequestModel, string patientId)
        {
            var response = new ResponseObj<string>();
            var patient = await _unitOfWork.PatientRepository.GetWithTracking().Where(p => p.Id == patientId
                                            || (p.Phone == patientRequestModel.Phone
                                                && p.FirstName == patientRequestModel.FirstName
                                                && p.LastName == patientRequestModel.LastName))
                                        .FirstOrDefaultAsync();
            var healthRpt = await _unitOfWork.HealthReportRepository.GetWithTracking().Where(h => h.PatientId == patientId && h.CreatedAt.Date == DateTime.Today).OrderByDescending(d => d.CreatedAt).FirstOrDefaultAsync();

            if (patient == null)
            {
                response.IsSuccess = false;
                response.Message = "Patient not found.";
                return response;
            }

            if (patient.Id != patientId)
            {
                response.IsSuccess = false;
                response.Message = "Patient already exists.";
                return response;
            }
            patient.FirstName = patientRequestModel.FirstName;
            patient.LastName = patientRequestModel.LastName;
            patient.UserId = patientRequestModel.UserId;
            patient.Phone = patientRequestModel.Phone;
            patient.Address = patientRequestModel.Address;
            patient.BloodGroup = patientRequestModel.BloodGroup;
            patient.DateOfBirth = patientRequestModel.DateOfBirth;
            patient.Gender = patientRequestModel.Gender;
            bool isSameHealthReport =
        healthRpt != null &&
        (healthRpt.Weight ?? "") == (patientRequestModel.Weight ?? "") &&
        (healthRpt.Height ?? "") == (patientRequestModel.Height ?? "") &&
        (healthRpt.SystolicBp ?? "") == (patientRequestModel.SystolicBp ?? "") &&
        (healthRpt.DiastolicBp ?? "") == (patientRequestModel.DiastolicBp ?? "") &&
        (healthRpt.SugarAfterMeal ?? "") == (patientRequestModel.SugarAfterMeal ?? "") &&
        (healthRpt.SugarBeforeMeal ?? "") == (patientRequestModel.SugarBeforeMeal ?? "");

            // Insert HealthReport ONLY if changed
            if (!isSameHealthReport)
            {
                HealthReport healthReport = new HealthReport
                {
                    PatientId = patientId,
                    Weight = patientRequestModel.Weight,
                    Height = patientRequestModel.Height,
                    SystolicBp = patientRequestModel.SystolicBp,
                    DiastolicBp = patientRequestModel.DiastolicBp,
                    SugarBeforeMeal = patientRequestModel.SugarBeforeMeal,
                    SugarAfterMeal = patientRequestModel.SugarAfterMeal,
                    CreatedAt = DateTime.UtcNow
                };

                await _unitOfWork.HealthReportRepository.Add(healthReport);
            }
            await _unitOfWork.PatientRepository.Update(patient);
            await _unitOfWork.Complete();
            response.IsSuccess = true;
            response.Message = "Patient updated successfully.";
            return response;
        }
        public async Task<ResponseObj<List<PatientViewModel>>> GetPatients(string? doctorId = null)
        {
            var response = new ResponseObj<List<PatientViewModel>>();

            var patientsQuery = _unitOfWork.PatientRepository.GetWithoutTracking().AsQueryable();
            var patients = await patientsQuery
                .Select(p => new PatientViewModel
                {
                    Id = p.Id,
                    FirstName = p.FirstName,
                    LastName = p.LastName,
                    UserId = p.UserId,
                    Phone = p.Phone,
                    Gender = p.Gender,
                    Address = p.Address,
                    BloodGroup = p.BloodGroup,
                    DateOfBirth = p.DateOfBirth,
                    CreatedAt = p.CreatedAt,
                }).OrderByDescending(p => p.CreatedAt)
                .ToListAsync();
            if (patients == null || patients.Count == 0)
            {
                response.IsSuccess = false;
                response.Message = "Patients not found.";
            }
            response.IsSuccess = true;
            response.Message = "Patients retrieved successfully.";
            response.Data = patients;
            return response;
        }
        public async Task<ResponseObj<PatientDetailsViewModel>> PatientDetails(string Id)
        {
            ResponseObj<PatientDetailsViewModel> responseObj = new ResponseObj<PatientDetailsViewModel>();
            var healthRpt = await _unitOfWork.HealthReportRepository.GetWithoutTracking().Where(h => h.PatientId == Id).OrderByDescending(d => d.CreatedAt).FirstOrDefaultAsync();
            var details = await _unitOfWork.PatientRepository.GetWithoutTracking().Where(x => x.Id == Id)
                .Select(p => new PatientDetailsViewModel
                {
                    Id = p.Id,
                    PatientName = p.FirstName + " " + p.LastName,
                    DateOfBirth = p.DateOfBirth,
                    CreatedAt = p.CreatedAt,
                    Gender = p.Gender,
                    Phone = p.Phone,
                    Address = p.Address,
                    BloodGroup = p.BloodGroup,
                    Prescriptions = p.Prescriptions.Select(pr => new PrintViewModel
                    {
                        PrescriptionDate = pr.PrescriptionDate,
                        prescriptionItemRequests = pr.Items.Select(p => new PrescriptionItemRequest { Id = p.Id, MedicineName = p.MedicineName, Dosage = p.Dosage, Duration = p.Duration, MealTiming = p.MealTiming, Frequency = p.Frequency }).ToList(),
                        DoctorName = pr.Doctor.FirstName + " " + pr.Doctor.LastName
                    }).ToList(),
                    Weight = healthRpt != null && healthRpt.Weight != null ? healthRpt.Weight : "0",
                    Height = healthRpt != null && healthRpt.Height != null ? healthRpt.Height : "0",
                    DiastolicBp = healthRpt != null && healthRpt.DiastolicBp != null ? healthRpt.DiastolicBp : "0",
                    SystolicBp = healthRpt != null && healthRpt.SystolicBp != null ? healthRpt.SystolicBp : "0",
                    SugarBeforeMeal = healthRpt != null && healthRpt.DiastolicBp != null ? healthRpt.SugarBeforeMeal : "0",
                    SugarAfterMeal = healthRpt != null && healthRpt.SugarAfterMeal != null ? healthRpt.SugarAfterMeal : "0",
                    Appointments = p.Appointments.Select(a => new AppointmentDto { AppointmentDate = a.AppointmentDate, Status = a.Status, DoctorName = a.Doctor.FirstName + " " + a.Doctor.LastName, Id = a.Id }).ToList(),
                    MedicalRecords = p.MedicalRecords.Select(m => new MedicalRecordResponseModel { Id = m.Id, VisitDate = m.VisitDate, DoctorName = m.Doctor.FirstName + " " + m.Doctor.LastName, Diagnosis = m.Diagnosis }).ToList()

                }).FirstOrDefaultAsync();
            if (details == null)
            {
                responseObj.Data = null;
                responseObj.IsSuccess = false;
                responseObj.Message = "No record found";
                return responseObj;
            }
            responseObj.IsSuccess = true;
            responseObj.Data = details;
            responseObj.Message = "Record retreived successfully";
            return responseObj;
        }
    }
}
