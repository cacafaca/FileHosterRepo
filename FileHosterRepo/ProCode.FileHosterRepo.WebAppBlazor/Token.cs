using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ProCode.FileHosterRepo.WebAppBlazor
{
    public class Token
    {
        private string tokenValue;

        public string Value
        {
            get { return tokenValue; }
            set { this.tokenValue = value; }
        }
    }
}
