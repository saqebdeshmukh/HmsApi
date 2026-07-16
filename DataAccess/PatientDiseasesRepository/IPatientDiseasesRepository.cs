using BusinessLogic.GenericRepository;
using DataAccess.DBEntities;
using System;
using System.Collections.Generic;
using System.Text;

namespace DataAccess.PatientDiseasesRepository
{
    public interface IPatientDiseasesRepository : IGenericRepository<PatientDiseases>
    {
    }
}
