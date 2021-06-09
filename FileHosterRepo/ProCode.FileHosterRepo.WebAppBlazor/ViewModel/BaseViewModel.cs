using System.Net.Http;

namespace ProCode.FileHosterRepo.WebAppBlazor.ViewModel
{
    public class BaseViewModel
    {
        #region Fields
        protected readonly HttpClient httpClient;
        #endregion

        public BaseViewModel()
        {
        }

        public BaseViewModel(HttpClient httpClient)
        {
            this.httpClient = httpClient;
        }

        public void SetToken(string token = null)
        {
            httpClient.DefaultRequestHeaders.Remove("Authorization");
            if (token != null)
                httpClient.DefaultRequestHeaders.Add("Authorization", "Bearer " + token);
        }

        public void ClearToken()
        {
            SetToken();
        }
    }
}
