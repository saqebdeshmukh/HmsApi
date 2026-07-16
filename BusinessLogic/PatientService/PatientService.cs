using BusinessLogic.CommonService;
using BusinessLogic.ExtensionMethods;
using BusinessLogic.UnitOfWork;
using DataAccess.DBEntities;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using ViewModels;

namespace BusinessLogic.PatientService
{
    public class PatientService : IPatientService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ICommonService _commonService;
        public PatientService(IUnitOfWork unitOfWork, ICommonService commonService)
        {
            _unitOfWork = unitOfWork;
            _commonService = commonService;
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
            patient.GeneticFrom = patientRequestModel.GeneticFrom;
            HealthReport healthReport = new HealthReport();
            healthReport.PatientId = patient.Id;
            healthReport.BloodPressure = patientRequestModel.BloodPressure;
            healthReport.SugarAfterMeal = patientRequestModel.SugarAfterMeal;
            healthReport.SugarBeforeMeal = patientRequestModel.SugarBeforeMeal;
            healthReport.CreatedAt = _commonService.GetCurrentIndianTime();
            healthReport.Height = patientRequestModel.Height;
            healthReport.Weight = patientRequestModel.Weight;
            patient.DateOfBirth = patientRequestModel.DateOfBirth;
            patient.Gender = patientRequestModel.Gender;
            patient.CreatedAt = _commonService.GetCurrentIndianTime();
            //patient.DoctorId = patientRequestModel.DoctorId;
            await _unitOfWork.HealthReportRepository.Add(healthReport);
            await _unitOfWork.PatientRepository.Add(patient);
            if (patientRequestModel.Documents != null && patientRequestModel.Documents.Count > 0)
                await SavePatientDocumentsAsync(patient.Id, patientRequestModel.Documents);
            await _unitOfWork.Complete();
            if (patientRequestModel.GeneticDiseases != null && patientRequestModel.GeneticDiseases.Count > 0)
            {
                foreach (var geneticDisease in patientRequestModel.GeneticDiseases)
                {
                    PatientDiseases patientDisease = new PatientDiseases
                    {
                        PatientId = patient.Id,
                        GeneticDiseaseId = geneticDisease.GeneticDiseaseId,
                        Medicines = geneticDisease.Medicines
                    };
                    await _unitOfWork.PatientDiseasesRepository.Add(patientDisease);
                }
                await _unitOfWork.Complete();
            }
            if (patientRequestModel.BookAppointment)
            {
                Appointment appointment = new Appointment
                {
                    Id = Guid.NewGuid().ToString(),
                    PatientId = patient.Id,
                    DoctorId = patientRequestModel.DoctorId,
                    AppointmentDate = _commonService.GetCurrentIndianTime(),
                    Status = AppointmentStatus.Scheduled,
                    CreatedAt = _commonService.GetCurrentIndianTime()
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
                        .Where(h => h.PatientId == p.Id && h.CreatedAt.Date == _commonService.GetCurrentIndianTime().Date)
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
                    GeneticFrom = x.Patient.GeneticFrom,
                    DateOfBirth = x.Patient.DateOfBirth,
                    Gender = x.Patient.Gender,
                    GeneticDiseases = x.Patient.PatientDiseases.Select(pd => new GeneticDiseaseViewModel
                    {
                        GeneticDiseaseId = pd.GeneticDiseaseId,
                        Medicines = pd.Medicines,
                        DiseaseName = pd.GeneticDisease != null ? pd.GeneticDisease.DiseaseName : null,
                        ColorCode = pd.GeneticDisease != null ? pd.GeneticDisease.ColorCode : null
                    }).ToList(),

                    Weight = x.HealthReport != null ? x.HealthReport.Weight ?? "0" : "0",
                    Height = x.HealthReport != null ? x.HealthReport.Height ?? "0" : "0",
                    BloodPressure = x.HealthReport != null ? x.HealthReport.BloodPressure ?? "0" : "0",
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

            // populate docs for this patient
            var patientDocs = await _unitOfWork.PatientReportDocsRepository.GetWithoutTracking()
                .Where(d => d.PatientId == Id)
                .Select(d => new PatientReportDocViewModel { Id = d.Id, DocAddress = d.DocAddress, DocType = d.DocType, CreatedAt = d.CreatedAt })
                .ToListAsync();
            patient.PatientReportDocs = patientDocs;

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
            var healthRpt = await _unitOfWork.HealthReportRepository.GetWithTracking().Where(h => h.PatientId == patientId && h.CreatedAt.Date == _commonService.GetCurrentIndianTime().Date).OrderByDescending(d => d.CreatedAt).FirstOrDefaultAsync();

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
            patient.GeneticFrom = patientRequestModel.GeneticFrom;
            patient.DateOfBirth = patientRequestModel.DateOfBirth;
            patient.Gender = patientRequestModel.Gender;
            bool isSameHealthReport =
        healthRpt != null &&
        (healthRpt.Weight ?? "") == (patientRequestModel.Weight ?? "") &&
        (healthRpt.Height ?? "") == (patientRequestModel.Height ?? "") &&
        (healthRpt.BloodPressure ?? "") == (patientRequestModel.BloodPressure ?? "") &&
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
                    BloodPressure = patientRequestModel.BloodPressure,
                    SugarBeforeMeal = patientRequestModel.SugarBeforeMeal,
                    SugarAfterMeal = patientRequestModel.SugarAfterMeal,
                    CreatedAt = _commonService.GetCurrentIndianTime()
                };

                await _unitOfWork.HealthReportRepository.Add(healthReport);
            }
            if (patientRequestModel.GeneticDiseases != null && patientRequestModel.GeneticDiseases.Count > 0)
            {
                var existingDiseases = await _unitOfWork.PatientDiseasesRepository.GetWithTracking().Where(x => x.PatientId == patientId).ToListAsync();

                await _unitOfWork.PatientDiseasesRepository.RemoveRange(existingDiseases);
                foreach (var geneticDisease in patientRequestModel.GeneticDiseases)
                {
                    var existing = await _unitOfWork.PatientDiseasesRepository.GetWithTracking()
                        .Where(pd => pd.PatientId == patientId && pd.GeneticDiseaseId == geneticDisease.GeneticDiseaseId)
                        .FirstOrDefaultAsync();
                    if (existing != null)
                    {
                        existing.Medicines = geneticDisease.Medicines;
                        await _unitOfWork.PatientDiseasesRepository.Update(existing);
                    }
                    else
                    {
                        PatientDiseases patientDisease = new PatientDiseases
                        {
                            PatientId = patientId,
                            GeneticDiseaseId = geneticDisease.GeneticDiseaseId,
                            Medicines = geneticDisease.Medicines
                        };
                        await _unitOfWork.PatientDiseasesRepository.Add(patientDisease);
                    }
                }
            }
            if (patientRequestModel.DeletedDocumentIds != null && patientRequestModel.DeletedDocumentIds.Count > 0)
            {
                var docsToDelete = await _unitOfWork.PatientReportDocsRepository.GetWithTracking()
                    .Where(d => d.PatientId == patientId && patientRequestModel.DeletedDocumentIds.Contains(d.Id))
                    .ToListAsync();

                foreach (var doc in docsToDelete)
                {
                    try
                    {
                        if (!string.IsNullOrEmpty(doc.DocAddress) && File.Exists(doc.DocAddress))
                            File.Delete(doc.DocAddress);
                    }
                    catch
                    {
                        // ignore file delete errors
                    }
                    await _unitOfWork.PatientReportDocsRepository.Remove(doc);
                }
            }

            if (patientRequestModel.Documents != null && patientRequestModel.Documents.Count > 0)
                await SavePatientDocumentsAsync(patient.Id, patientRequestModel.Documents);
            await _unitOfWork.PatientRepository.Update(patient);
            await _unitOfWork.Complete();
            response.IsSuccess = true;
            response.Message = "Patient updated successfully.";
            return response;
        }
        //public async Task<ResponseObj<List<PatientViewModel>>> GetPatients(string? doctorId = null)
        //{
        //    var response = new ResponseObj<List<PatientViewModel>>();

