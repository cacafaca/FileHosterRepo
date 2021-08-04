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

        public async Task<IList<Common.Api.Response.MediaHeader>> GetLast10AnonymousAsync()
        {
            return await HttpClient.GetFromJsonAsync<IList<Common.Api.Response.MediaHeader>>(Common.ApiRoutes.Media.Last10Anonymous);
        }
    }
}
