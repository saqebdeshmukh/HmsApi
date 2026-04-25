using BusinessLogic.GenericRepository;
using DataAccess.Context;
using DataAccess.DBEntities;
using System;
using System.Collections.Generic;
using System.Text;

namespace DataAccess.ReceptionistRepository
{
    public class ReceptionistRepository:GenericRepository<Receptionist>, IReceptionistRepository
    {
        public ReceptionistRepository(ApplicationContext context) : base(context) { }
    }
}
