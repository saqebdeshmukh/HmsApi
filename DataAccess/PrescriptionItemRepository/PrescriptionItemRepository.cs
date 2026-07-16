using BusinessLogic.GenericRepository;
using DataAccess.Context;
using DataAccess.DBEntities;
using System;
using System.Collections.Generic;
using System.Text;

namespace DataAccess.PrescriptionItemRepository
{
    public class PrescriptionItemRepository:GenericRepository<PrescriptionItem>, IPrescriptionItemRepository
    {
        public PrescriptionItemRepository(ApplicationContext context) : base(context)
        {
        }
    }
}
