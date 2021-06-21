using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ProCode.FileHosterRepo.WebAppBlazor
{
    public class UnknownEnvironmentException: Exception
    {
        readonly string environment;
        public UnknownEnvironmentException(string environment)
        {
            this.environment = environment;
        }
        public override string Message => $"Unknown environment '{environment}'.";
    }
}
