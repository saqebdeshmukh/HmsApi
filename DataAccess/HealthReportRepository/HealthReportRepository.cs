using BusinessLogic.GenericRepository;
using DataAccess.Context;
using DataAccess.DBEntities;
using System;
using System.Collections.Generic;
using System.Text;

namespace DataAccess.HealthReportRepository
{
    public class HealthReportRepository : GenericRepository<HealthReport>, IHealthReportRepository
    {
        public HealthReportRepository(ApplicationContext applicationContext) : base(applicationContext) { }
    }
}
