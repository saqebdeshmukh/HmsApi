using BusinessLogic.GenericRepository;
using DataAccess.Context;
using DataAccess.DBEntities;

namespace DataAccess.MedicineReportDocsRepository
{
    public class MedicineReportDocsRepository : GenericRepository<MedicineReportDocs>, IMedicineReportDocsRepository
    {
        public MedicineReportDocsRepository(ApplicationContext applicationContext) : base(applicationContext) { }
    }
}
