using BusinessLogic.MedicinesService;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using ViewModels;

namespace HealthCareAPI.Controllers
{
    [Authorize]
    [Route("api/[controller]")]
    [ApiController]
    public class MedicinesController : ControllerBase
    {
        private readonly IMedicinesService _medicinesService;
        public MedicinesController(IMedicinesService medicinesService)
        {
            _medicinesService = medicinesService;
        }

        [HttpPost("CreateMedicine")]
        public async Task<ResponseObj<string>> CreateMedicine([FromBody] MedicinesRequestModel request)
            => await _medicinesService.CreateMedicines(request);

        [HttpGet("GetAllMedicines")]
        public async Task<ResponseObj<List<MedicinesDto>>> GetAllMedicines()
            => await _medicinesService.GetAllMedicines();

        [HttpGet("GetMedicine/{id}")]
        public async Task<ResponseObj<MedicinesDto>> GetMedicine(string id)
            => await _medicinesService.GetMedicineById(id);

        [HttpPut("UpdateMedicine/{id}")]
        public async Task<ResponseObj<string>> UpdateMedicine([FromBody] MedicinesRequestModel request, string id)
            => await _medicinesService.UpdateMedicine(request, id);

        [HttpDelete("DeleteMedicine/{id}")]
        public async Task<ResponseObj<string>> DeleteMedicine(string id)
            => await _medicinesService.DeleteMedicine(id);
        [HttpGet("GetSearchedMedicines/{searchTerm}")]
        public async Task<ResponseObj<List<MedicinesDto>>> GetSearchedMedicines(string? searchTerm)
            => await _medicinesService.GetSearchedMedicines(searchTerm);
    }
}
