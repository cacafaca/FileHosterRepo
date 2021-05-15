﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Security.Principal;
using System.Threading.Tasks;

namespace ProCode.FileHosterRepo.Api
{
    public static class LoggedUser
    {
        #region Fields
        internal const string ClaimTypeNameUserId = "userId";
        #endregion

        public static int GetUserId(this ClaimsPrincipal user)
        {
            var userIdClaim = user.Claims.Where(c => c.Type == ClaimTypeNameUserId).FirstOrDefault();
            if (userIdClaim != null)
            {
                if (!int.TryParse(userIdClaim.Value, out int id))
                {
                    throw new ArgumentException($"Claim value {userIdClaim.Value} is not an integer.");
                }
                return id;
            }
            else
            {
                throw new ArgumentException($"Can't find claim {ClaimTypeNameUserId}.");
            }
        }

        public static string GetEmail(this ClaimsPrincipal user)
        {
            var userIdClaim = user.Claims.Where(c => c.Type == ClaimTypes.Email).FirstOrDefault();
            if (userIdClaim != null)
            {
                return userIdClaim.Value;
            }
            else
            {
                throw new ArgumentException($"Can't find claim {ClaimTypes.Email}.");
            }
        }
    }
}