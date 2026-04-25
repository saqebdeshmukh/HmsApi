using BusinessLogic.GenericRepository;
using DataAccess.DBEntities;
using System;
using System.Collections.Generic;
using System.Text;

namespace DataAccess.AppointmentRepository
{
    public interface IAppointmentRepository:IGenericRepository<Appointment>
    {
    }
}
