using BusinessLogic.CommonService;
using BusinessLogic.UnitOfWork;
using DataAccess.DBEntities;
using DocumentFormat.OpenXml.Office2016.Drawing.ChartDrawing;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Text;
using ViewModels;

namespace BusinessLogic.DoctorService
{
    public class DoctorService : IDoctorService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ICommonService _commonService;
        public DoctorService(IUnitOfWork unitOfWork, ICommonService commonService)
        {
            _unitOfWork = unitOfWork;
            _commonService = commonService;
        }
        public async Task<ResponseObj<string>> CreateDoctor(DoctorRequestModel doctorRequestModel)
        {
            var doctor = new Doctor
            {
                Id = Guid.NewGuid().ToString(),
                FirstName = doctorRequestModel.FirstName,
                LastName = doctorRequestModel.LastName,
                Specialization = doctorRequestModel.Specialization,
                Email = doctorRequestModel.Email,
                Phone = doctorRequestModel.PhoneNumber,
                ConsultationFee = doctorRequestModel.ConsultationFee,
                CreatedAt = _commonService.GetCurrentIndianTime(),
                RegNo = doctorRequestModel.RegNo,
                Address = doctorRequestModel.Address,
            };
            await _unitOfWork.DoctorRepository.Add(doctor);
            await _unitOfWork.Complete();
            return new ResponseObj<string>
            {
                IsSuccess = true,
                Message = "Doctor created successfully",
                Data = doctor.Id
            };
        }
        public async Task<ResponseObj<List<DoctorViewModel>>> GetDoctors()
        {
            ResponseObj<List<DoctorViewModel>> responseObj = new ResponseObj<List<DoctorViewModel>>();
            var Doctors = await _unitOfWork.DoctorRepository.GetWithoutTracking()
                .Select(s => new DoctorViewModel
                {
                    Id = s.Id,
                    FirstName = s.FirstName,
                    LastName = s.LastName,
                    Specialization = s.Specialization,
                    Email = s.Email,
                    ConsultationFee = s.ConsultationFee,
                    PhoneNumber = s.Phone,
                    UserId = s.UserId,
                    CreatedAt = s.CreatedAt,
                    RegNo = s.RegNo,
                    Address = s.Address,
                })
                .OrderByDescending(x => x.CreatedAt).ToListAsync();
            if (Doctors.Any())
            {
                responseObj.IsSuccess = true;
                responseObj.Message = "Doctors retrieved successully";
                responseObj.Data = Doctors;
            }
            return responseObj;
        }
        public async Task<ResponseObj<DoctorViewModel>> GetDoctorById(string Id)
        {
            ResponseObj<DoctorViewModel> responseObj = new ResponseObj<DoctorViewModel>();
            var Doctors = await _unitOfWork.DoctorRepository.GetWithoutTracking().Where(x => x.Id == Id)
                .Select(s => new DoctorViewModel
                {

                    FirstName = s.FirstName,
                    LastName = s.LastName,
                    Specialization = s.Specialization,
                    Email = s.Email,
                    ConsultationFee = s.ConsultationFee,
                    PhoneNumber = s.Phone,
                    UserId = s.UserId,
                    CreatedAt = s.CreatedAt,
                    RegNo = s.RegNo,
                    Address = s.Address,
                })
                .OrderByDescending(x => x.CreatedAt).FirstOrDefaultAsync();
            if (Doctors == null)
            {
                responseObj.IsSuccess = false;
                responseObj.Message = "Doctor not found";
                return responseObj;
            }
            responseObj.IsSuccess = true;
            responseObj.Message = "Doctor retrieved successully";
            responseObj.Data = Doctors;
            return responseObj;
        }
        public async Task<ResponseObj<string>> UpdateDoctor(DoctorRequestModel doctorRequest, string doctorId)
        {
            ResponseObj<string> responseObj = new ResponseObj<string>();
            var Doctor = await _unitOfWork.DoctorRepository.GetWithTracking().Where(x => x.Id == doctorId).FirstOrDefaultAsync();
            if (Doctor == null)
            {
                responseObj.IsSuccess = false;
                responseObj.Message = "Doctor not found";
                return responseObj;
            }
            Doctor.FirstName = doctorRequest.FirstName;
            Doctor.LastName = doctorRequest.LastName;
            Doctor.Specialization = doctorRequest.Specialization;
            Doctor.ConsultationFee = doctorRequest.ConsultationFee;
            Doctor.Phone = doctorRequest.PhoneNumber;
            Doctor.RegNo = doctorRequest.RegNo;
            Doctor.Address = doctorRequest.Address;
            await _unitOfWork.DoctorRepository.Update(Doctor);
            await _unitOfWork.Complete();
            responseObj.IsSuccess = true;
            responseObj.Message = "Doctor updated successfully";
            return responseObj;
        }
    }
}
