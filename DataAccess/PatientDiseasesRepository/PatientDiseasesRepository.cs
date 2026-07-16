using BusinessLogic.GenericRepository;
using DataAccess.Context;
using DataAccess.DBEntities;
using System;
using System.Collections.Generic;
using System.Text;

namespace DataAccess.PatientDiseasesRepository
{
    public class PatientDiseasesRepository : GenericRepository<PatientDiseases>, IPatientDiseasesRepository
    {
        public PatientDiseasesRepository(ApplicationContext context) : base(context)
        {
        }
    }
}
