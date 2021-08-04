using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Net.Http.Json;
using ProCode.FileHosterRepo.Common.Api.Response;

namespace ProCode.FileHosterRepo.WebAppBlazor.ViewModel.User
{
    public class UserViewModel : BaseViewModel, IUserViewModel
    {
        #region Fields
        private readonly Admin.IAdminViewModel adminViewModel;
        #endregion

        #region Constructors
        public UserViewModel() { }
        public UserViewModel(System.Net.Http.IHttpClientFactory httpClientFactory, Admin.IAdminViewModel adminViewModel) : base(httpClientFactory)
        {
            this.adminViewModel = adminViewModel;
        }
        #endregion

        public async Task<bool> RegisterAsync(Common.Api.Request.UserRegister userRegister, string confirmPassword)
        {
            var response = await HttpClient.PostAsJsonAsync(Common.ApiRoutes.User.Register, userRegister);
            if (response.StatusCode == System.Net.HttpStatusCode.OK)
            {
                SetToken(await response.Content.ReadAsStringAsync());
                return true;
            }
            else
                return false;
        }

        public async Task<bool> LoginAsync(Common.Api.Request.User user)
        {
            var response = await HttpClient.PostAsJsonAsync(Common.ApiRoutes.User.Login, user);
            if (response.StatusCode == System.Net.HttpStatusCode.OK)
            {
                var token = await response.Content.ReadAsStringAsync();
                SetToken(token);
                if (await IsUserAdminAsync())
                    adminViewModel.SetToken(token);
                return true;
            }
            else
                return false;
        }

        public async Task<string> LogoutAsync()
        {
            var response = await HttpClient.GetAsync(Common.ApiRoutes.User.Logout);
            response.EnsureSuccessStatusCode();
            ClearToken();
            var message = await response.Content.ReadAsStringAsync();
            return message;
        }

        public async Task<Common.Api.Response.User> GetInfoAsync()
        {
            return await HttpClient.GetFromJsonAsync<Common.Api.Response.User>(Common.ApiRoutes.User.Info);
        }

        public async Task<bool> IsAdminRegistredAsync()
        {
            return await adminViewModel.IsRegisteredAsync();
        }

        public async Task<bool> IsUserAdminAsync()
        {
            return (await GetInfoAsync()).Role == Common.User.UserRole.Admin;
        }

    }
}
