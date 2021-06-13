using System.Net.Http;

namespace ProCode.FileHosterRepo.WebAppBlazor.ViewModel
{
    public class BaseViewModel : IIsLogged, IToken
    {
        #region Fields
        public const string HttpClientName = "WebAPI";
        private const string authorizationSchemeName = "Bearer";
        private readonly HttpClient httpClient;
        #endregion

        #region Constructors
        public BaseViewModel() { }

        public BaseViewModel(IHttpClientFactory httpClientFactory)
        {
            this.httpClient = httpClientFactory.CreateClient(HttpClientName);
        }
        #endregion

        #region Properties
        public HttpClient HttpClient { get { return httpClient; } }
        #endregion

        #region Methods
        public void SetToken(string token = null)
        {
            if (token != null)
                httpClient.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue(authorizationSchemeName, token);
        }

        public void ClearToken()
        {
            httpClient.DefaultRequestHeaders.Authorization = null;
        }

        /// <summary>
        /// Potentially dangerous! Consider otherwise.
        /// </summary>
        /// <returns></returns>
        public bool IsLoggedIn()
        {
            return httpClient.DefaultRequestHeaders.Authorization != null;
        }

        public string GetToken()
        {
            return httpClient.DefaultRequestHeaders.Authorization.Parameter;
        }
        #endregion
    }
}
