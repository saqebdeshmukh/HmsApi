using BusinessLogic.GenericRepository;
using DataAccess.Context;
using DataAccess.DBEntities;
using System;
using System.Collections.Generic;
using System.Text;

namespace DataAccess.PrescritpionRepository
{
    public class PrescritpionRepository: GenericRepository<Prescription>, IPrescritpionRepository
    {
        public PrescritpionRepository(ApplicationContext context) : base(context)
        {
        }
    }
}
