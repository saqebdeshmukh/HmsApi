using BusinessLogic.GenericRepository;
using DataAccess.Context;
using DataAccess.DBEntities;
using System;
using System.Collections.Generic;
using System.Text;

namespace DataAccess.PatientAllergyRepository
{
    public class PatientAllergyRepository : GenericRepository<PatientAllergy>, IPatientAllergyRepository
    {
        public PatientAllergyRepository(ApplicationContext context) : base(context)
        {
        }
    }
}
