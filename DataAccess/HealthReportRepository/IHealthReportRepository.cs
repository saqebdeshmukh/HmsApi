using BusinessLogic.GenericRepository;
using DataAccess.DBEntities;
using System;
using System.Collections.Generic;
using System.Text;

namespace DataAccess.HealthReportRepository
{
    public interface IHealthReportRepository:IGenericRepository<HealthReport>
    {
    }
}
