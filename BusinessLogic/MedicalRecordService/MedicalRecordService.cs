using Azure.Core;
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

        public async Task<ResponseObj<PrescriptionIdModel>> CreateMedicalRecord(MedicalAndPrescriptionModel medicalAndPrescriptionModel)
        {
            ResponseObj<PrescriptionIdModel> responseObj = new ResponseObj<PrescriptionIdModel>();

            var request = medicalAndPrescriptionModel.medicalRecordRequestModel;

            // ---------------------------------------------------------
            // 1. Validate appointment
            // ---------------------------------------------------------

            var appointment = await _unitOfWork.AppointmentRepository
                .GetWithoutTracking()
                .Where(x => x.PatientId == request.PatientId)
                .FirstOrDefaultAsync();

            if (appointment == null)
            {
                responseObj.IsSuccess = false;
                responseObj.Message = "No appointment found for this patient";
                return responseObj;
            }

            // ---------------------------------------------------------
            // 2. Create Medical Record
            // ---------------------------------------------------------

            MedicalRecord medicalRecord = new MedicalRecord
            {
                Id = Guid.NewGuid().ToString(),
                VisitDate = _commonService.GetCurrentIndianTime(),
                AppointmentId = request.AppointmentId,
                Treatment = request.Treatment,
                Diagnosis = request.Diagnosis,
                DoctorId = request.DoctorId,
                PatientId = request.PatientId,
                Symptoms = request.Symptoms,
                Notes = request.Notes,
                CreatedAt = _commonService.GetCurrentIndianTime()
            };

            await _unitOfWork.MedicalRecordRepository.Add(medicalRecord);

            // ---------------------------------------------------------
            // Health Report
            // ---------------------------------------------------------

            bool HasValue(string? value)
            {
                return !string.IsNullOrWhiteSpace(value)
                       && value.Trim() != "0";
            }

            bool hasDoctorHealthReportData =
                HasValue(request.Weight) ||
                HasValue(request.Height) ||
                HasValue(request.BloodPressure) ||
                HasValue(request.SugarBeforeMeal) ||
                HasValue(request.SugarAfterMeal);


            // Find HealthReport belonging to THIS appointment only.
            // This prevents a previous medical record/report of the
            // same patient from being selected.
            var existingHealthReport = await _unitOfWork.HealthReportRepository
                .GetWithTracking()
                .Where(x =>
                    x.PatientId == request.PatientId &&
                    x.AppointmentId == request.AppointmentId &&
                    x.MedicalRecordId == null)
                .FirstOrDefaultAsync();


            if (existingHealthReport != null)
            {
                // -----------------------------------------------------
                // Receptionist already created the HealthReport
                // for this appointment.
                //
                // Now link it to the CURRENT MedicalRecord.
                // -----------------------------------------------------

                existingHealthReport.MedicalRecordId = medicalRecord.Id;

                // Update only values entered by doctor.
                // Do not overwrite receptionist values with 0/null/empty.

                if (HasValue(request.Weight))
                {
                    existingHealthReport.Weight = request.Weight;
                }

                if (HasValue(request.Height))
                {
                    existingHealthReport.Height = request.Height;
                }

                if (HasValue(request.BloodPressure))
                {
                    existingHealthReport.BloodPressure = request.BloodPressure;
                }

                if (HasValue(request.SugarBeforeMeal))
                {
                    existingHealthReport.SugarBeforeMeal =
                        request.SugarBeforeMeal;
                }

                if (HasValue(request.SugarAfterMeal))
                {
                    existingHealthReport.SugarAfterMeal =
                        request.SugarAfterMeal;
                }

                await _unitOfWork.HealthReportRepository
                    .Update(existingHealthReport);
            }
            else if (hasDoctorHealthReportData)
            {
                // -----------------------------------------------------
                // Receptionist did not create HealthReport.
                //
                // Doctor has entered health data.
                // Create HealthReport for THIS appointment and
                // CURRENT MedicalRecord.
                // -----------------------------------------------------

                var healthReport = new HealthReport
                {
                    PatientId = request.PatientId,
                    AppointmentId = request.AppointmentId,
                    MedicalRecordId = medicalRecord.Id,

                    Weight = request.Weight,
                    Height = request.Height,
                    BloodPressure = request.BloodPressure,
                    SugarBeforeMeal = request.SugarBeforeMeal,
                    SugarAfterMeal = request.SugarAfterMeal,

                    CreatedAt = _commonService.GetCurrentIndianTime()
                };

                await _unitOfWork.HealthReportRepository
                    .Add(healthReport);
            }

            // If:
            // - no receptionist HealthReport exists for this appointment
            // - AND doctor did not enter any health value
            //
            // Do nothing.

            // ---------------------------------------------------------
            // 4. Create prescriptions
            // ---------------------------------------------------------

            var internalPrescription =
                medicalAndPrescriptionModel.prescriptionRequestModel
                    .InternalPrescriptionRequest == null
                        ? null
                        : CreatePrescriptionModel(
                            medicalAndPrescriptionModel.prescriptionRequestModel
                                .InternalPrescriptionRequest,
                            medicalRecord.Id);

            var externalPrescription =
                medicalAndPrescriptionModel.prescriptionRequestModel
                    .ExternalPrescriptionRequest == null
                        ? null
                        : CreatePrescriptionModel(
                            medicalAndPrescriptionModel.prescriptionRequestModel
                                .ExternalPrescriptionRequest,
                            medicalRecord.Id);

            PrescriptionIdModel prescriptionIdModel =
                new PrescriptionIdModel();

            // ---------------------------------------------------------
            // 5. Internal prescription
            // ---------------------------------------------------------

            if (internalPrescription != null)
            {
                var appointmentStatus = await _unitOfWork.AppointmentRepository
                    .GetWithTracking()
                    .FirstOrDefaultAsync(x => x.Id == medicalRecord.AppointmentId);

                if (appointmentStatus != null)
                {
                    prescriptionIdModel.InternalId = internalPrescription.Id;

                    appointmentStatus.Status = AppointmentStatus.Completed;

                    await _unitOfWork.AppointmentRepository
                        .Update(appointmentStatus);

                    await _unitOfWork.PrescritpionRepository
                        .Add(internalPrescription);
                }
            }

            // ---------------------------------------------------------
            // 6. External / Medical Store prescription
            // ---------------------------------------------------------

            if (externalPrescription != null)
            {
                var appointmentForExternal =
                    await _unitOfWork.AppointmentRepository
                        .GetWithTracking()
                        .FirstOrDefaultAsync(x => x.Id == medicalRecord.AppointmentId);

                if (appointmentForExternal != null)
                {
                    prescriptionIdModel.ExternalId = externalPrescription.Id;

                    appointmentForExternal.Status =
                        AppointmentStatus.Completed;

                    await _unitOfWork.AppointmentRepository
                        .Update(appointmentForExternal);

                    await _unitOfWork.PrescritpionRepository
                        .Add(externalPrescription);
                }
            }

            // ---------------------------------------------------------
            // 7. Update appointment status
            // ---------------------------------------------------------

            var appointmentToUpdate =
                await _unitOfWork.AppointmentRepository
                    .GetWithTracking()
                    .FirstOrDefaultAsync(x => x.Id == medicalRecord.AppointmentId);

            if (appointmentToUpdate != null)
            {
                appointmentToUpdate.Status =
                    AppointmentStatus.MedicalRecordCreated;

                await _unitOfWork.AppointmentRepository
                    .Update(appointmentToUpdate);
            }

            // ---------------------------------------------------------
            // 8. Save everything
            // ---------------------------------------------------------

            await _unitOfWork.Complete();

            responseObj.Data = prescriptionIdModel;
            responseObj.IsSuccess = true;
            responseObj.Message = "Medical record created successfully";

            return responseObj;
        }

        //public async Task<ResponseObj<PrescriptionIdModel>> CreateMedicalRecord(MedicalAndPrescriptionModel medicalAndPrescriptionModel)
        //{
        //    ResponseObj<PrescriptionIdModel> responseObj = new ResponseObj<PrescriptionIdModel>();
        //    MedicalRecord medicalRecord = new MedicalRecord();
        //    var appointments = await _unitOfWork.AppointmentRepository.GetWithoutTracking().Where(x => x.PatientId == medicalAndPrescriptionModel.medicalRecordRequestModel.PatientId).FirstOrDefaultAsync();
        //    if (appointments == null)
        //    {
        //        responseObj.IsSuccess = false;
        //        responseObj.Message = "No appointment found for this patient";
        //        return responseObj;
        //    }
        //    medicalRecord.Id = Guid.NewGuid().ToString();
        //    medicalRecord.VisitDate = _commonService.GetCurrentIndianTime();
        //    medicalRecord.AppointmentId = medicalAndPrescriptionModel.medicalRecordRequestModel.AppointmentId;
        //    medicalRecord.Treatment = medicalAndPrescriptionModel.medicalRecordRequestModel.Treatment;
        //    medicalRecord.Diagnosis = medicalAndPrescriptionModel.medicalRecordRequestModel.Diagnosis;
        //    medicalRecord.DoctorId = medicalAndPrescriptionModel.medicalRecordRequestModel.DoctorId;
        //    medicalRecord.PatientId = medicalAndPrescriptionModel.medicalRecordRequestModel.PatientId;
        //    medicalRecord.Symptoms = medicalAndPrescriptionModel.medicalRecordRequestModel.Symptoms;
        //    medicalRecord.Notes = medicalAndPrescriptionModel.medicalRecordRequestModel.Notes;
        //    medicalRecord.CreatedAt = _commonService.GetCurrentIndianTime();
        //    var request = medicalAndPrescriptionModel.medicalRecordRequestModel;

        //    bool HasValue(string? value)
        //    {
        //        return !string.IsNullOrWhiteSpace(value)
        //               && value.Trim() != "0";
        //    }

        //    bool hasHealthReportData =
        //        HasValue(request.Weight) ||
        //        HasValue(request.Height) ||
        //        HasValue(request.BloodPressure) ||
        //        HasValue(request.SugarBeforeMeal) ||
        //        HasValue(request.SugarAfterMeal);

        //    if (hasHealthReportData)
        //    {
        //        HealthReport healthReport = new HealthReport
        //        {
        //            PatientId = medicalAndPrescriptionModel.medicalRecordRequestModel.PatientId,
        //            Weight = medicalAndPrescriptionModel.medicalRecordRequestModel.Weight,
        //            Height = medicalAndPrescriptionModel.medicalRecordRequestModel.Height,
        //            BloodPressure = medicalAndPrescriptionModel.medicalRecordRequestModel.BloodPressure,
        //            SugarBeforeMeal = medicalAndPrescriptionModel.medicalRecordRequestModel.SugarBeforeMeal,
        //            SugarAfterMeal = medicalAndPrescriptionModel.medicalRecordRequestModel.SugarAfterMeal,
        //            CreatedAt = _commonService.GetCurrentIndianTime()
        //        };

        //        await _unitOfWork.HealthReportRepository.Add(healthReport);
        //    }

        //    await _unitOfWork.MedicalRecordRepository.Add(medicalRecord);
        //    var internalPrescription = medicalAndPrescriptionModel.prescriptionRequestModel.InternalPrescriptionRequest == null ? null : CreatePrescriptionModel(medicalAndPrescriptionModel.prescriptionRequestModel.InternalPrescriptionRequest, medicalRecord.Id);
        //    var externalPrescription = medicalAndPrescriptionModel.prescriptionRequestModel.ExternalPrescriptionRequest == null ? null : CreatePrescriptionModel(medicalAndPrescriptionModel.prescriptionRequestModel.ExternalPrescriptionRequest, medicalRecord.Id);
        //    PrescriptionIdModel prescriptionIdModel = new PrescriptionIdModel();
        //    if (internalPrescription != null)
        //    {
        //        var appointmentStatus = await _unitOfWork.AppointmentRepository.GetWithTracking().FirstOrDefaultAsync(x => x.Id == medicalRecord.AppointmentId);
        //        if (appointmentStatus != null)
        //        {
        //            prescriptionIdModel.InternalId = internalPrescription.Id;
        //            appointmentStatus.Status = AppointmentStatus.Completed;
        //            await _unitOfWork.AppointmentRepository.Update(appointmentStatus);
        //            await _unitOfWork.PrescritpionRepository.Add(internalPrescription);
        //        }
        //    }
        //    if (externalPrescription != null)
        //    {
        //        var appointmentForExternal = await _unitOfWork.AppointmentRepository.GetWithTracking().FirstOrDefaultAsync(x => x.Id == medicalRecord.AppointmentId);
        //        if (appointmentForExternal != null)
        //        {
        //            prescriptionIdModel.ExternalId = externalPrescription.Id;
        //            appointmentForExternal.Status = AppointmentStatus.Completed;
        //            await _unitOfWork.AppointmentRepository.Update(appointmentForExternal);
        //            await _unitOfWork.PrescritpionRepository.Add(externalPrescription);
        //        }
        //    }
        //    var appointment = await _unitOfWork.AppointmentRepository.GetWithTracking().Where(x => x.Id == medicalRecord.AppointmentId).FirstOrDefaultAsync();
        //    appointment.Status = AppointmentStatus.MedicalRecordCreated;
        //    await _unitOfWork.AppointmentRepository.Update(appointment);
        //    await _unitOfWork.Complete();
        //    responseObj.Data = prescriptionIdModel;
        //    responseObj.IsSuccess = true;
        //    responseObj.Message = "Medical record created successfully";
        //    return responseObj;
        //}
        public async Task<ResponseObj<string>> UpdateMedicalRecord(MedicalAndPrescriptionModel medicalAndPrescriptionModel, string medicalRecordId)
        {
            ResponseObj<string> responseObj = new ResponseObj<string>();
            var healthRpt = await _unitOfWork.HealthReportRepository.GetWithoutTracking()
                .Where(h => h.PatientId == medicalAndPrescriptionModel.medicalRecordRequestModel.PatientId && h.CreatedAt.Date == _commonService.GetCurrentIndianTime().Date)
                .OrderByDescending(d => d.CreatedAt).FirstOrDefaultAsync();
            var medicalRecord = await _unitOfWork.MedicalRecordRepository.GetWithoutTracking().Where(x => x.Id == medicalRecordId).FirstOrDefaultAsync();
            if (medicalRecord == null)
            {
                responseObj.IsSuccess = false;
                responseObj.Message = "Medical record not found";
                return responseObj;
            }
            medicalRecord.Treatment = medicalAndPrescriptionModel.medicalRecordRequestModel.Treatment;
            medicalRecord.Diagnosis = medicalAndPrescriptionModel.medicalRecordRequestModel.Diagnosis;
            medicalRecord.DoctorId = medicalAndPrescriptionModel.medicalRecordRequestModel.DoctorId;
            medicalRecord.PatientId = medicalAndPrescriptionModel.medicalRecordRequestModel.PatientId;
            medicalRecord.Symptoms = medicalAndPrescriptionModel.medicalRecordRequestModel.Symptoms;
            medicalRecord.Notes = medicalAndPrescriptionModel.medicalRecordRequestModel.Notes;
            var request = medicalAndPrescriptionModel.medicalRecordRequestModel;

            bool HasValue(string? value)
            {
                return !string.IsNullOrWhiteSpace(value)
                       && value.Trim() != "0";
            }

            bool hasHealthReportData =
                HasValue(request.Weight) ||
                HasValue(request.Height) ||
                HasValue(request.BloodPressure) ||
                HasValue(request.SugarBeforeMeal) ||
                HasValue(request.SugarAfterMeal);

            if (hasHealthReportData)
            {
                if (healthRpt == null)
                {
                    var healthReport = new HealthReport
                    {
                        PatientId = request.PatientId,
                        Weight = request.Weight,
                        Height = request.Height,
                        BloodPressure = request.BloodPressure,
                        SugarBeforeMeal = request.SugarBeforeMeal,
                        SugarAfterMeal = request.SugarAfterMeal,
                        CreatedAt = _commonService.GetCurrentIndianTime()
                    };

                    await _unitOfWork.HealthReportRepository.Add(healthReport);
                }
                else
                {
                    bool isSameHealthReport =
                        (healthRpt.Weight ?? "") == (request.Weight ?? "") &&
                        (healthRpt.Height ?? "") == (request.Height ?? "") &&
                        (healthRpt.BloodPressure ?? "") == (request.BloodPressure ?? "") &&
                        (healthRpt.SugarAfterMeal ?? "") == (request.SugarAfterMeal ?? "") &&
                        (healthRpt.SugarBeforeMeal ?? "") == (request.SugarBeforeMeal ?? "");

                    if (!isSameHealthReport)
                    {
                        healthRpt.Weight = request.Weight;
                        healthRpt.Height = request.Height;
                        healthRpt.BloodPressure = request.BloodPressure;
                        healthRpt.SugarBeforeMeal = request.SugarBeforeMeal;
                        healthRpt.SugarAfterMeal = request.SugarAfterMeal;

                        await _unitOfWork.HealthReportRepository.Update(healthRpt);
                    }
                }
            }
            await _unitOfWork.MedicalRecordRepository.Update(medicalRecord);
            var internalRequest = medicalAndPrescriptionModel.prescriptionRequestModel?.InternalPrescriptionRequest;

            if (internalRequest != null)
            {
                await UpdatePrescription(internalRequest, medicalRecordId);
            }
            var externalRequest = medicalAndPrescriptionModel.prescriptionRequestModel?.ExternalPrescriptionRequest;
            if (externalRequest != null)
            {
                await UpdatePrescription(externalRequest, medicalRecordId);
            }
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
                        PrescriptionItemRequests = p.Items.Select(pi => new PrescriptionItemRequest
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
                    Prescriptions = x.Prescriptions
                .Select(p => new PrescriptionViewModel
                {
                    Id = p.Id,
                    MedicalRecordId = p.MedicalRecordId,
                    PatientId = p.PatientId,
                    PrescriptionDate = p.PrescriptionDate,
                    DoctorId = p.DoctorId,
                    Instructions = p.Instructions,
                    Type = p.Type,
                    PrescriptionItemRequests = p.Items
                        .Select(i => new PrescriptionItemRequest
                        {
                            Id = i.Id,
                            MedicineName = i.MedicineName,
                            Dosage = i.Dosage,
                            Frequency = i.Frequency,
                            MealTiming = i.MealTiming,
                            Duration = i.Duration
                        })
                        .ToList()
                })
                .ToList()
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
        public async Task<ResponseObj<List<HistoryModel>>> GetMedicalHistory(string patientId)
        {
            ResponseObj<List<HistoryModel>> responseObj = new ResponseObj<List<HistoryModel>>();

            var medicalHistory = await _unitOfWork.MedicalRecordRepository
                .GetWithoutTracking()
                .Where(x => x.PatientId == patientId)
                .OrderByDescending(x => x.VisitDate)
                .Select(x => new HistoryModel
                {
                    MedicalRecordId = x.Id,
                    VisitDate = x.VisitDate,
                    Diagnosis = x.Diagnosis,
                    Symptoms = x.Symptoms,
                    Treatment = x.Treatment ?? string.Empty,

                    Prescriptions = x.Prescriptions
                        .Select(p => new PrescriptionViewModel
                        {
                            Id = p.Id,
                            MedicalRecordId = p.MedicalRecordId,
                            PatientId = p.PatientId,
                            PrescriptionDate = p.PrescriptionDate,
                            DoctorId = p.DoctorId,
                            Instructions = p.Instructions,
                            Type = p.Type,

                            PrescriptionItemRequests = p.Items
                                .Where(i => i.PrescriptionId == p.Id)
                                .Select(i => new PrescriptionItemRequest
                                {
                                    Id = i.Id,
                                    MedicineName = i.MedicineName,
                                    Dosage = i.Dosage ?? string.Empty,
                                    Frequency = i.Frequency,
                                    MealTiming = i.MealTiming ?? string.Empty,
                                    Duration = i.Duration ?? string.Empty
                                })
                                .ToList()
                        })
                        .ToList()
                })
                .ToListAsync();

            responseObj.Data = medicalHistory;
            responseObj.IsSuccess = true;

            return responseObj;
        }
        public async Task<ResponseObj<string>> DuplicateRecord(string medicalRecordId, string appointmentId)
        {
            ResponseObj<string> responseObj = new ResponseObj<string>();
            var existingRecord = await _unitOfWork.MedicalRecordRepository.GetWithoutTracking()
                .FirstOrDefaultAsync(x => x.Id == medicalRecordId);
            if (existingRecord == null)
            {
                responseObj.IsSuccess = false;
                responseObj.Message = "Medical record not found";
                return responseObj;
            }
            var newRecord = new MedicalRecord
            {
                Id = Guid.NewGuid().ToString(),
                VisitDate = _commonService.GetCurrentIndianTime(),
                AppointmentId = appointmentId,
                Treatment = existingRecord.Treatment,
                Diagnosis = existingRecord.Diagnosis,
                DoctorId = existingRecord.DoctorId,
                PatientId = existingRecord.PatientId,
                Symptoms = existingRecord.Symptoms,
                Notes = existingRecord.Notes,
                CreatedAt = _commonService.GetCurrentIndianTime()
            };
            await _unitOfWork.MedicalRecordRepository.Add(newRecord);
            await _unitOfWork.Complete();
            // Duplicate any prescriptions (and their items) associated with the existing record
            var existingPrescriptions = await _unitOfWork.PrescritpionRepository.GetWithoutTracking()
                .Include(p => p.Items)
                .Where(p => p.MedicalRecordId == existingRecord.Id)
                .ToListAsync();

            foreach (var pres in existingPrescriptions)
            {
                var newPres = new Prescription
                {
                    Id = Guid.NewGuid().ToString(),
                    MedicalRecordId = newRecord.Id,
                    PatientId = pres.PatientId,
                    DoctorId = pres.DoctorId,
                    Instructions = pres.Instructions,
                    Type = pres.Type,
                    PrescriptionDate = _commonService.GetCurrentIndianTime(),
                    Items = new List<PrescriptionItem>()
                };

                if (pres.Items != null && pres.Items.Any())
                {
                    foreach (var item in pres.Items)
                    {
                        var newItem = new PrescriptionItem
                        {
                            Id = Guid.NewGuid().ToString(),
                            PrescriptionId = newPres.Id,
                            MedicineName = item.MedicineName,
                            Dosage = item.Dosage,
                            Frequency = item.Frequency,
                            MealTiming = item.MealTiming,
                            Duration = item.Duration
                        };
                        newPres.Items.Add(newItem);
                    }
                }

                await _unitOfWork.PrescritpionRepository.Add(newPres);
            }

            await _unitOfWork.Complete();
            responseObj.IsSuccess = true;
            responseObj.Message = "Medical record duplicated successfully";
            responseObj.Data = newRecord.Id;
            return responseObj;
        }
        public async Task<ResponseObj<List<PatientAllergy>>> CreatePatientAllergy(PatientAllergy patientAllergy)
        {
            ResponseObj<List<PatientAllergy>> responseObj = new ResponseObj<List<PatientAllergy>>();
            patientAllergy.Id = 0;
            await _unitOfWork.PatientAllergyRepository.Add(patientAllergy);
            await _unitOfWork.Complete();
            var allergies = await _unitOfWork.PatientAllergyRepository.GetWithoutTracking()
                .Where(x => x.PatientId == patientAllergy.PatientId)
                .ToListAsync();
            responseObj.Data = allergies;
            responseObj.IsSuccess = true;
            responseObj.Message = "Patient allergy added successfully";
            return responseObj;
        }
        public async Task<ResponseObj<List<PatientAllergy>>> RemovePatientAllergy(int allergyId, string patientId)
        {
            ResponseObj<List<PatientAllergy>> responseObj = new ResponseObj<List<PatientAllergy>>();
            var allergy = await _unitOfWork.PatientAllergyRepository.GetWithTracking()
                .FirstOrDefaultAsync(x => x.Id == allergyId && x.PatientId == patientId);
            if (allergy != null)
            {
                _unitOfWork.PatientAllergyRepository.Remove(allergy);
                await _unitOfWork.Complete();
                var allergies = await _unitOfWork.PatientAllergyRepository.GetWithoutTracking()
                    .Where(x => x.PatientId == patientId)
                    .ToListAsync();
                responseObj.Data = allergies;
                responseObj.IsSuccess = true;
                responseObj.Message = "Patient allergy removed successfully";
            }
            else
            {
                responseObj.IsSuccess = false;
                responseObj.Message = "Patient allergy not found";
            }
            return responseObj;
        }
        public async Task<ResponseObj<List<PatientAllergy>>> GetAllergiesByPatientId(string patientId)
        {
            ResponseObj<List<PatientAllergy>> responseObj = new ResponseObj<List<PatientAllergy>>();
            var allergies = await _unitOfWork.PatientAllergyRepository.GetWithoutTracking()
                .Where(x => x.PatientId == patientId)
                .ToListAsync();
            responseObj.Data = allergies;
            responseObj.IsSuccess = true;
            responseObj.Message = "Patient allergies retrieved successfully";
            return responseObj;
        }

        private Prescription CreatePrescriptionModel(PrescriptionRequest prescriptionRequestModel, string medicalRecordId)
        {
            Prescription prescription = new Prescription();
            prescription.Id = Guid.NewGuid().ToString();
            prescription.MedicalRecordId = medicalRecordId;
            prescription.PatientId = prescriptionRequestModel.PatientId;
            prescription.DoctorId = prescriptionRequestModel.DoctorId;
            prescription.Instructions = prescriptionRequestModel.Instructions;
            prescription.Type = prescriptionRequestModel.Type;
            prescription.PrescriptionDate = _commonService.GetCurrentIndianTime();
            prescription.Items = new List<PrescriptionItem>();
            foreach (var item in prescriptionRequestModel.prescriptionItemRequests)
            {
                PrescriptionItem prescriptionItem = new PrescriptionItem();
                prescriptionItem.Id = Guid.NewGuid().ToString();
                prescriptionItem.PrescriptionId = prescription.Id;
                prescriptionItem.MedicineName = item.MedicineName;
                prescriptionItem.Dosage = item.Dosage;
                prescriptionItem.Frequency = item.Frequency;
                prescriptionItem.MealTiming = item.MealTiming;
                prescriptionItem.Duration = item.Duration;
                prescription.Items.Add(prescriptionItem);
            }
            return prescription;
        }
        private async Task UpdatePrescription(PrescriptionRequest prescriptionRequestModel, string medicalRecordId)
        {
            var prescription = await _unitOfWork.PrescritpionRepository.GetWithTracking().Include(x => x.Items)
                .FirstOrDefaultAsync(p => p.Id == prescriptionRequestModel.Id && p.MedicalRecordId == prescriptionRequestModel.MedicalRecordId);
            if (prescription != null)
            {
                prescription.MedicalRecordId = medicalRecordId;
                prescription.PatientId = prescriptionRequestModel.PatientId;
                prescription.DoctorId = prescriptionRequestModel.DoctorId;
                prescription.Instructions = prescriptionRequestModel.Instructions;
                prescription.Type = prescriptionRequestModel.Type;
                // Remove existing items
                if (prescription.Items.Any())
                {
                    _unitOfWork.PrescriptionItemRepository.RemoveRange(prescription.Items);
                }
                // Add new items
                prescription.Items = prescriptionRequestModel.prescriptionItemRequests
                    .Select(item => new PrescriptionItem
                    {
                        Id = Guid.NewGuid().ToString(),
                        PrescriptionId = prescription.Id,
                        MedicineName = item.MedicineName,
                        Dosage = item.Dosage,
                        Frequency = item.Frequency,
                        MealTiming = item.MealTiming,
                        Duration = item.Duration
                    })
                    .ToList();
                await _unitOfWork.PrescritpionRepository.Update(prescription);
            }
            else
            {
                prescription = CreatePrescriptionModel(
                    prescriptionRequestModel,
                    medicalRecordId);

                await _unitOfWork.PrescritpionRepository
                    .Add(prescription);

                return;
            }
        }
    }
}
