using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ProCode.FileHosterRepo.Common.ApiRoutes
{
    public static class Media
    {
        public const string ControlerName = "Media";
        
        public const string Add = "Media/Add";
        public const string Get = "Media/Get/{mediaHeaderId}";
        public const string Update = "Media/Update";
        public const string Last10 = "Media/Last10";
        public const string Last10Anonymous = "Media/Last10Anonymous";

        public static string GetId(int id)
        {
            return Get.Replace("{mediaHeaderId}", id.ToString());
        }
    }
}
