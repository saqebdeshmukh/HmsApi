using System;
using System.Collections.Generic;
using System.Text;
using ViewModels;

namespace BusinessLogic.PatientService
{
    public interface IPatientService
    {
        Task<ResponseObj<string>> CreatePatient(PatientRequestModel patientRequestModel);
        Task<PagedResponse<PatientViewModel>> GetPatients(PaginationRequest request);
        Task<ResponseObj<string>> UpdatePatient(PatientRequestModel patientRequestModel, string patientId);
        Task<ResponseObj<PatientViewModel>> GetPatientById(string Id);
        Task<ResponseObj<PatientDetailsViewModel>> PatientDetails(string Id);
    }
}
