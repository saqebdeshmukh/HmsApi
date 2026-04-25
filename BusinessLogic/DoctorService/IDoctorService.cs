using System;
using System.Collections.Generic;
using System.Text;
using ViewModels;

namespace BusinessLogic.DoctorService
{
    public interface IDoctorService
    {
        Task<ResponseObj<string>> CreateDoctor(DoctorRequestModel doctorRequestModel);
        Task<ResponseObj<List<DoctorViewModel>>> GetDoctors();
        Task<ResponseObj<string>> UpdateDoctor(DoctorRequestModel doctorRequest, string doctorId);
        Task<ResponseObj<DoctorViewModel>> GetDoctorById(string Id);
    }
}
