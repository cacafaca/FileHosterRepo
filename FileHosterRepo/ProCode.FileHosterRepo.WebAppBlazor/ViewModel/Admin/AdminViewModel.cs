using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Net.Http.Json;
using ProCode.FileHosterRepo.Common.Api.Response;

namespace ProCode.FileHosterRepo.WebAppBlazor.ViewModel.Admin
{
    public class AdminViewModel : BaseViewModel, IAdminViewModel
    {
        #region Fields
#if RELEASE
        private const int passwordMinimumLength = 6;
#endif
        #endregion

        #region Constructors
        public AdminViewModel() { }
        public AdminViewModel(System.Net.Http.IHttpClientFactory httpClientFactory) : base(httpClientFactory) { }
#endregion

        public async Task<bool> RegisterAsync(Common.Api.Request.UserRegister userRegister, string confirmPassword)
        {
            // Validations
            if (string.IsNullOrWhiteSpace(userRegister.Email) || string.IsNullOrWhiteSpace(userRegister.Password) || string.IsNullOrWhiteSpace(confirmPassword))
            {
                throw new Exception("Please fill all fields.");
            }
#if RELEASE
            if (userRegister.Password.Length < passwordMinimumLength)
            {
                throw new Exception("Password must be at least {passwordMinimumLength} characters.");
            }
#endif
            if (userRegister.Password != confirmPassword)
            {
                throw new Exception("Silly, you must retype same password in Confirm password field. :)");
            }

            var response = await HttpClient.PostAsJsonAsync(Common.Routes.Admin.Register, userRegister);
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
            var response = await HttpClient.PostAsJsonAsync(Common.Routes.Admin.Login, user);
            if (response.StatusCode == System.Net.HttpStatusCode.OK)
            {
                SetToken(await response.Content.ReadAsStringAsync());
                return true;
            }
            else
                return false;
        }

        public async Task<Common.Api.Response.User> GetInfoAsync()
        {
            return await HttpClient.GetFromJsonAsync<Common.Api.Response.User>(Common.Routes.Admin.Info);
        }

        public async Task<bool> IsRegisteredAsync()
        {
            return await HttpClient.GetFromJsonAsync<bool>(Common.Routes.Admin.IsRegistered);
        }

        public async Task<string> LogoutAsync()
        {
            var x = await HttpClient.GetAsync(Common.Routes.Admin.Logout);
            var m = await x.Content.ReadAsStringAsync();
            //var res = await HttpClient.GetFromJsonAsync<string>("Admin/Logout");
            ClearToken();
            //return res;
            return m;
        }
    }
}
