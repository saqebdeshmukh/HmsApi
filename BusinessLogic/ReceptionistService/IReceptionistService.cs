using System;
using System.Collections.Generic;
using System.Text;
using ViewModels;

namespace BusinessLogic.ReceptionistService
{
    public interface IReceptionistService
    {
        Task<ResponseObj<string>> CreateReceptionist(ReceptionistRequestModel receptionstRequestModel);
        Task<ResponseObj<List<ReceptionistViewModel>>> Receptionists();
    }
}
