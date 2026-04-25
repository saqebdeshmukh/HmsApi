using BusinessLogic.GenericRepository;
using DataAccess.DBEntities;
using System;
using System.Collections.Generic;
using System.Text;

namespace DataAccess.PrescritpionRepository
{
    public interface IPrescritpionRepository:IGenericRepository<Prescription>
    {
    }
}
