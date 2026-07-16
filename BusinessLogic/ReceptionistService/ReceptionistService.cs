using BusinessLogic.CommonService;
using BusinessLogic.UnitOfWork;
using DataAccess.DBEntities;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Text;
using ViewModels;

namespace BusinessLogic.ReceptionistService
{
    public class ReceptionistService:IReceptionistService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IPasswordHasher<User> _hasher;
        private readonly ICommonService _commonService;
        public ReceptionistService(IUnitOfWork unitOfWork, IPasswordHasher<User> hasher, ICommonService commonService)
        {
            _unitOfWork = unitOfWork;
            _hasher = hasher;
            _commonService = commonService;
        }
        public async Task<ResponseObj<string>> CreateReceptionist(ReceptionistRequestModel receptionstRequestModel)
        {
            ResponseObj<string> responseObj = new ResponseObj<string>();
            var user = new User
            {
                Id = Guid.NewGuid().ToString(),
                Email = receptionstRequestModel.Email,
                Role = "Receptionist",
                CreatedAt = _commonService.GetCurrentIndianTime(),
            };
            user.Password = _hasher.HashPassword(user, receptionstRequestModel.Password);
            await _unitOfWork.UserRepository.Add(user);
            var receptionist = new Receptionist
            {
                Id = Guid.NewGuid().ToString(),
                UserId = user.Id,
                FirstName = receptionstRequestModel.FirstName,
                LastName = receptionstRequestModel.LastName,
                Phone = receptionstRequestModel.Phone ?? string.Empty,
                CreatedAt = _commonService.GetCurrentIndianTime()
            };
            await _unitOfWork.ReceptionistRepository.Add(receptionist);
            await _unitOfWork.Complete();
            responseObj.IsSuccess= true;
            responseObj.Message = "Receptionist added successfully";
            return responseObj;
        }
        public async Task<ResponseObj<List<ReceptionistViewModel>>> Receptionists()
        {
            ResponseObj<List<ReceptionistViewModel>> responseObj = new ResponseObj<List<ReceptionistViewModel>>();
            var receptionistList = await _unitOfWork.ReceptionistRepository.GetWithoutTracking().
                Select(s => new ReceptionistViewModel
                {
                    Name = s.FirstName + " " + s.LastName,
                    EmailId = s.User.Email,
                    Created = s.CreatedAt,
                    PhoneNo = s.Phone
                }).ToListAsync();
            if (receptionistList == null || receptionistList.Count == 0)
            {
                responseObj.IsSuccess = false;
                responseObj.Message = "Receptionist not found";
                return responseObj;
            }
            responseObj.IsSuccess = true;
            responseObj.Data = receptionistList;
            responseObj.Message = "Receptionist retrieved successfully";
            return responseObj;
        }
    }
}
