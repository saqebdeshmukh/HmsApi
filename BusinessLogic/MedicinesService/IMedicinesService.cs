using System.Collections.Generic;
using ViewModels;

namespace BusinessLogic.MedicinesService
{
    public interface IMedicinesService
    {
        Task<ResponseObj<string>> CreateMedicines(MedicinesRequestModel request);
        Task<ResponseObj<string>> UpdateMedicine(MedicinesRequestModel request, string id);
        Task<ResponseObj<string>> DeleteMedicine(string id);
        Task<ResponseObj<MedicinesDto>> GetMedicineById(string id);
        Task<ResponseObj<List<MedicinesDto>>> GetAllMedicines();
        Task<ResponseObj<List<MedicinesDto>>> GetSearchedMedicines(string? searchTerm);
    }
}
