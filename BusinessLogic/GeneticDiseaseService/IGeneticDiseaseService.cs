using DataAccess.DBEntities;
using System;
using System.Collections.Generic;
using System.Text;
using ViewModels;

namespace BusinessLogic.GeneticDiseaseService
{
    public interface IGeneticDiseaseService
    {
        Task<ResponseObj<List<GeneticDisease>>> GetGeneticDiseases();
        Task<ResponseObj<GeneticDisease>> CreateGeneticDisease(GeneticDiseaseRequestModel geneticDiseaseRequestModel);
    }
}
