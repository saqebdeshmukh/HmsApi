using BusinessLogic.GenericRepository;
using DataAccess.Context;
using DataAccess.DBEntities;

namespace DataAccess.PatientReportDocsRepository
{
    public class PatientReportDocsRepository : GenericRepository<PatientReportDocs>, IPatientReportDocsRepository
    {
        public PatientReportDocsRepository(ApplicationContext applicationContext) : base(applicationContext) { }
    }
}
