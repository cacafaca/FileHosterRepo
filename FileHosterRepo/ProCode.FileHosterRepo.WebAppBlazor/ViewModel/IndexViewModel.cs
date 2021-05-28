using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using System.Net.Http.Json;

namespace ProCode.FileHosterRepo.WebAppBlazor.ViewModel
{
    public class IndexViewModel : IIndexViewModel
    {
        #region Fields
        private readonly HttpClient httpClient;
        #endregion

        public IndexViewModel()
        {
        }

        public IndexViewModel(HttpClient httpClient)
        {
            this.httpClient = httpClient;
        }

        public async Task<IList<Dto.Api.Response.MediaHeader>> GetLast10Async()
        {
            return await httpClient.GetFromJsonAsync<IList<Dto.Api.Response.MediaHeader>>("/Media/List10");
        }
        public async Task<string> GetLast10StringAsync()
        {
            //return await httpClient.GetFromJsonAsync<string>("/Media/List10");
            try
            {
                var resp = await httpClient.GetAsync("/Media/List10");
                var respStr = await resp.Content.ReadAsStringAsync();
                return respStr;
            }
            catch (Exception ex)
            {
                throw new Exception(httpClient.BaseAddress + ex.Message);
            }
            //return httpClient.BaseAddress.AbsoluteUri;
        }
    }
}
