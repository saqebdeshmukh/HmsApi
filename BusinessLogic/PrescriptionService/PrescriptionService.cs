using BusinessLogic.UnitOfWork;
using DataAccess.DBEntities;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Text;
using ViewModels;

namespace BusinessLogic.PrescriptionService
{
    public class PrescriptionService : IPrescriptionService
    {
        private readonly IUnitOfWork _unitOfWork;
        public PrescriptionService(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }
        public async Task<ResponseObj<string>> CreatePrescription(PrescriptionRequestModel prescriptionRequestModel)
        {
            ResponseObj<string> responseObj = new ResponseObj<string>();
            Prescription prescription = new Prescription();
            prescription.Id = Guid.NewGuid().ToString();
            prescription.MedicalRecordId = prescriptionRequestModel.MedicalRecordId;
            prescription.PatientId = prescriptionRequestModel.PatientId;
            prescription.DoctorId = prescriptionRequestModel.DoctorId;
            prescription.Instructions = prescriptionRequestModel.Instructions;
            prescription.Type = prescriptionRequestModel.Type;
            prescription.PrescriptionDate = DateTime.UtcNow;
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


            await _unitOfWork.PrescritpionRepository.Add(prescription);
            await _unitOfWork.Complete();
            responseObj.Data = prescription.Id;
            responseObj.Message = "Prescription added successfully";
            responseObj.IsSuccess = true;
            return responseObj;
        }
        public async Task<ResponseObj<string>> UpdatePrescription(PrescriptionRequestModel prescriptionRequestModel, string PrescriptionId)
        {
            ResponseObj<string> responseObj = new ResponseObj<string>();
            var prescription = await _unitOfWork.PrescritpionRepository.GetWithTracking().Where(x => x.Id == PrescriptionId).Include(p => p.Items).FirstOrDefaultAsync();
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

            prescription.Items?.Clear();

            prescription.Items = prescriptionRequestModel.prescriptionItemRequests.Select(item => new PrescriptionItem
            {
                Id = Guid.NewGuid().ToString(),
                PrescriptionId = prescription.Id,
                MedicineName = item.MedicineName,
                Dosage = item.Dosage,
                Frequency = item.Frequency,
                MealTiming = item.MealTiming,
                Duration = item.Duration
            }).ToList();
            await _unitOfWork.PrescritpionRepository.Update(prescription);
            await _unitOfWork.Complete();
            responseObj.Message = "Prescription updated successfully";
            responseObj.IsSuccess = true;
            return responseObj;
        }
        public async Task<ResponseObj<List<PrescriptionViewModel>>> Prescriptions(string? doctorId = null)
        {
            ResponseObj<List<PrescriptionViewModel>> responseObj = new ResponseObj<List<PrescriptionViewModel>>();
            var prescriptionQuery = _unitOfWork.PrescritpionRepository.GetWithoutTracking().AsQueryable();
            if (doctorId != null)
            {
                prescriptionQuery = prescriptionQuery.Where(x => x.DoctorId == doctorId);
            }
            var prescriptionList = await prescriptionQuery.Select(s => new PrescriptionViewModel
            {
                Id = s.Id,
                DoctorId = s.DoctorId,
                DoctorName = s.Doctor.FirstName + " " + s.Doctor.LastName,
                PatientId = s.PatientId,
                PatientName = s.Patient.FirstName + " " + s.Patient.LastName,
                PrescriptionDate = s.PrescriptionDate,
                Type = s.Type,
                prescriptionItemRequests = s.Items.Select(p => new PrescriptionItemRequest { MedicineName = p.MedicineName, Dosage = p.Dosage, Duration = p.Duration, MealTiming = p.MealTiming, Frequency = p.Frequency }).ToList(),
            })
               .ToListAsync();
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
    }
}