        //    var patientsQuery = _unitOfWork.PatientRepository.GetWithoutTracking().AsQueryable();
        //    var patients = await patientsQuery
        //        .Select(p => new PatientViewModel
        //        {
        //            Id = p.Id,
        //            FirstName = p.FirstName,
        //            LastName = p.LastName,
        //            UserId = p.UserId,
        //            Phone = p.Phone,
        //            Gender = p.Gender,
        //            Address = p.Address,
        //            BloodGroup = p.BloodGroup,
        //            GeneticFrom = p.GeneticFrom,
        //            DateOfBirth = p.DateOfBirth,
        //            CreatedAt = p.CreatedAt,
        //            PatientId = $"PAT{p.PatientId:D6}"
        //        }).OrderByDescending(p => p.CreatedAt)
        //        .ToListAsync();
        //    foreach (var pv in patients)
        //    {
        //        var docs = await _unitOfWork.PatientReportDocsRepository.GetWithoutTracking().Where(d => d.PatientId == pv.Id).Select(d => new ViewModels.PatientReportDocViewModel { Id = d.Id, DocAddress = d.DocAddress, DocType = d.DocType, CreatedAt = d.CreatedAt }).ToListAsync();
        //        pv.PatientReportDocs = docs;
        //    }
        //    if (patients == null || patients.Count == 0)
        //    {
        //        response.IsSuccess = false;
        //        response.Message = "Patients not found.";
        //    }
        //    response.IsSuccess = true;
        //    response.Message = "Patients retrieved successfully.";
        //    response.Data = patients;
        //    return response;
        //}
        public async Task<PagedResponse<PatientViewModel>> GetPatients(PaginationRequest request)
        {
            var query = _unitOfWork.PatientRepository
                       .GetWithoutTracking();

            query = query
                    .ApplySearch(request.Search,
                        x => x.FirstName,
                        x => x.LastName,
                        x => x.Phone)
                    .ApplyFilters(request.Filters);

            var totalCount = await query.CountAsync();

            if (!string.IsNullOrWhiteSpace(request.SortColumn))
            {
                query = query.ApplySorting(
                    request.SortColumn,
                    request.SortDirection);
            }
            else
            {
                query = query.OrderByDescending(x => x.CreatedAt);
            }

            var patients = await query
                    .ApplyPagination(request.PageNumber, request.PageSize)
                    .Select(p => new PatientViewModel
                    {
                        Id = p.Id,
                        FirstName = p.FirstName,
                        LastName = p.LastName,
                        Phone = p.Phone,
                        Gender = p.Gender,
                        BloodGroup = p.BloodGroup,
                        Address = p.Address,
                        DateOfBirth = p.DateOfBirth,
                        CreatedAt = p.CreatedAt,
                        PatientId = $"PAT{p.PatientId:D6}"
                    })
                    .ToListAsync();

            return new PagedResponse<PatientViewModel>
            {
                IsSuccess = true,
                Message = "Patients retrieved successfully.",
                Data = patients,
                TotalCount = totalCount,
                PageNumber = request.PageNumber,
                PageSize = request.PageSize
            };
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
                    GeneticFrom = p.GeneticFrom,
                    PatientId = $"PAT{p.PatientId:D6}",
                    Age = _commonService.GetCurrentIndianTime().Year - p.DateOfBirth.Year,
                    Weight = healthRpt != null && healthRpt.Weight != null ? healthRpt.Weight : "0",
                    Height = healthRpt != null && healthRpt.Height != null ? healthRpt.Height : "0",
                    BloodPressure = healthRpt != null && healthRpt.BloodPressure != null ? healthRpt.BloodPressure : "0",
                    SugarBeforeMeal = healthRpt != null && healthRpt.SugarBeforeMeal != null ? healthRpt.SugarBeforeMeal : "0",
                    SugarAfterMeal = healthRpt != null && healthRpt.SugarAfterMeal != null ? healthRpt.SugarAfterMeal : "0",
                    Appointments = p.Appointments.Select(a => new AppointmentDto { AppointmentDate = a.AppointmentDate, Status = a.Status, DoctorName = a.Doctor.FirstName + " " + a.Doctor.LastName, Id = a.Id }).ToList(),
                    PrescriptionCount = _unitOfWork.PrescritpionRepository.GetWithoutTracking().Count(x=>x.PatientId == Id),
                    MedicalRecordCount = _unitOfWork.MedicalRecordRepository.GetWithoutTracking().Count(x => x.PatientId == Id),
                })
                .FirstOrDefaultAsync();
            if (details == null)
            {
                responseObj.Data = null;
                responseObj.IsSuccess = false;
                responseObj.Message = "No record found";
                return responseObj;
            }
            var patientDocs = await _unitOfWork.PatientReportDocsRepository.GetWithoutTracking()
                .Where(d => d.PatientId == Id)
                .Select(d => new PatientReportDocViewModel { Id = d.Id, DocAddress = d.DocAddress, DocType = d.DocType, CreatedAt = d.CreatedAt })
                .ToListAsync();
            details.PatientReportDocs = patientDocs;
            responseObj.IsSuccess = true;
            responseObj.Data = details;
            responseObj.Message = "Record retreived successfully";
            return responseObj;
        }

        private const string PatientFilesFolderName = "PatientFiles";

        private static string GetPatientFilesFolder()
        {
            var baseFolder = Path.Combine(Directory.GetCurrentDirectory(), PatientFilesFolderName);
            if (!Directory.Exists(baseFolder))
                Directory.CreateDirectory(baseFolder);
            return baseFolder;
        }
        private static string BuildPatientStoredFileName(string patientFirstName, string patientLastName, DateTime createdAt, string originalFileName, int sequence, string? contentType)
        {
            var ext = ResolveFileExtension(originalFileName, contentType);

            var safeBase = SanitizeFileStem(Path.GetFileNameWithoutExtension(originalFileName));
            if (string.IsNullOrEmpty(safeBase))
                safeBase = "document";
            if (safeBase.Length > 60)
                safeBase = safeBase[..60];

            var safeFirst = SanitizeFileStem(patientFirstName ?? string.Empty);
            var safeLast = SanitizeFileStem(patientLastName ?? string.Empty);
            if (safeFirst.Length > 20) safeFirst = safeFirst[..20];
            if (safeLast.Length > 20) safeLast = safeLast[..20];

            var timestamp = createdAt.ToString("yyyyMMddHHmmssfff");
            var namePart = string.IsNullOrEmpty(safeFirst) && string.IsNullOrEmpty(safeLast)
                ? "patient"
                : (string.IsNullOrEmpty(safeLast) ? safeFirst : $"{safeFirst}_{safeLast}");

            return $"{namePart}_{timestamp}_{sequence:D2}_{safeBase}{ext}";
        }

        private static string SanitizeFileStem(string value)
        {
            if (string.IsNullOrWhiteSpace(value))
                return string.Empty;

            var invalid = Path.GetInvalidFileNameChars();
            var sb = new StringBuilder(value.Length);
            foreach (var c in value)
            {
                if (Array.IndexOf(invalid, c) >= 0 || c == ' ')
                    sb.Append('-');
                else
                    sb.Append(char.ToLowerInvariant(c));
            }

            var sanitized = sb.ToString().Trim('-');
            while (sanitized.Contains("--", StringComparison.Ordinal))
                sanitized = sanitized.Replace("--", "-", StringComparison.Ordinal);

            return sanitized;
        }

        private static string ResolveFileExtension(string fileName, string? contentType)
        {
            var ext = Path.GetExtension(fileName);
            if (!string.IsNullOrWhiteSpace(ext))
                return ext.ToLowerInvariant();

            if (string.IsNullOrWhiteSpace(contentType))
                return ".bin";

            return contentType.ToLowerInvariant() switch
            {
                "image/jpeg" or "image/jpg" => ".jpg",
                "image/png" => ".png",
                "image/gif" => ".gif",
                "image/webp" => ".webp",
                "image/bmp" => ".bmp",
                "application/pdf" => ".pdf",
                "text/csv" => ".csv",
                "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet" => ".xlsx",
                "application/vnd.ms-excel" => ".xls",
                _ => ".bin"
            };
        }

        private async Task SavePatientDocumentsAsync(string patientId, IEnumerable<PatientDocumentModel> documents)
        {
            var baseFolder = GetPatientFilesFolder();
            var sequence = 0;

            // try to get patient names to build friendly filenames
            var patientInfo = await _unitOfWork.PatientRepository.GetWithoutTracking()
                .Where(p => p.Id == patientId)
                .Select(p => new { p.FirstName, p.LastName })
                .FirstOrDefaultAsync();

            foreach (var doc in documents)
            {
                if (string.IsNullOrWhiteSpace(doc.ContentBase64))
                    continue;

                sequence++;
                try
                {
                    var createdAt = _commonService.GetCurrentIndianTime();
                    var savedName = BuildPatientStoredFileName(
                        patientInfo?.FirstName ?? string.Empty,
                        patientInfo?.LastName ?? string.Empty,
                        createdAt,
                        doc.FileName,
                        sequence,
                        doc.ContentType);
                    var savedPath = Path.Combine(baseFolder, savedName);
                    var bytes = Convert.FromBase64String(doc.ContentBase64);
                    await File.WriteAllBytesAsync(savedPath, bytes);

                    var reportDoc = new PatientReportDocs
                    {
                        Id = Guid.NewGuid().ToString(),
                        PatientId = patientId,
                        DocAddress = savedPath,
                        DocType = doc.ContentType ?? Path.GetExtension(doc.FileName),
                        CreatedAt = createdAt
                    };
                    await _unitOfWork.PatientReportDocsRepository.Add(reportDoc);
                }
                catch
                {
                    // ignore single file errors
                }
            }
        }
    }
}
