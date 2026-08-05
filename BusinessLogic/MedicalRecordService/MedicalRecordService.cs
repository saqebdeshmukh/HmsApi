using BusinessLogic.CommonService;
using BusinessLogic.ExtensionMethods;
using BusinessLogic.UnitOfWork;
using DataAccess.DBEntities;
using DocumentFormat.OpenXml.Drawing.Charts;
using Microsoft.AspNetCore.Http.HttpResults;
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
        private readonly ICommonService _commonService;
        public MedicalRecordService(IUnitOfWork unitOfWork, ICommonService commonService)
        {
            _unitOfWork = unitOfWork;
            _commonService = commonService;
        }
        public async Task<ResponseObj<string>> CreateMedicalRecord(MedicalRecordRequestModel medicalRecordRequestModel)
        {
            ResponseObj<string> responseObj = new ResponseObj<string>();
            MedicalRecord medicalRecord = new MedicalRecord();
            //var healthRpt = await _unitOfWork.HealthReportRepository.GetWithTracking()
            //    .Where(h => h.PatientId == medicalRecordRequestModel.PatientId && h.CreatedAt.Date == _commonService.GetCurrentIndianTime().Date).OrderByDescending(d => d.CreatedAt).FirstOrDefaultAsync();
            var appointments = await _unitOfWork.AppointmentRepository.GetWithoutTracking().Where(x => x.PatientId == medicalRecordRequestModel.PatientId).FirstOrDefaultAsync();
            if (appointments == null)
            {
                responseObj.IsSuccess = false;
                responseObj.Message = "No appointment found for this patient";
                return responseObj;
            }
            medicalRecord.Id = Guid.NewGuid().ToString();
            medicalRecord.VisitDate = _commonService.GetCurrentIndianTime();
            medicalRecord.AppointmentId = medicalRecordRequestModel.AppointmentId;
            medicalRecord.Treatment = medicalRecordRequestModel.Treatment;
            medicalRecord.Diagnosis = medicalRecordRequestModel.Diagnosis;
            medicalRecord.DoctorId = medicalRecordRequestModel.DoctorId;
            medicalRecord.PatientId = medicalRecordRequestModel.PatientId;
            medicalRecord.Symptoms = medicalRecordRequestModel.Symptoms;
            medicalRecord.Notes = medicalRecordRequestModel.Notes;
            medicalRecord.CreatedAt = _commonService.GetCurrentIndianTime();
            bool hasHealthData =
    medicalRecordRequestModel.Weight != "0" ||
    medicalRecordRequestModel.Height != "0" ||
    medicalRecordRequestModel.BloodPressure != "0" ||
    medicalRecordRequestModel.SugarBeforeMeal != "0" ||
    medicalRecordRequestModel.SugarAfterMeal != "0";

            if (hasHealthData)
            {
                HealthReport healthReport = new HealthReport
                {
                    PatientId = medicalRecordRequestModel.PatientId,
                    Weight = medicalRecordRequestModel.Weight,
                    Height = medicalRecordRequestModel.Height,
                    BloodPressure = medicalRecordRequestModel.BloodPressure,
                    SugarBeforeMeal = medicalRecordRequestModel.SugarBeforeMeal,
                    SugarAfterMeal = medicalRecordRequestModel.SugarAfterMeal,
                    CreatedAt = _commonService.GetCurrentIndianTime()
                };

                await _unitOfWork.HealthReportRepository.Add(healthReport);
            }

            await _unitOfWork.MedicalRecordRepository.Add(medicalRecord);
            await _unitOfWork.Complete();
            var appointment = await _unitOfWork.AppointmentRepository.GetWithTracking().Where(x => x.Id == medicalRecord.AppointmentId).FirstOrDefaultAsync();
            appointment.Status = AppointmentStatus.MedicalRecordCreated;
            await _unitOfWork.AppointmentRepository.Update(appointment);
            await _unitOfWork.Complete();
            responseObj.Data = medicalRecord.Id;
            responseObj.IsSuccess = true;
            responseObj.Message = "Medical record created successfully";
            return responseObj;
        }
        public async Task<ResponseObj<string>> UpdateMedicalRecord(MedicalRecordRequestModel medicalRecordRequestModel, string medicalRecordId)
        {
            ResponseObj<string> responseObj = new ResponseObj<string>();
            var healthRpt = await _unitOfWork.HealthReportRepository.GetWithoutTracking()
                .Where(h => h.PatientId == medicalRecordRequestModel.PatientId && h.CreatedAt.Date == _commonService.GetCurrentIndianTime().Date)
                .OrderByDescending(d => d.CreatedAt).FirstOrDefaultAsync();
            var medicalRecord = await _unitOfWork.MedicalRecordRepository.GetWithoutTracking().Where(x => x.Id == medicalRecordId).FirstOrDefaultAsync();
            if (medicalRecord == null)
            {
                responseObj.IsSuccess = false;
                responseObj.Message = "Medical record not found";
                return responseObj;
            }
            medicalRecord.Treatment = medicalRecordRequestModel.Treatment;
            medicalRecord.Diagnosis = medicalRecordRequestModel.Diagnosis;
            medicalRecord.DoctorId = medicalRecordRequestModel.DoctorId;
            medicalRecord.PatientId = medicalRecordRequestModel.PatientId;
            medicalRecord.Symptoms = medicalRecordRequestModel.Symptoms;
            medicalRecord.Notes = medicalRecordRequestModel.Notes;
            medicalRecord.CreatedAt = _commonService.GetCurrentIndianTime();
            bool isSameHealthReport =
        healthRpt != null &&
        (healthRpt.Weight ?? "") == (medicalRecordRequestModel.Weight ?? "") &&
        (healthRpt.Height ?? "") == (medicalRecordRequestModel.Height ?? "") &&
        (healthRpt.BloodPressure ?? "") == (medicalRecordRequestModel.BloodPressure ?? "") &&
        (healthRpt.SugarAfterMeal ?? "") == (medicalRecordRequestModel.SugarAfterMeal ?? "") &&
        (healthRpt.SugarBeforeMeal ?? "") == (medicalRecordRequestModel.SugarBeforeMeal ?? "");

            if (!isSameHealthReport)
            {
                HealthReport healthReport = new HealthReport
                {
                    PatientId = medicalRecordRequestModel.PatientId,
                    Weight = medicalRecordRequestModel.Weight,
                    Height = medicalRecordRequestModel.Height,
                    BloodPressure = medicalRecordRequestModel.BloodPressure,
                    SugarBeforeMeal = medicalRecordRequestModel.SugarBeforeMeal,
                    SugarAfterMeal = medicalRecordRequestModel.SugarAfterMeal,
                    CreatedAt = _commonService.GetCurrentIndianTime()
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
            var healthRpt = await _unitOfWork.HealthReportRepository.GetWithoutTracking()
                .Where(h => h.PatientId == patientId && h.CreatedAt.Date == _commonService.GetCurrentIndianTime().Date).OrderByDescending(d => d.CreatedAt).FirstOrDefaultAsync();
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
                    BloodPressure = healthRpt != null && healthRpt.BloodPressure != null ? healthRpt.BloodPressure : "0",
                    SugarBeforeMeal = healthRpt != null && healthRpt.SugarBeforeMeal != null ? healthRpt.SugarBeforeMeal : "0",
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
        public async Task<PagedResponse<MedicalRecordResponseModel>> GetMedicalRecords(PaginationRequest request)
        {
            var query = _unitOfWork.MedicalRecordRepository
                .GetWithoutTracking();

            // Search
            query = query.ApplySearch(
                request.Search,
                x => x.Patient.FirstName,
                x => x.Patient.LastName,
                x => x.Patient.Phone);

            // Date Filter
            if (request.Filters != null &&
                request.Filters.TryGetValue("visitDate", out var visitDate) &&
                !string.IsNullOrWhiteSpace(visitDate))
            {
                if (DateTime.TryParse(visitDate, out var selectedDate))
                {
                    selectedDate = selectedDate.Date;

                    query = query.Where(x => x.VisitDate.Date == selectedDate);
                }
            }

            var totalCount = await query.CountAsync();

            // Sorting
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
                else if (request.SortColumn.Equals("doctorName", StringComparison.OrdinalIgnoreCase))
                {
                    query = request.SortDirection?.Equals("desc", StringComparison.OrdinalIgnoreCase) == true
                        ? query.OrderByDescending(x => x.Doctor.FirstName)
                               .ThenByDescending(x => x.Doctor.LastName)
                        : query.OrderBy(x => x.Doctor.FirstName)
                               .ThenBy(x => x.Doctor.LastName);
                }
                else
                {
                    query = query.ApplySorting(request.SortColumn, request.SortDirection);
                }
            }
            else
            {
                query = query.OrderByDescending(x => x.CreatedAt);
            }

            // Pagination
            var medicalRecords = await query
                .ApplyPagination(request.PageNumber, request.PageSize)
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
                })
                .ToListAsync();

            return new PagedResponse<MedicalRecordResponseModel>
            {
                IsSuccess = true,
                Message = "Medical records retrieved successfully.",
                Data = medicalRecords,
                TotalCount = totalCount,
                PageNumber = request.PageNumber,
                PageSize = request.PageSize
            };
        }
        public async Task<ResponseObj<MedicalRecordResponseModel>> GetMedicalRecordById(string id)
        {
            ResponseObj<MedicalRecordResponseModel> responseObj = new ResponseObj<MedicalRecordResponseModel>();
            var medicalRecord = await _unitOfWork.MedicalRecordRepository.GetWithoutTracking()
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
                    Gender = x.Patient.Gender,
                    Diagnosis = x.Diagnosis,
                    PatientPhone = x.Patient.Phone,
                    GeneticFrom = x.Patient.GeneticFrom,
                    Symptoms = x.Symptoms,
                    Treatment = x.Treatment,
                    Notes = x.Notes,
                    GeneticDiseases = x.Patient.PatientDiseases.Select(g => new GeneticDiseaseViewModel
                    {
                        DiseaseName = g.GeneticDisease.DiseaseName,
                        Medicines = g.Medicines,
                        ColorCode = g.GeneticDisease.ColorCode
                    }).ToList(),
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
                        Type = p.Type,
                        prescriptionItemRequests = p.Items.Select(pi => new PrescriptionItemRequest
                        {
                            Id = pi.Id,
                            MedicineName = pi.MedicineName,
                            Dosage = pi.Dosage,
                            Duration = pi.Duration,
                        }).ToList()
                    }).ToList(),

                }).FirstOrDefaultAsync();
            if (medicalRecord == null)
            {
                responseObj.IsSuccess = false;
                responseObj.Message = "No medical record found";
                return responseObj;
            }

            var patient = await _unitOfWork.PatientRepository.GetWithoutTracking()
                .Where(p => p.Id == medicalRecord.PatientId)
                .Select(p => new { p.DateOfBirth })
                .FirstOrDefaultAsync();

            if (patient != null)
            {
                var indianTime = _commonService.GetCurrentIndianTime();
                var age = indianTime.Year - patient.DateOfBirth.Year;
                if (patient.DateOfBirth.Date > indianTime.Date.AddYears(-age)) age--;
                medicalRecord.PatientAge = age;
            }

            var healthHistory = await _unitOfWork.HealthReportRepository.GetWithoutTracking()
                .Where(h => h.PatientId == medicalRecord.PatientId)
                .OrderByDescending(h => h.CreatedAt)
                .Take(3)
                .Select(h => new HealthHistoryDto
                {
                    Id = h.Id,
                    Weight = h.Weight,
                    Height = h.Height,
                    BloodPressure = h.BloodPressure,
                    SugarBeforeMeal = h.SugarBeforeMeal,
                    SugarAfterMeal = h.SugarAfterMeal,
                    CreatedAt = h.CreatedAt
                })
                .ToListAsync();

            medicalRecord.HealthHistory = healthHistory;

            responseObj.IsSuccess = true;
            responseObj.Message = "Medical record retrieved successfully";
            responseObj.Data = medicalRecord;
            return responseObj;
        }
        public async Task<ResponseObj<MedicalRecordResponseModel>> GetMedicalRecordForEdit(string id)
        {
            ResponseObj<MedicalRecordResponseModel> responseObj = new ResponseObj<MedicalRecordResponseModel>();
            var medicalRecord = await _unitOfWork.MedicalRecordRepository.GetWithTracking().Where(x => x.Id == id)
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
                    PatientPhone = x.Patient.Phone,
                    Notes = x.Notes,
                    Gender = x.Patient.Gender,
                    AppointmentDate = x.Appointment.AppointmentDate,
                    GeneticFrom = x.Patient.GeneticFrom,
                    GeneticDiseases = x.Patient.PatientDiseases.Select(g => new GeneticDiseaseViewModel
                    {
                        DiseaseName = g.GeneticDisease.DiseaseName,
                        Medicines = g.Medicines,
                        ColorCode = g.GeneticDisease.ColorCode
                    }).ToList(),
                }).FirstOrDefaultAsync();
            if (medicalRecord != null)
            {
                var healthRpt = await _unitOfWork.HealthReportRepository.GetWithoutTracking()
                    .Where(h => h.PatientId == medicalRecord.PatientId && h.CreatedAt.Date == _commonService.GetCurrentIndianTime().Date)
                    .OrderByDescending(d => d.CreatedAt).FirstOrDefaultAsync();
                if (healthRpt != null)
                {
                    medicalRecord.Weight = healthRpt.Weight != null ? healthRpt.Weight : "0";
                    medicalRecord.Height = healthRpt.Height != null ? healthRpt.Height : "0";
                    medicalRecord.BloodPressure = healthRpt.BloodPressure != null ? healthRpt.BloodPressure : "0";
                    medicalRecord.SugarBeforeMeal = healthRpt.SugarBeforeMeal != null ? healthRpt.SugarBeforeMeal : "0";
                    medicalRecord.SugarAfterMeal = healthRpt.SugarAfterMeal != null ? healthRpt.SugarAfterMeal : "0";
                }
            }

            if (medicalRecord == null)
            {
                responseObj.IsSuccess = false;
                responseObj.Message = "No medical record found";
                return responseObj;
            }
            responseObj.IsSuccess = true;
            responseObj.Message = "Medical record retrieved successfully";
            responseObj.Data = medicalRecord;
            return responseObj;
        }
        public async Task<ResponseObj<MedicalRecordResponseModel>> GetMedicalRecordByIdForPres(string id)
        {
            ResponseObj<MedicalRecordResponseModel> responseObj = new ResponseObj<MedicalRecordResponseModel>();
            var medicalRecord = await _unitOfWork.MedicalRecordRepository.GetWithoutTracking()
                .Where(x => x.Id == id)
                .Select(x => new MedicalRecordResponseModel
                {
                    Id = x.Id,
                    PatientId = x.PatientId,
                    DoctorId = x.DoctorId,
                    PatientName = x.Patient.FirstName + " " + x.Patient.LastName,
                    BloodGroup = x.Patient.BloodGroup,
                    AppointmentId = x.AppointmentId,
                    VisitDate = x.Appointment.AppointmentDate,
                    //VisitDate = x.VisitDate,
                    Gender = x.Patient.Gender,
                    PatientPhone = x.Patient.Phone,
                    GeneticFrom = x.Patient.GeneticFrom,                    
                    GeneticDiseases = x.Patient.PatientDiseases.Select(g => new GeneticDiseaseViewModel
                    {
                        DiseaseName = g.GeneticDisease.DiseaseName,
                        Medicines = g.Medicines,
                        ColorCode = g.GeneticDisease.ColorCode
                    }).ToList()

                }).FirstOrDefaultAsync();
            if (medicalRecord == null)
            {
                responseObj.IsSuccess = false;
                responseObj.Message = "No medical record found";
                return responseObj;
            }
            responseObj.IsSuccess = true;
            responseObj.Message = "Medical record retrieved successfully";
            responseObj.Data = medicalRecord;
            return responseObj;
        }
        public async Task<PagedResponse<HealthReportViewModel>> GetHealthReport(string patientId, PaginationRequest request)
        {
            var query = _unitOfWork.HealthReportRepository
                .GetWithoutTracking()
                .Where(x => x.PatientId == patientId);

            // Date Filter
            if (request.Filters != null &&
                request.Filters.TryGetValue("createdAt", out var createdAt) &&
                !string.IsNullOrWhiteSpace(createdAt) &&
                DateTime.TryParse(createdAt, out var selectedDate))
            {
                var fromDate = selectedDate.Date;
                var toDate = fromDate.AddDays(1);

                query = query.Where(x => x.CreatedAt >= fromDate &&
                                         x.CreatedAt < toDate);
            }

            var totalCount = await query.CountAsync();

            // Sorting
            if (!string.IsNullOrWhiteSpace(request.SortColumn))
            {
                query = query.ApplySorting(request.SortColumn, request.SortDirection);
            }
            else
            {
                query = query.OrderByDescending(x => x.CreatedAt);
            }

            var healthReports = await query
                .ApplyPagination(request.PageNumber, request.PageSize)
                .Select(x => new HealthReportViewModel
                {
                    Id = x.Id,
                    PatientName = x.Patient.FirstName + " " + x.Patient.LastName,
                    Weight = x.Weight,
                    Height = x.Height,
                    BP = x.BloodPressure,
                    SugarBeforeMeal = x.SugarBeforeMeal,
                    SugarAfterMeal = x.SugarAfterMeal,
                    CreatedAt = x.CreatedAt
                })
                .ToListAsync();

            return new PagedResponse<HealthReportViewModel>
            {
                IsSuccess = true,
                Message = "Health reports retrieved successfully.",
                Data = healthReports,
                TotalCount = totalCount,
                PageNumber = request.PageNumber,
                PageSize = request.PageSize
            };
        }
    }
}
