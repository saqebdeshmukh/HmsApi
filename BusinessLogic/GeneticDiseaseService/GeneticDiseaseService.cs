using BusinessLogic.UnitOfWork;
using DataAccess.DBEntities;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Text;
using ViewModels;

namespace BusinessLogic.GeneticDiseaseService
{
    public class GeneticDiseaseService : IGeneticDiseaseService
    {
        private readonly IUnitOfWork _unitOfWork;
        public GeneticDiseaseService(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }
        public async Task<ResponseObj<List<GeneticDisease>>> GetGeneticDiseases()
        {
            ResponseObj<List<GeneticDisease>> responseObj = new ResponseObj<List<GeneticDisease>>();
            var getList = await _unitOfWork.GeneticDiseaseRepository.GetWithoutTracking().ToListAsync();
            responseObj.Data = getList;
            responseObj.Message = "Genetic diseases retrieved successfully";
            responseObj.IsSuccess = true;
            return responseObj;
        }
        public async Task<ResponseObj<GeneticDisease>> CreateGeneticDisease(GeneticDiseaseRequestModel geneticDiseaseRequestModel)
        {
            ResponseObj<GeneticDisease> responseObj = new ResponseObj<GeneticDisease>();
            GeneticDisease geneticDisease = new GeneticDisease()
            {
                DiseaseName = geneticDiseaseRequestModel.DiseaseName,
                ColorCode = geneticDiseaseRequestModel.ColorCode
            };
            await _unitOfWork.GeneticDiseaseRepository.Add(geneticDisease);
            await _unitOfWork.Complete();
            responseObj.Data = geneticDisease;
            responseObj.Message = "Genetic disease added to the list";
            responseObj.IsSuccess = true;
            return responseObj;
        }

    }
}
