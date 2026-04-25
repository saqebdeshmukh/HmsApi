using BusinessLogic.GenericRepository;
using DataAccess.DBEntities;
using System;
using System.Collections.Generic;
using System.Text;

namespace DataAccess.RefreshTokenRepository
{
    public interface IRefreshTokenRepository:IGenericRepository<RefreshToken>
    {
    }
}
