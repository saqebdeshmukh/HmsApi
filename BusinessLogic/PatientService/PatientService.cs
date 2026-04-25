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
            patient.Weight = patientRequestModel.Weight;
            patient.Height = patientRequestModel.Height;
            patient.BloodPressure = patientRequestModel.BloodPressure;
            patient.Sugar = patientRequestModel.Sugar;
            patient.DateOfBirth = patientRequestModel.DateOfBirth;
            patient.Gender = patientRequestModel.Gender;
            patient.CreatedAt= DateTime.UtcNow;
            //patient.DoctorId = patientRequestModel.DoctorId;
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
            var patients = await _unitOfWork.PatientRepository.GetWithoutTracking().Where(x => x.Id == Id)
                .Select(p => new PatientViewModel
                {
                    Id = p.Id,
                    FirstName = p.FirstName,
                    LastName = p.LastName,
                    UserId = p.UserId,
                    Phone = p.Phone,
                    Address = p.Address,
                    BloodGroup = p.BloodGroup,
                    Weight = p.Weight,
                    Height = p.Height,
                    BloodPressure = p.BloodPressure,
                    Sugar = p.Sugar,
                    DateOfBirth = p.DateOfBirth,
                    Gender=p.Gender
                })
                .FirstOrDefaultAsync();
            if (patients == null)
            {
                response.IsSuccess = false;
                response.Message = "Patient not found.";
                return response;
            }
            response.IsSuccess = true;
            response.Message = "Patients retrieved successfully.";
            response.Data = patients;
            return response;
        }
        public async Task<ResponseObj<string>> UpdatePatient(PatientRequestModel patientRequestModel, string patientId)
        {
            var response = new ResponseObj<string>();
            var patient = await _unitOfWork.PatientRepository.GetWithTracking().Where(p => p.Id == patientId
                                            || (p.Phone == patientRequestModel.Phone
                                                && p.FirstName == patientRequestModel.FirstName
                                                && p.LastName == patientRequestModel.LastName))
                                        .FirstOrDefaultAsync();

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
            patient.Weight = patientRequestModel.Weight;
            patient.Height = patientRequestModel.Height;
            patient.BloodPressure = patientRequestModel.BloodPressure;
            patient.Sugar = patientRequestModel.Sugar;
            patient.DateOfBirth = patientRequestModel.DateOfBirth;
            patient.Gender = patientRequestModel.Gender;
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

            if (!string.IsNullOrEmpty(doctorId))
            {
                //patientsQuery = patientsQuery.Where(p => p.DoctorId == doctorId);
            }

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
                    Weight = p.Weight,
                    Height = p.Height,
                    BloodPressure = p.BloodPressure,
                    Sugar = p.Sugar,
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
            response.Message= "Patients retrieved successfully.";
            response.Data = patients;
            return response;
        }
    }
}
