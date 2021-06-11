using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Net.Http.Json;
using ProCode.FileHosterRepo.Dto.Api.Response;

namespace ProCode.FileHosterRepo.WebAppBlazor.ViewModel.User
{
    public class UserViewModel : BaseViewModel, IUserViewModel
    {
        #region Constructors
        public UserViewModel() { }
        public UserViewModel(System.Net.Http.IHttpClientFactory httpClientFactory) : base(httpClientFactory) { }
        #endregion

        public async Task<bool> RegisterAsync(Dto.Api.Request.UserRegister userRegister)
        {
            var response = await HttpClient.PostAsJsonAsync("/User/Register", userRegister);
            if (response.StatusCode == System.Net.HttpStatusCode.OK)
            {
                SetToken(await response.Content.ReadAsStringAsync());
                return true;
            }
            else
                return false;
        }

        public async Task<bool> LoginAsync(Dto.Api.Request.UserRegister userRegister)
        {
            var response = await HttpClient.PostAsJsonAsync("/User/Login", userRegister);
            if (response.StatusCode == System.Net.HttpStatusCode.OK)
            {
                SetToken(await response.Content.ReadAsStringAsync());
                return true;
            }
            else
                return false;
        }

        public async Task<Dto.Api.Response.User> GetInfoAsync()
        {
            return await HttpClient.GetFromJsonAsync<Dto.Api.Response.User>("/User/Info");
        }
    }
}
