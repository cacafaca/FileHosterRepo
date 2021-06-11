using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using System.Net.Http.Json;

namespace ProCode.FileHosterRepo.WebAppBlazor.ViewModel
{
    public class IndexViewModel : BaseViewModel, IIndexViewModel
    {
        public IndexViewModel() { }

        public IndexViewModel(IHttpClientFactory httpClientFactory) : base(httpClientFactory) { }

        public async Task<IList<Dto.Api.Response.MediaHeader>> GetLast10Async()
        {
            return await HttpClient.GetFromJsonAsync<IList<Dto.Api.Response.MediaHeader>>("Media/List10");
        }
    }
}
