using BusinessLogic.GenericRepository;
using DataAccess.Context;
using DataAccess.DBEntities;
using System;
using System.Collections.Generic;
using System.Text;

namespace DataAccess.GeneticDiseaseRepository
{
    public class GeneticDiseaseRepository : GenericRepository<GeneticDisease>, IGeneticDiseaseRepository
    {
        public GeneticDiseaseRepository(ApplicationContext context) : base(context)
        {
        }
    }
}
