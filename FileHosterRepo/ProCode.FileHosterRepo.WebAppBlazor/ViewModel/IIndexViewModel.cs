using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ProCode.FileHosterRepo.WebAppBlazor.ViewModel
{
    public interface IIndexViewModel
    {
        public Task<IList<Dto.Api.Response.MediaHeader>> GetLast10Async();
    }
}
