using BusinessLogic.GeneticDiseaseService;
using DataAccess.DBEntities;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using ViewModels;

namespace HealthCareAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class GeneticDiseaseController : ControllerBase
    {
        private readonly IGeneticDiseaseService _geneticDiseaseService;
        public GeneticDiseaseController(IGeneticDiseaseService geneticDiseaseService)
        {
            _geneticDiseaseService = geneticDiseaseService;
        }
        [HttpGet("GetGeneticDiseases")]
        public async Task<ResponseObj<List<GeneticDisease>>> GetGeneticDiseases()
            => await _geneticDiseaseService.GetGeneticDiseases();
        
        [HttpPost("CreateGeneticDisease")]
        public async Task<ResponseObj<GeneticDisease>> CreateGeneticDisease(GeneticDiseaseRequestModel geneticDiseaseRequestModel)
            => await _geneticDiseaseService.CreateGeneticDisease(geneticDiseaseRequestModel);
    }
}
