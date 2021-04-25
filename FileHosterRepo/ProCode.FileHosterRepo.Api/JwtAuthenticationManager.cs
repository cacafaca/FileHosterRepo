using Microsoft.IdentityModel.Tokens;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ProCode.FileHosterRepo.Api
{
    public class JwtAuthenticationManager : IJwtAuthenticationManager
    {
        #region Fields
        string secretKey;
        #endregion

        #region Constructors
        public JwtAuthenticationManager(string secretKey)
        {
            this.secretKey = secretKey;
        }
        #endregion

        #region Methods
        public SymmetricSecurityKey GetSymmetricSecurityKey()
        {
            return new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey));
        }
        #endregion
    }
}