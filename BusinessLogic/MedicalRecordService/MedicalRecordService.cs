using BusinessLogic.UnitOfWork;
using DataAccess.DBEntities;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Text;
using ViewModels;

namespace BusinessLogic.MedicalRecordService
{
    public class MedicalRecordService : IMedicalRecordService
    {
        private readonly IUnitOfWork _unitOfWork;
        public MedicalRecordService(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }
        public async Task<ResponseObj<string>> CreateMedicalRecord(MedicalRecordRequestModel medicalRecordRequestModel)
        {
            ResponseObj<string> responseObj = new ResponseObj<string>();
            MedicalRecord medicalRecord = new MedicalRecord();
            var healthRpt = await _unitOfWork.HealthReportRepository.GetWithTracking().Where(h => h.PatientId == medicalRecordRequestModel.PatientId).OrderByDescending(d => d.CreatedAt).FirstOrDefaultAsync();
            var appointments = await _unitOfWork.AppointmentRepository.GetWithoutTracking().Where(x => x.PatientId == medicalRecordRequestModel.PatientId).FirstOrDefaultAsync();
            if (appointments == null)
            {
                responseObj.IsSuccess = false;
                responseObj.Message = "No appointment found for this patient";
                return responseObj;
            }
            medicalRecord.Id = Guid.NewGuid().ToString();
            medicalRecord.VisitDate = DateTime.UtcNow;
            medicalRecord.AppointmentId = medicalRecordRequestModel.AppointmentId;
            medicalRecord.Treatment = medicalRecordRequestModel.Treatment;
            medicalRecord.Diagnosis = medicalRecordRequestModel.Diagnosis;
            medicalRecord.DoctorId = medicalRecordRequestModel.DoctorId;
            medicalRecord.PatientId = medicalRecordRequestModel.PatientId;
            medicalRecord.Symptoms = medicalRecordRequestModel.Symptoms;
            medicalRecord.Notes = medicalRecordRequestModel.Notes;
            medicalRecord.CreatedAt = DateTime.UtcNow;
            bool isSameHealthReport =
        healthRpt != null &&
        (healthRpt.Weight ?? "") == (medicalRecordRequestModel.Weight ?? "") &&
        (healthRpt.Height ?? "") == (medicalRecordRequestModel.Height ?? "") &&
        (healthRpt.SystolicBp ?? "") == (medicalRecordRequestModel.SystolicBp ?? "") &&
        (healthRpt.DiastolicBp ?? "") == (medicalRecordRequestModel.DiastolicBp ?? "") &&
        (healthRpt.SugarAfterMeal ?? "") == (medicalRecordRequestModel.SugarAfterMeal ?? "") &&
        (healthRpt.SugarBeforeMeal ?? "") == (medicalRecordRequestModel.SugarBeforeMeal ?? "");

            if (!isSameHealthReport)
            {
                HealthReport healthReport = new HealthReport
                {
                    PatientId = medicalRecordRequestModel.PatientId,
                    Weight = medicalRecordRequestModel.Weight,
                    Height = medicalRecordRequestModel.Height,
                    SystolicBp = medicalRecordRequestModel.SystolicBp,
                    DiastolicBp = medicalRecordRequestModel.DiastolicBp,
                    SugarBeforeMeal = medicalRecordRequestModel.SugarBeforeMeal,
                    SugarAfterMeal = medicalRecordRequestModel.SugarAfterMeal,
                    CreatedAt = DateTime.UtcNow
                };

                await _unitOfWork.HealthReportRepository.Add(healthReport);
            }

            await _unitOfWork.MedicalRecordRepository.Add(medicalRecord);
            await _unitOfWork.Complete();
            responseObj.IsSuccess = true;
            responseObj.Message = "Medical record created successfully";
            return responseObj;
        }
        public async Task<ResponseObj<string>> UpdateMedicalRecord(MedicalRecordRequestModel medicalRecordRequestModel, string medicalRecordId)
        {
            ResponseObj<string> responseObj = new ResponseObj<string>();
            var healthRpt = await _unitOfWork.HealthReportRepository.GetWithoutTracking().Where(h => h.PatientId == medicalRecordRequestModel.PatientId).OrderByDescending(d => d.CreatedAt).FirstOrDefaultAsync();
            var medicalRecord = await _unitOfWork.MedicalRecordRepository.GetWithoutTracking().Where(x => x.Id == medicalRecordId).FirstOrDefaultAsync();
            if (medicalRecord == null)
            {
                responseObj.IsSuccess = false;
                responseObj.Message = "Medical record not found";
                return responseObj;
            }
            medicalRecord.VisitDate = DateTime.UtcNow;
            medicalRecord.Treatment = medicalRecordRequestModel.Treatment;
            medicalRecord.Diagnosis = medicalRecordRequestModel.Diagnosis;
            medicalRecord.DoctorId = medicalRecordRequestModel.DoctorId;
            medicalRecord.PatientId = medicalRecordRequestModel.PatientId;
            medicalRecord.Symptoms = medicalRecordRequestModel.Symptoms;
            medicalRecord.Notes = medicalRecordRequestModel.Notes;
            medicalRecord.CreatedAt = DateTime.UtcNow;
            bool isSameHealthReport =
        healthRpt != null &&
        (healthRpt.Weight ?? "") == (medicalRecordRequestModel.Weight ?? "") &&
        (healthRpt.Height ?? "") == (medicalRecordRequestModel.Height ?? "") &&
        (healthRpt.SystolicBp ?? "") == (medicalRecordRequestModel.SystolicBp ?? "") &&
        (healthRpt.DiastolicBp ?? "") == (medicalRecordRequestModel.DiastolicBp ?? "") &&
        (healthRpt.SugarAfterMeal ?? "") == (medicalRecordRequestModel.SugarAfterMeal ?? "") &&
        (healthRpt.SugarBeforeMeal ?? "") == (medicalRecordRequestModel.SugarBeforeMeal ?? "");

            if (!isSameHealthReport)
            {
                HealthReport healthReport = new HealthReport
                {
                    PatientId = medicalRecordRequestModel.PatientId,
                    Weight = medicalRecordRequestModel.Weight,
                    Height = medicalRecordRequestModel.Height,
                    SystolicBp = medicalRecordRequestModel.SystolicBp,
                    DiastolicBp = medicalRecordRequestModel.DiastolicBp,
                    SugarBeforeMeal = medicalRecordRequestModel.SugarBeforeMeal,
                    SugarAfterMeal = medicalRecordRequestModel.SugarAfterMeal,
                    CreatedAt = DateTime.UtcNow
                };

                await _unitOfWork.HealthReportRepository.Add(healthReport);
            }
            await _unitOfWork.MedicalRecordRepository.Update(medicalRecord);
            await _unitOfWork.Complete();
            responseObj.IsSuccess = true;
            responseObj.Message = "Medical record updated successfully";
            return responseObj;
        }
        public async Task<ResponseObj<List<MedicalRecordResponseModel>>> GetMedicalRecordsByPatientId(string patientId)
        {
            ResponseObj<List<MedicalRecordResponseModel>> responseObj = new ResponseObj<List<MedicalRecordResponseModel>>();
            var healthRpt = await _unitOfWork.HealthReportRepository.GetWithoutTracking().Where(h => h.PatientId == patientId).OrderByDescending(d => d.CreatedAt).FirstOrDefaultAsync();
            var medicalRecords = await _unitOfWork.MedicalRecordRepository.GetWithoutTracking().Where(x => x.PatientId == patientId).
                Select(x => new MedicalRecordResponseModel
                {
                    Id = x.Id,
                    PatientId = x.PatientId,
                    DoctorId = x.DoctorId,
                    PatientName = x.Patient.FirstName + " " + x.Patient.LastName,
                    DoctorName = x.Doctor.FirstName + " " + x.Doctor.LastName,
                    AppointmentId = x.AppointmentId,
                    VisitDate = x.VisitDate,
                    Diagnosis = x.Diagnosis,
                    Symptoms = x.Symptoms,
                    Treatment = x.Treatment,
                    Notes = x.Notes,
                    Weight = healthRpt != null && healthRpt.Weight != null ? healthRpt.Weight : "0",
                    Height = healthRpt != null && healthRpt.Height != null ? healthRpt.Height : "0",
                    DiastolicBp = healthRpt != null && healthRpt.DiastolicBp != null ? healthRpt.DiastolicBp : "0",
                    SystolicBp = healthRpt != null && healthRpt.SystolicBp != null ? healthRpt.SystolicBp : "0",
                    SugarBeforeMeal = healthRpt != null && healthRpt.DiastolicBp != null ? healthRpt.SugarBeforeMeal : "0",
                    SugarAfterMeal = healthRpt != null && healthRpt.SugarAfterMeal != null ? healthRpt.SugarAfterMeal : "0",
                }).ToListAsync();
            if (medicalRecords == null || medicalRecords.Count == 0)
            {
                responseObj.IsSuccess = false;
                responseObj.Message = "No medical records found for this patient";
                return responseObj;
            }
            responseObj.IsSuccess = true;
            responseObj.Message = "Medical records retrieved successfully";
            responseObj.Data = medicalRecords;
            return responseObj;
        }
        public async Task<ResponseObj<List<MedicalRecordResponseModel>>> GetMedicalRecords()
        {
            ResponseObj<List<MedicalRecordResponseModel>> responseObj = new ResponseObj<List<MedicalRecordResponseModel>>();
            var medicalRecords = await _unitOfWork.MedicalRecordRepository.GetWithoutTracking()
                .Select(x => new MedicalRecordResponseModel
                {
                    Id = x.Id,
                    PatientId = x.PatientId,
                    DoctorId = x.DoctorId,
                    PatientName = x.Patient.FirstName + " " + x.Patient.LastName,
                    DoctorName = x.Doctor.FirstName + " " + x.Doctor.LastName,
                    AppointmentId = x.AppointmentId,
                    VisitDate = x.VisitDate,
                    Diagnosis = x.Diagnosis,
                    Symptoms = x.Symptoms,
                    Treatment = x.Treatment,
                    Notes = x.Notes
                }).ToListAsync();
            if (medicalRecords == null || medicalRecords.Count == 0)
            {
                responseObj.IsSuccess = false;
                responseObj.Message = "No medical records found";
                return responseObj;
            }
            responseObj.IsSuccess = true;
            responseObj.Message = "Medical records retrieved successfully";
            responseObj.Data = medicalRecords;
            return responseObj;
        }
        public async Task<ResponseObj<List<MedicalRecordResponseModel>>> GetMedicalRecordById(string id)
        {
            ResponseObj<List<MedicalRecordResponseModel>> responseObj = new ResponseObj<List<MedicalRecordResponseModel>>();
            var medicalRecords = await _unitOfWork.MedicalRecordRepository.GetWithoutTracking()
                .Where(x => x.Id == id)
                .Select(x => new MedicalRecordResponseModel
                {
                    Id = x.Id,
                    PatientId = x.PatientId,
                    DoctorId = x.DoctorId,
                    PatientName = x.Patient.FirstName + " " + x.Patient.LastName,
                    BloodGroup = x.Patient.BloodGroup,
                    DoctorName = x.Doctor.FirstName + " " + x.Doctor.LastName,
                    Specialization = x.Doctor.Specialization,
                    AppointmentId = x.AppointmentId,
                    VisitDate = x.VisitDate,
                    Diagnosis = x.Diagnosis,
                    PatientPhone = x.Patient.Phone,
                    Symptoms = x.Symptoms,
                    Treatment = x.Treatment,
                    Notes = x.Notes,
                    Appointment = x.Appointment != null ? new AppointmentViewModel
                    {
                        Id = x.Appointment.Id,
                        PatientId = x.Appointment.PatientId,
                        DoctorId = x.Appointment.DoctorId,
                        AppointmentDate = x.Appointment.AppointmentDate,
                        Status = x.Appointment.Status,
                        Reason = x.Appointment.Reason
                    } : null,
                    Prescriptions = x.Prescriptions.Select(p => new PrescriptionViewModel
                    {
                        Id = p.Id,
                        MedicalRecordId = p.MedicalRecordId,
                        Instructions = p.Instructions,
                        prescriptionItemRequests = p.Items.Select(pi => new PrescriptionItemRequest
                        {
                            Id = pi.Id,
                            MedicineName = pi.MedicineName,
                            Dosage = pi.Dosage,
                            Duration = pi.Duration,
                        }).ToList()
                    }).ToList(),

                }).ToListAsync();
            if (medicalRecords == null || medicalRecords.Count == 0)
            {
                responseObj.IsSuccess = false;
                responseObj.Message = "No medical records found";
                return responseObj;
            }
            responseObj.IsSuccess = true;
            responseObj.Message = "Medical records retrieved successfully";
            responseObj.Data = medicalRecords;
            return responseObj;
        }
        public async Task<ResponseObj<MedicalRecordResponseModel>> GetMedicalRecordForEdit(string id)
        {
            ResponseObj<MedicalRecordResponseModel> responseObj = new ResponseObj<MedicalRecordResponseModel>();
            var medicalRecords = await _unitOfWork.MedicalRecordRepository.GetWithTracking().Where(x => x.Id == id)
                .Select(x => new MedicalRecordResponseModel
                {
                    Id = x.Id,
                    PatientId = x.PatientId,
                    DoctorId = x.DoctorId,
                    PatientName = x.Patient.FirstName + " " + x.Patient.LastName,
                    DoctorName = x.Doctor.FirstName + " " + x.Doctor.LastName,
                    AppointmentId = x.AppointmentId,
                    VisitDate = x.VisitDate,
                    Diagnosis = x.Diagnosis,
                    Symptoms = x.Symptoms,
                    Treatment = x.Treatment,
                    Notes = x.Notes,
                    AppointmentDate = x.Appointment.AppointmentDate
                }).FirstOrDefaultAsync();
            if (medicalRecords != null)
            {
                var healthRpt = await _unitOfWork.HealthReportRepository.GetWithoutTracking().Where(h => h.PatientId == medicalRecords.PatientId).OrderByDescending(d => d.CreatedAt).FirstOrDefaultAsync();
                if (healthRpt != null)
                {
                    medicalRecords.Weight = healthRpt.Weight != null ? healthRpt.Weight : "0";
                    medicalRecords.Height = healthRpt.Height != null ? healthRpt.Height : "0";
                    medicalRecords.DiastolicBp = healthRpt.DiastolicBp != null ? healthRpt.DiastolicBp : "0";
                    medicalRecords.SystolicBp = healthRpt.SystolicBp != null ? healthRpt.SystolicBp : "0";
                    medicalRecords.SugarBeforeMeal = healthRpt.DiastolicBp != null ? healthRpt.SugarBeforeMeal : "0";
                    medicalRecords.SugarAfterMeal = healthRpt.SugarAfterMeal != null ? healthRpt.SugarAfterMeal : "0";
                }
            }

            if (medicalRecords == null)
            {
                responseObj.IsSuccess = false;
                responseObj.Message = "No medical records found";
                return responseObj;
            }
            responseObj.IsSuccess = true;
            responseObj.Message = "Medical records retrieved successfully";
            responseObj.Data = medicalRecords;
            return responseObj;
        }
    }
}
