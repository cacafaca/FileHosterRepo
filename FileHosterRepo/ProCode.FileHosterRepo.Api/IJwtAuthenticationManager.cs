using Microsoft.IdentityModel.Tokens;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ProCode.FileHosterRepo.Api
{
    public interface IJwtAuthenticationManager
    {
        SymmetricSecurityKey GetSymmetricSecurityKey();
    }
}
