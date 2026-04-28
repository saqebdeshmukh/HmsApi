using DataAccess.AppointmentRepository;
using DataAccess.DoctorRepository;
using DataAccess.HealthReportRepository;
using DataAccess.MedicalRecordRepository;
using DataAccess.PatientRepository;
using DataAccess.PrescritpionRepository;
using DataAccess.ReceptionistRepository;
using DataAccess.RefreshTokenRepository;
using DataAccess.UserRepository;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BusinessLogic.UnitOfWork
{
    public interface IUnitOfWork : IDisposable
    {
        Task<int> Complete();
        void Dispose();
        IUserRepository UserRepository { get; }
        IRefreshTokenRepository RefreshTokenRepository { get; }
        IPatientRepository PatientRepository { get; }
        IDoctorRepository DoctorRepository { get; }
        IAppointmentRepository AppointmentRepository { get; }
        IMedicalRecordRepository MedicalRecordRepository { get; }
        IReceptionistRepository ReceptionistRepository { get; }
        IPrescritpionRepository PrescritpionRepository { get; }
        IHealthReportRepository HealthReportRepository { get; }
    }
}
