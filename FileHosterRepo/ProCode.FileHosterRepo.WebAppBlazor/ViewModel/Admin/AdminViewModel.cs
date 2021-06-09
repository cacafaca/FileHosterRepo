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
        public async Task<bool> RegisterAsync(Dto.Api.Request.UserRegister userRegister)
        {
            var response = await httpClient.PostAsJsonAsync("/Admin/Register", userRegister);
            if (response.StatusCode == System.Net.HttpStatusCode.OK)
            {
                SetToken(await response.Content.ReadAsStringAsync());
                return true;
            }
            else
                return false;
        }
     
        public async Task<User> GetInfoAsync()
        {
            return await httpClient.GetFromJsonAsync<User>("/Admin/Info");
        }
    }
}
