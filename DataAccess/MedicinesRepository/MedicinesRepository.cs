using BusinessLogic.GenericRepository;
using DataAccess.Context;
using DataAccess.DBEntities;

namespace DataAccess.MedicinesRepository
{
    public class MedicinesRepository : GenericRepository<Medicines>, IMedicinesRepository
    {
        public MedicinesRepository(ApplicationContext applicationContext) : base(applicationContext) { }
    }
}
