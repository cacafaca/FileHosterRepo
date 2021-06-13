using System.Threading.Tasks;
using System.Net.Http.Json;
using Request = ProCode.FileHosterRepo.Common.Api.Request;
using Response = ProCode.FileHosterRepo.Common.Api.Response;
using System.Text.Json;
using System.IO;

namespace ProCode.FileHosterRepo.WebAppBlazor.ViewModel.Media
{
    public class MediaViewModel : BaseViewModel, IMediaViewModel
    {
        #region Fields
        private readonly User.IUserViewModel userViewModel;
        private Response.MediaHeader cacheMediaHeader;
        #endregion

        #region Constructors
        public MediaViewModel() { }
        public MediaViewModel(System.Net.Http.IHttpClientFactory httpClientFactory, User.IUserViewModel userViewModel) : base(httpClientFactory)
        {
            this.userViewModel = userViewModel;
        }
        #endregion

        public async Task<Response.MediaHeader> AddAsync(Request.MediaHeader mediaHeader)
        {
            UpdateToken();
            var response = await HttpClient.PostAsJsonAsync(Common.Routes.Media.Add, mediaHeader);
            //response.EnsureSuccessStatusCode();
            if (response.StatusCode == System.Net.HttpStatusCode.OK)
            {
                var responseMediaHeader = JsonSerializer.Deserialize<Response.MediaHeader>(await response.Content.ReadAsStringAsync());
                return responseMediaHeader;
            }
            else
            {
                throw new System.Exception(await response.Content.ReadAsStringAsync());
            }
        }

        public void WriteCacheMediaHeader(Response.MediaHeader mediaHeader)
        {
            cacheMediaHeader = mediaHeader;
        }

        public Response.MediaHeader ReadCacheMediaHeader()
        {
            var cacheCopy = cacheMediaHeader;
            cacheMediaHeader = null;
            return cacheCopy;
        }

        private void UpdateToken()
        {
            SetToken(userViewModel.GetToken());
        }
    }
}
