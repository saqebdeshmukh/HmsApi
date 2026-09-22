using BusinessLogic.CommonService;
using BusinessLogic.ExtensionMethods;
using BusinessLogic.UnitOfWork;
using DataAccess.DBEntities;
using DocumentFormat.OpenXml.InkML;
using DocumentFormat.OpenXml.Office2010.Excel;
using DocumentFormat.OpenXml.Wordprocessing;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Net.NetworkInformation;
using System.Text;
using ViewModels;

namespace BusinessLogic.PrescriptionService
{
    public class PrescriptionService : IPrescriptionService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ICommonService _commonService;
        public PrescriptionService(IUnitOfWork unitOfWork, ICommonService commonService)
        {
            _unitOfWork = unitOfWork;
            _commonService = commonService;
        }
        public async Task<ResponseObj<PrescriptionIdModel>> CreatePrescription(PrescriptionRequestModel prescriptionRequestModel)
        {
            ResponseObj<PrescriptionIdModel> responseObj = new ResponseObj<PrescriptionIdModel>();
            var internalPrescription = prescriptionRequestModel.InternalPrescriptionRequest == null ? null : CreatePrescriptionModel(prescriptionRequestModel.InternalPrescriptionRequest);
            var externalPrescription = prescriptionRequestModel.ExternalPrescriptionRequest == null ? null : CreatePrescriptionModel(prescriptionRequestModel.ExternalPrescriptionRequest);
            PrescriptionIdModel prescriptionIdModel = new PrescriptionIdModel();
            if (internalPrescription != null)
            {
                var medicalRecord = await _unitOfWork.MedicalRecordRepository.GetWithTracking().FirstOrDefaultAsync(x => x.Id == internalPrescription.MedicalRecordId);
                var appointment = await _unitOfWork.AppointmentRepository.GetWithTracking().FirstOrDefaultAsync(x => x.Id == medicalRecord.AppointmentId);
                if (appointment != null)
                {
                    prescriptionIdModel.InternalId = internalPrescription.Id;
                    appointment.Status = AppointmentStatus.Completed;
                    await _unitOfWork.AppointmentRepository.Update(appointment);
                    await _unitOfWork.PrescritpionRepository.Add(internalPrescription);
                }
            }
            if (externalPrescription != null)
            {
                var medicalRecordForExternal = await _unitOfWork.MedicalRecordRepository.GetWithTracking().FirstOrDefaultAsync(x => x.Id == externalPrescription.MedicalRecordId);
                var appointmentForExternal = await _unitOfWork.AppointmentRepository.GetWithTracking().FirstOrDefaultAsync(x => x.Id == medicalRecordForExternal.AppointmentId);
                if (appointmentForExternal != null)
                {
                    prescriptionIdModel.ExternalId = externalPrescription.Id;
                    appointmentForExternal.Status = AppointmentStatus.Completed;
                    await _unitOfWork.AppointmentRepository.Update(appointmentForExternal);
                    await _unitOfWork.PrescritpionRepository.Add(externalPrescription);
                }
            }
            await _unitOfWork.Complete();
            responseObj.Data = prescriptionIdModel;
            responseObj.Message = "Prescription added successfully";
            responseObj.IsSuccess = true;
            return responseObj;
        }
        public async Task<ResponseObj<string>> UpdatePrescription(PrescriptionRequest prescriptionRequestModel, string prescriptionId)
        {
            ResponseObj<string> responseObj = new ResponseObj<string>();

            var prescription = await _unitOfWork.PrescritpionRepository
                .GetWithTracking()
                .Include(x => x.Items)
                .FirstOrDefaultAsync(x => x.Id == prescriptionId);

            if (prescription == null)
            {
                responseObj.Message = "Prescription not found";
                responseObj.IsSuccess = false;
                return responseObj;
            }

            prescription.MedicalRecordId = prescriptionRequestModel.MedicalRecordId;
            prescription.PatientId = prescriptionRequestModel.PatientId;
            prescription.DoctorId = prescriptionRequestModel.DoctorId;
            prescription.Instructions = prescriptionRequestModel.Instructions;
            prescription.Type = prescriptionRequestModel.Type;

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
            await _unitOfWork.Complete();

            responseObj.IsSuccess = true;
            responseObj.Message = "Prescription updated successfully";
            responseObj.Data = prescription.Id;

            return responseObj;
        }
        public async Task<ResponseObj<List<PrescriptionViewModel>>> Prescriptions(PrescriptionQueryParameters? query = null)
        {
            ResponseObj<List<PrescriptionViewModel>> responseObj = new ResponseObj<List<PrescriptionViewModel>>();
            var q = query ?? new PrescriptionQueryParameters();

            var prescriptionQuery = _unitOfWork.PrescritpionRepository.GetWithoutTracking().AsQueryable();

            if (q.FromDate.HasValue)
            {
                prescriptionQuery = prescriptionQuery.Where(x => x.PrescriptionDate >= q.FromDate.Value);
            }
            if (q.ToDate.HasValue)
            {
                prescriptionQuery = prescriptionQuery.Where(x => x.PrescriptionDate <= q.ToDate.Value);
            }
            if (!string.IsNullOrEmpty(q.Search))
            {
                var search = q.Search.Trim().ToLower();
                prescriptionQuery = prescriptionQuery.Where(x => (x.Patient.FirstName + " " + x.Patient.LastName).ToLower().Contains(search)
                    || (x.Doctor.FirstName + " " + x.Doctor.LastName).ToLower().Contains(search)
                    || x.Items.Any(i => i.MedicineName.ToLower().Contains(search)));
            }

            var projected = prescriptionQuery.Select(s => new PrescriptionViewModel
            {
                Id = s.Id,
                DoctorId = s.DoctorId,
                DoctorName = s.Doctor.FirstName + " " + s.Doctor.LastName,
                PatientId = s.PatientId,
                PatientName = s.Patient.FirstName + " " + s.Patient.LastName,
                PrescriptionDate = s.PrescriptionDate,
                Type = s.Type,
                PrescriptionItemRequests = s.Items.Select(p => new PrescriptionItemRequest { MedicineName = p.MedicineName, Dosage = p.Dosage, Duration = p.Duration, MealTiming = p.MealTiming, Frequency = p.Frequency }).ToList(),
            });

            var sortBy = (q.SortBy ?? "PrescriptionDate").ToLower();
            var desc = (q.SortDir ?? "desc").ToLower() != "asc";
            switch (sortBy)
            {
                case "doctorname":
                    projected = desc ? projected.OrderByDescending(o => o.DoctorName) : projected.OrderBy(o => o.DoctorName);
                    break;
                case "patientname":
                    projected = desc ? projected.OrderByDescending(o => o.PatientName) : projected.OrderBy(o => o.PatientName);
                    break;
                default:
                    projected = desc ? projected.OrderByDescending(o => o.PrescriptionDate) : projected.OrderBy(o => o.PrescriptionDate);
                    break;
            }
            var totalCount = await prescriptionQuery.CountAsync();

            var skip = (q.PageNumber - 1) * q.PageSize;
            var prescriptionList = await projected.Skip(skip).Take(q.PageSize).ToListAsync();

            if (prescriptionList == null || prescriptionList.Count == 0)
            {
                responseObj.IsSuccess = false;
                responseObj.Message = "No prescription found";
                return responseObj;
            }

            responseObj.IsSuccess = true;
            responseObj.Message = "Prescription retrieved successfully";
            responseObj.Data = prescriptionList;
            return responseObj;
        }
        public async Task<PagedResponse<PrescriptionViewModel>> InternalPrescriptions(PaginationRequest request, string? doctorId = null)
        {
            var query = _unitOfWork.PrescritpionRepository
                .GetWithoutTracking()
                .Where(x => x.Type == "Internal");


            if (!string.IsNullOrEmpty(doctorId))
            {
                query = query.Where(x => x.DoctorId == doctorId);
            }


            query = query.ApplySearch(
                request.Search,
                x => x.Patient.FirstName,
                x => x.Patient.LastName,
                x => x.Patient.Phone);



            if (request.Filters != null)
            {
                if (request.Filters != null)
                {
                    if (request.Filters.TryGetValue("date", out var date))
                    {
                        if (DateTime.TryParse(date, out var prescriptionDate))
                        {
                            query = query.Where(x =>
                                x.PrescriptionDate.Date == prescriptionDate.Date);
                        }
                    }

                    if (request.Filters.TryGetValue("patientId", out var patientId))
                    {
                        if (!string.IsNullOrWhiteSpace(patientId))
                        {
                            query = query.Where(x => x.PatientId == patientId);
                        }
                    }

                    //if (request.Filters.TryGetValue("doctorId", out var doctorId))
                    //{
                    //    if (!string.IsNullOrWhiteSpace(doctorId))
                    //    {
                    //        query = query.Where(x => x.DoctorId == doctorId);
                    //    }
                    //}
                }
            }



            var totalCount = await query.CountAsync();
            if (!string.IsNullOrWhiteSpace(request.SortColumn))
            {
                switch (request.SortColumn.ToLower())
                {
                    case "patientname":

                        query = request.SortDirection == "desc"
                            ? query.OrderByDescending(x => x.Patient.FirstName)
                                   .ThenByDescending(x => x.Patient.LastName)
                            : query.OrderBy(x => x.Patient.FirstName)
                                   .ThenBy(x => x.Patient.LastName);

                        break;

                    case "doctorname":

                        query = request.SortDirection == "desc"
                            ? query.OrderByDescending(x => x.Doctor.FirstName)
                                   .ThenByDescending(x => x.Doctor.LastName)
                            : query.OrderBy(x => x.Doctor.FirstName)
                                   .ThenBy(x => x.Doctor.LastName);

                        break;

                    default:

                        query = query.ApplySorting(
                            request.SortColumn,
                            request.SortDirection);

                        break;
                }
            }
            else
            {
                query = query.OrderByDescending(x => x.PrescriptionDate);
            }

            var data = await query
                .ApplyPagination(
                    request.PageNumber,
                    request.PageSize)
                .Select(s => new PrescriptionViewModel
                {
                    Id = s.Id,

                    DoctorId = s.DoctorId,

                    DoctorName =
                      s.Doctor.FirstName + " " + s.Doctor.LastName,


                    PatientId = s.PatientId,

                    PatientName =
                      s.Patient.FirstName + " " + s.Patient.LastName,


                    PrescriptionDate = s.PrescriptionDate,

                    Type = s.Type,


                    PrescriptionItemRequests =
                      s.Items.Select(i => new PrescriptionItemRequest
                      {
                          MedicineName = i.MedicineName,
                          Dosage = i.Dosage,
                          Duration = i.Duration,
                          MealTiming = i.MealTiming,
                          Frequency = i.Frequency
                      }).ToList()

                })
                .ToListAsync();



            return new PagedResponse<PrescriptionViewModel>
            {
                IsSuccess = true,
                Message = "Internal prescriptions retrieved successfully",
                Data = data,
                TotalCount = totalCount,
                PageNumber = request.PageNumber,
                PageSize = request.PageSize
            };
        }
        public async Task<PagedResponse<PrescriptionViewModel>> ExternalPrescriptions(PaginationRequest request, string? doctorId = null)
        {
            var query = _unitOfWork.PrescritpionRepository
                .GetWithoutTracking()
                .Where(x => x.Type == "MedicalStore");

            if (!string.IsNullOrWhiteSpace(doctorId))
            {
                query = query.Where(x => x.DoctorId == doctorId);
            }

            query = query.ApplySearch(
                request.Search,
                x => x.Patient.FirstName,
                x => x.Patient.LastName,
                x => x.Doctor.FirstName,
                x => x.Doctor.LastName);

            if (request.Filters != null)
            {
                if (request.Filters.TryGetValue("date", out var date))
                {
                    if (DateTime.TryParse(date, out var visitDate))
                    {
                        query = query.Where(x =>
                            x.PrescriptionDate.Date == visitDate.Date);
                    }
                }

                if (request.Filters.TryGetValue("patientId", out var patientId))
                {
                    if (!string.IsNullOrWhiteSpace(patientId))
                    {
                        query = query.Where(x => x.PatientId == patientId);
                    }
                }

                if (request.Filters.TryGetValue("doctorId", out var doctorIdFilter))
                {
                    if (!string.IsNullOrWhiteSpace(doctorIdFilter))
                    {
                        query = query.Where(x => x.DoctorId == doctorIdFilter);
                    }
                }
            }

            var totalCount = await query.CountAsync();

            if (!string.IsNullOrWhiteSpace(request.SortColumn))
            {
                switch (request.SortColumn.ToLower())
                {
                    case "patientname":

                        query = request.SortDirection == "desc"
                            ? query.OrderByDescending(x => x.Patient.FirstName)
                                   .ThenByDescending(x => x.Patient.LastName)
                            : query.OrderBy(x => x.Patient.FirstName)
                                   .ThenBy(x => x.Patient.LastName);

                        break;

                    case "doctorname":

                        query = request.SortDirection == "desc"
                            ? query.OrderByDescending(x => x.Doctor.FirstName)
                                   .ThenByDescending(x => x.Doctor.LastName)
                            : query.OrderBy(x => x.Doctor.FirstName)
                                   .ThenBy(x => x.Doctor.LastName);

                        break;

                    default:

                        query = query.ApplySorting(
                            request.SortColumn,
                            request.SortDirection);

                        break;
                }
            }
            else
            {
                query = query.OrderByDescending(x => x.PrescriptionDate);
            }

            var prescriptions = await query
                .ApplyPagination(request.PageNumber, request.PageSize)
                .Select(s => new PrescriptionViewModel
                {
                    Id = s.Id,

                    DoctorId = s.DoctorId,

                    DoctorName = s.Doctor.FirstName + " " + s.Doctor.LastName,

                    PatientId = s.PatientId,

                    PatientName = s.Patient.FirstName + " " + s.Patient.LastName,

                    PrescriptionDate = s.PrescriptionDate,

                    Type = s.Type,

                    PrescriptionItemRequests = s.Items
                        .Select(i => new PrescriptionItemRequest
                        {
                            MedicineName = i.MedicineName,
                            Dosage = i.Dosage,
                            Duration = i.Duration,
                            MealTiming = i.MealTiming,
                            Frequency = i.Frequency
                        })
                        .ToList()
                })
                .ToListAsync();

            return new PagedResponse<PrescriptionViewModel>
            {
                IsSuccess = true,
                Message = "External prescriptions retrieved successfully.",

                Data = prescriptions,

                TotalCount = totalCount,

                PageNumber = request.PageNumber,

                PageSize = request.PageSize
            };
        }
        public async Task<ResponseObj<PrintViewModel>> PrescriptionById(string id)
        {
            ResponseObj<PrintViewModel> responseObj = new ResponseObj<PrintViewModel>();
            var prescription = await _unitOfWork.PrescritpionRepository.GetWithoutTracking()
                .Where(x => x.Id == id)
                .Select(s => new PrintViewModel
                {
                    Id = s.Id,
                    PatientPhoneNo = s.Patient.Phone,
                    Instructions = s.Instructions,
                    Diagnosis = s.MedicalRecord.Diagnosis,
                    PatientId = s.PatientId,
                    DoctorId = s.DoctorId,
                    MedicalRecordId = s.MedicalRecordId,
                    Specialization = s.Doctor.Specialization,
                    PrescriptionDate = s.PrescriptionDate,
                    Type = s.Type,
                    AppointmentId = s.MedicalRecord.AppointmentId,
                    DoctorName = s.Doctor.FirstName + " " + s.Doctor.LastName,
                    PatientName = s.Patient.FirstName + " " + s.Patient.LastName,
                    CreatedAt = s.PrescriptionDate,
                    GeneticFrom = s.Patient.GeneticFrom,
                    NextAppointment = _unitOfWork.AppointmentRepository.GetWithoutTracking()
                .Where(x => x.PatientId == s.PatientId && x.Status == AppointmentStatus.Scheduled && x.AppointmentDate > s.PrescriptionDate)
                .OrderBy(x => x.AppointmentDate)
                .Select(x => x.AppointmentDate)
                .FirstOrDefault(),
                    GeneticDiseases = s.Patient.PatientDiseases.Select(g => new GeneticDiseaseViewModel
                    {
                        DiseaseName = g.GeneticDisease.DiseaseName,
                        Medicines = g.Medicines,
                        ColorCode = g.GeneticDisease.ColorCode
                    }).ToList(),
                    prescriptionItemRequests = s.Items.Select(p => new PrescriptionItemRequest { MedicineName = p.MedicineName, Dosage = p.Dosage, Duration = p.Duration, MealTiming = p.MealTiming, Frequency = p.Frequency }).ToList(),
                })
                .OrderByDescending(o => o.CreatedAt)
                .FirstOrDefaultAsync();
            if (prescription == null)
            {
                responseObj.IsSuccess = false;
                responseObj.Message = "No prescription found";
                return responseObj;
            }

            responseObj.IsSuccess = true;
            responseObj.Message = "Prescription retrieved successfully";
            responseObj.Data = prescription;
            return responseObj;
        }
        public async Task<ResponseObj<PrintViewModel>> PrescriptionByIdForPrint(string id)
        {
            ResponseObj<PrintViewModel> responseObj = new ResponseObj<PrintViewModel>();
            var prescription = await _unitOfWork.PrescritpionRepository.GetWithoutTracking()
                .Where(x => x.Id == id)
                .Select(s => new PrintViewModel
                {
                    Id = s.Id,
                    PatientPhoneNo = s.Patient.Phone,
                    Instructions = s.Instructions,
                    Diagnosis = s.MedicalRecord.Diagnosis,
                    PatientId = s.PatientId,
                    DoctorId = s.DoctorId,
                    DoctorPhoneNo = s.Doctor.Phone,
                    ClinicAddress = s.Doctor.Address,
                    DoctorEmail = s.Doctor.Email,
                    MedicalRecordId = s.MedicalRecordId,
                    Specialization = s.Doctor.Specialization,
                    clinicName = s.Doctor.ClinicName,
                    PrescriptionDate = s.PrescriptionDate,
                    PatientAge = _commonService.GetCurrentIndianTime().Year - s.Patient.DateOfBirth.Year,
                    PatientGender = s.Patient.Gender,
                    DoctorDegree = s.Doctor.DoctorDegree,
                    RegNo = s.Doctor.RegNo,
                    Type = s.Type,
                    AppointmentId = s.MedicalRecord.AppointmentId,
                    DoctorName = s.Doctor.FirstName + " " + s.Doctor.LastName,
                    PatientName = s.Patient.FirstName + " " + s.Patient.LastName,
                    CreatedAt = s.PrescriptionDate,
                    Weight = s.MedicalRecord.HealthReport!=null? s.MedicalRecord.HealthReport.Weight : null,
                    BP = s.MedicalRecord.HealthReport!=null? s.MedicalRecord.HealthReport.BloodPressure : null,
                    SugarBeforeMeal = s.MedicalRecord.HealthReport!=null? s.MedicalRecord.HealthReport.SugarBeforeMeal : null,
                    SugarAfterMeal = s.MedicalRecord.HealthReport!=null? s.MedicalRecord.HealthReport.SugarAfterMeal : null,
                    prescriptionItemRequests = s.Items.Select(p => new PrescriptionItemRequest { MedicineName = p.MedicineName, Dosage = p.Dosage, Duration = p.Duration, MealTiming = p.MealTiming, Frequency = p.Frequency }).ToList(),
                })
                .OrderByDescending(o => o.CreatedAt)
                .FirstOrDefaultAsync();
            if (prescription == null)
            {
                responseObj.IsSuccess = false;
                responseObj.Message = "No prescription found";
                return responseObj;
            }
            prescription.NextAppointment = _unitOfWork.AppointmentRepository
     .GetWithoutTracking()
     .Where(x =>
         x.PatientId == prescription.PatientId &&
         x.Status == AppointmentStatus.Scheduled &&
         x.AppointmentDate > prescription.PrescriptionDate)
     .OrderBy(x => x.AppointmentDate)
     .Select(x => (DateTime?)x.AppointmentDate)
     .FirstOrDefault();
            responseObj.IsSuccess = true;
            responseObj.Message = "Prescription retrieved successfully";
            responseObj.Data = prescription;
            return responseObj;
        }
        public async Task<ResponseObj<List<PrintViewModel>>> GetPrescriptionsByPatientId(string patientId, string role)
        {
            ResponseObj<List<PrintViewModel>> responseObj = new ResponseObj<List<PrintViewModel>>();
            var query = _unitOfWork.PrescritpionRepository.GetWithoutTracking();
            if (role == "Receptionist")
            {
                query = query.Where(x => x.Type == "Internal");
            }
            var prescriptions = await query.Where(x => x.PatientId == patientId)
                .Select(s => new PrintViewModel
                {
                    Id = s.Id,
                    PatientPhoneNo = s.Patient.Phone,
                    Instructions = s.Instructions,
                    Diagnosis = s.MedicalRecord.Diagnosis,
                    PatientId = s.PatientId,
                    DoctorId = s.DoctorId,
                    Type = s.Type,
                    MedicalRecordId = s.MedicalRecordId,
                    Specialization = s.Doctor.Specialization,
                    PrescriptionDate = s.PrescriptionDate,
                    AppointmentId = s.MedicalRecord.AppointmentId,
                    DoctorName = s.Doctor.FirstName + " " + s.Doctor.LastName,
                    CreatedAt = s.PrescriptionDate,
                    prescriptionItemRequests = s.Items.Select(p => new PrescriptionItemRequest { MedicineName = p.MedicineName, Dosage = p.Dosage, Duration = p.Duration, MealTiming = p.MealTiming, Frequency = p.Frequency }).ToList(),
                })
                .OrderByDescending(o => o.CreatedAt)
                .ToListAsync();
            if (prescriptions == null || prescriptions.Count == 0)
            {
                responseObj.IsSuccess = false;
                responseObj.Message = "No prescription found";
                return responseObj;
            }
            responseObj.IsSuccess = true;
            responseObj.Message = "Prescriptions retrieved successfully";
            responseObj.Data = prescriptions;
            return responseObj;
        }        
        public async Task<ResponseObj<string>> UpdateInternalPrescriptionStatus(string prescriptionId)
        {
            var prescription = await _unitOfWork.PrescritpionRepository
                .GetWithTracking()
                .FirstOrDefaultAsync(p => p.Id == prescriptionId);
            if (prescription == null)
            {
                return new ResponseObj<string>
                {
                    IsSuccess = false,
                    Message = "Prescription not found"
                };
            }
            prescription.IsGiven = true;
            await _unitOfWork.PrescritpionRepository.Update(prescription);
            await _unitOfWork.Complete();
            return new ResponseObj<string>
            {
                IsSuccess = true,
                Message = "Prescription status updated successfully"
            };
        }
        public async Task<ResponseObj<List<QuickPrescriptionMedicineViewModel>>> GetAddQuickPrescription(string Type)
        {
            ResponseObj<List<QuickPrescriptionMedicineViewModel>> responseObj = new ResponseObj<List<QuickPrescriptionMedicineViewModel>>();
            var medicines = await _unitOfWork.PrescritpionRepository
                    .GetWithoutTracking()
                    .Where(x => x.Type == Type)
                    .SelectMany(x => x.Items)
                    .GroupBy(x => x.MedicineName.ToUpper())
                    .Select(g => new QuickPrescriptionMedicineViewModel
                    {
                        MedicineName = g.First().MedicineName,
                        Dosage = g.First().Dosage,
                        Duration = g.First().Duration,
                        Frequency = g.First().Frequency.ToString(),
                        MealTiming = g.First().MealTiming,
                        PrescriptionCount = g.Count()
                    })
                    .OrderByDescending(x => x.PrescriptionCount)
                    .Take(15)
                    .ToListAsync();
            if (!medicines.Any())
            {
                responseObj.IsSuccess = false;
                responseObj.Message = "No medicines found.";
                return responseObj;
            }

            responseObj.IsSuccess = true;
            responseObj.Message = "Medicines retrieved successfully.";
            responseObj.Data = medicines;
            return responseObj;
        }
        private Prescription CreatePrescriptionModel(PrescriptionRequest prescriptionRequestModel)
        {
            Prescription prescription = new Prescription();
            prescription.Id = Guid.NewGuid().ToString();
            prescription.MedicalRecordId = prescriptionRequestModel.MedicalRecordId;
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
    }
}
