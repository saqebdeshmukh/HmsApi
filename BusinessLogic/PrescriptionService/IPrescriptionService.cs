using System;
using System.Collections.Generic;
using System.Text;
using ViewModels;

namespace BusinessLogic.PrescriptionService
{
    public interface IPrescriptionService
    {
        Task<ResponseObj<PrescriptionIdModel>> CreatePrescription(PrescriptionRequestModel prescriptionRequestModel);
        Task<ResponseObj<string>> UpdatePrescription(PrescriptionRequest prescriptionRequestModel, string PrescriptionId);
        Task<ResponseObj<List<PrescriptionViewModel>>> Prescriptions(PrescriptionQueryParameters? query = null);
        Task<ResponseObj<PrintViewModel>> PrescriptionById(string id);
        Task<ResponseObj<List<PrintViewModel>>> GetPrescriptionsByPatientId(string patientId);
        Task<PagedResponse<PrescriptionViewModel>> InternalPrescriptions(PaginationRequest request, string? doctorId = null);
        Task<PagedResponse<PrescriptionViewModel>> ExternalPrescriptions(PaginationRequest request, string? doctorId = null);
        Task<ResponseObj<PrintViewModel>> PrescriptionByIdForPrint(string id);
    }
}
