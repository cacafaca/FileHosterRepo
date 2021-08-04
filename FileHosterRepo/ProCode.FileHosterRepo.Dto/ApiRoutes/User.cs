using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ProCode.FileHosterRepo.Common.ApiRoutes
{
    public static class User
    {
        public const string ControlerName = "User";
        
        public const string Register = "/User/Register";
        public const string Login = "/User/Login";
        public const string Logout = "/User/Logout";
        public const string Info = "/User/Info";
        public const string Update = "/User/Update";
        public const string Delete = "/User/Delete";
    }
}
