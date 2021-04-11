using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ProCode.FileHosterRepo.Api.Models
{
    public class UserWithToken : Dal.Model.User
    {
        public string AccessToken { get; set; }
        public string RefreshToken { get; set; }
        public UserWithToken(Dal.Model.User user)
        {
            Id = user.Id;
            Email = user.Email;
            Nickname = user.Nickname;
            Created = user.Created;
        }
    }
}
