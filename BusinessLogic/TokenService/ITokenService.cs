using DataAccess.DBEntities;
using System;
using System.Collections.Generic;
using System.Text;

namespace BusinessLogic.TokenService
{
    public interface ITokenService
    {
        string CreateToken(User user);
    }
}
