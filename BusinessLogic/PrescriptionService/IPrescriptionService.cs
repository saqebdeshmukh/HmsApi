using System;
using System.Collections.Generic;
using System.Text;
using ViewModels;

namespace BusinessLogic.PrescriptionService
{
    public interface IPrescriptionService
    {
        Task<ResponseObj<string>> CreatePrescription(PrescriptionRequestModel prescriptionRequestModel);
        Task<ResponseObj<string>> UpdatePrescription(PrescriptionRequestModel prescriptionRequestModel, string PrescriptionId);
        Task<ResponseObj<List<PrescriptionViewModel>>> Prescriptions(string? doctorId = null);
        Task<ResponseObj<PrintViewModel>> PrescriptionById(string id);
    }
}
