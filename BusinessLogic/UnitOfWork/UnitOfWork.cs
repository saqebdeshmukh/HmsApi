using DataAccess.AppointmentRepository;
using DataAccess.Context;
using DataAccess.DoctorRepository;
using DataAccess.HealthReportRepository;
using DataAccess.PatientReportDocsRepository;
using DataAccess.MedicinesRepository;
using DataAccess.MedicalRecordRepository;
using DataAccess.PatientRepository;
using DataAccess.PrescritpionRepository;
using DataAccess.ReceptionistRepository;
using DataAccess.RefreshTokenRepository;
using DataAccess.UserRepository;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DataAccess.GeneticDiseaseRepository;
using DataAccess.PatientDiseasesRepository;
using DataAccess.PrescriptionItemRepository;
using DataAccess.PatientAllergyRepository;

namespace BusinessLogic.UnitOfWork
{
    public class UnitOfWork : IUnitOfWork
    {
        private readonly ApplicationContext _context;
        public UnitOfWork(ApplicationContext context)
        {
            _context = context;
            UserRepository = new UserRepository(_context);
            RefreshTokenRepository = new RefreshTokenRepository(_context);
            PatientRepository = new PatientRepository(_context);
            DoctorRepository = new DoctorRepository(_context);
            AppointmentRepository = new AppointmentRepository(_context);
            MedicalRecordRepository = new MedicalRecordRepository(_context);
            ReceptionistRepository = new ReceptionistRepository(_context);
            PrescritpionRepository = new PrescritpionRepository(_context);
            HealthReportRepository = new HealthReportRepository(_context);
            PatientReportDocsRepository = new PatientReportDocsRepository(_context);
            MedicinesRepository = new MedicinesRepository(_context);
            GeneticDiseaseRepository = new GeneticDiseaseRepository(_context);
            PatientDiseasesRepository = new PatientDiseasesRepository(_context);
            PrescriptionItemRepository = new PrescriptionItemRepository(_context);
            PatientAllergyRepository = new PatientAllergyRepository(_context);
        }
        public IUserRepository UserRepository { get; private set; }
        public IRefreshTokenRepository RefreshTokenRepository { get; private set; }
        public IPatientRepository PatientRepository { get; private set; }
        public IDoctorRepository DoctorRepository { get; private set; }
        public IAppointmentRepository AppointmentRepository { get; private set; }
        public IMedicalRecordRepository MedicalRecordRepository { get; private set; }
        public IReceptionistRepository ReceptionistRepository { get; private set; }
        public IPrescritpionRepository PrescritpionRepository { get; private set; }
        public IHealthReportRepository HealthReportRepository { get; private set; }
        public IPatientReportDocsRepository PatientReportDocsRepository { get; private set; }
        public IMedicinesRepository MedicinesRepository { get; private set; }
        public IGeneticDiseaseRepository GeneticDiseaseRepository { get; private set; }
        public IPatientDiseasesRepository PatientDiseasesRepository { get; private set; }
        public IPrescriptionItemRepository PrescriptionItemRepository { get; private set; }
        public IPatientAllergyRepository PatientAllergyRepository { get; private set; }
        public async Task<int> Complete()
        {
            return await _context.SaveChangesAsync();
        }

        public void Dispose()
        {
            _context.Dispose();
        }
    }
}
