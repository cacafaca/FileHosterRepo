using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Net.Http.Json;
using ProCode.FileHosterRepo.Dto.Api.Response;

namespace ProCode.FileHosterRepo.WebAppBlazor.ViewModel.Admin
{
    public class AdminViewModel : BaseViewModel, IAdminViewModel
    {
        #region Constructors
        public AdminViewModel() { }
        public AdminViewModel(System.Net.Http.IHttpClientFactory httpClientFactory) : base(httpClientFactory) { }
        #endregion

        public async Task<bool> RegisterAsync(Dto.Api.Request.UserRegister userRegister)
        {
            var response = await HttpClient.PostAsJsonAsync("/Admin/Register", userRegister);
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
            var response = await HttpClient.PostAsJsonAsync("/Admin/Login", userRegister);
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
            return await HttpClient.GetFromJsonAsync<Dto.Api.Response.User>("/Admin/Info");
        }

        public async Task<bool> IsRegisteredAsync()
        {
            return await HttpClient.GetFromJsonAsync<bool>("Admin/IsRegistered");
        }

        public async Task<string> Logout()
        {
            var x = await HttpClient.GetAsync("Admin/Logout");
            var m = await x.Content.ReadAsStringAsync();
            //var res = await HttpClient.GetFromJsonAsync<string>("Admin/Logout");
            ClearToken();
            //return res;
            return m;
        }
    }
}
