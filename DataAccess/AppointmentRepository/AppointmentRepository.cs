using BusinessLogic.GenericRepository;
using DataAccess.Context;
using DataAccess.DBEntities;
using System;
using System.Collections.Generic;
using System.Text;

namespace DataAccess.AppointmentRepository
{
    public class AppointmentRepository:GenericRepository<Appointment>, IAppointmentRepository
    {
        public AppointmentRepository(ApplicationContext context) : base(context) { }
    }
}
