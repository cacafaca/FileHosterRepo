using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ProCode.FileHosterRepo.Api
{
    public static class Mapper
    {
        public static Model.Response.User MapReponseUser(this Dal.Model.User user)
        {
            return new Model.Response.User
            {
                UserId = user.UserId,
                Nickname = user.Nickname,
                Role = user.Role,
                Created = user.Created
            };
        }
    }
}
