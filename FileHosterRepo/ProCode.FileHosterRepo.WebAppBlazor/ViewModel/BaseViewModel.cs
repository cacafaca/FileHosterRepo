using System.Net.Http;

namespace ProCode.FileHosterRepo.WebAppBlazor.ViewModel
{
    public class BaseViewModel : IIsLogged
    {
        #region Fields
        public const string HttpClientName = "WebAPI";
        private const string authorizationHeaderName = "Authorization";
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
            httpClient.DefaultRequestHeaders.Remove(authorizationHeaderName);
            if (token != null)
                httpClient.DefaultRequestHeaders.Add(authorizationHeaderName, "Bearer " + token);
        }

        public void ClearToken()
        {
            SetToken();
        }

        public bool IsLoggedIn()
        {
            return httpClient.DefaultRequestHeaders.Contains(authorizationHeaderName);
        }

        #endregion
    }
}
