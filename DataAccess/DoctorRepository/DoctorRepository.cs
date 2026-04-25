using BusinessLogic.GenericRepository;
using DataAccess.Context;
using DataAccess.DBEntities;
using System;
using System.Collections.Generic;
using System.Text;

namespace DataAccess.DoctorRepository
{
    public class DoctorRepository:GenericRepository<Doctor>, IDoctorRepository
    {
        public DoctorRepository(ApplicationContext context) : base(context) { }
    }
}
