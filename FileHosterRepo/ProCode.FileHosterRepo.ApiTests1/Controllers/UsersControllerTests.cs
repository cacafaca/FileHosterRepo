using FakeItEasy;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using ProCode.FileHosterRepo.Api.Controllers;
using ProCode.FileHosterRepo.ApiTests;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace ProCode.FileHosterRepo.Api.Controllers.Tests
{
    [TestClass]
    public class UserControllerTests
    {
        #region Constructors
        public UserControllerTests()
        {
            ApiTests.Config.RecreateDatabaseAsync().Wait();

            // Register administrator.
            HttpContent httpContent = new StringContent(string.Join("&", new string[]
            {
                "Email=" + Uri.EscapeDataString("admin@admin.com"),
                "Password=" + Uri.EscapeDataString("admin")
            }));
            httpContent.Headers.ContentType = new MediaTypeHeaderValue("application/x-www-form-urlencoded");
            ApiTests.Config.Client.PostAsync("/Admin/Register", httpContent).Wait();
        }
        #endregion

        [TestMethod()]
        public async Task Register_Single_User()
        {
            // Register user.
            HttpContent httpContent = new StringContent(string.Join("&", new string[]
            {
                "Email=" + Uri.EscapeDataString("singleuser@user.com"),
                "Password=" + Uri.EscapeDataString("singleuser"),
                "Nickname=" + Uri.EscapeDataString("SingleUser")
            }), Encoding.UTF8, "application/x-www-form-urlencoded");
            HttpResponseMessage response = await ApiTests.Config.Client.PostAsync("/Users/Register", httpContent);
            Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);
            var token = await response.Content.ReadAsStringAsync();
            Assert.IsTrue(!string.IsNullOrWhiteSpace(token));
            Assert.AreEqual(1, ApiTests.Config.DbContext.Users.Where(u => u.Role != Dal.Model.UserRole.Admin).Count());  // Expect one user.
        }

        [TestMethod()]
        public async Task Login_User()
        {
            // Register user.
            HttpContent httpContent = new StringContent(string.Join("&", new string[]
            {
                "Email=" + Uri.EscapeDataString("loginuser@user.com"),
                "Password=" + Uri.EscapeDataString("loginuser"),
                "Nickname=" + Uri.EscapeDataString("LoginUser")
            }), Encoding.UTF8, "application/x-www-form-urlencoded");
            HttpResponseMessage response = await ApiTests.Config.Client.PostAsync("/Users/Register", httpContent);
            Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);

            // Login user.
            httpContent = new StringContent(string.Join("&", new string[]
            {
                "Email=" + Uri.EscapeDataString("loginuser@user.com"),
                "Password=" + Uri.EscapeDataString("loginuser")
            }), Encoding.UTF8, "application/x-www-form-urlencoded");
            response = await ApiTests.Config.Client.PostAsync("/Users/Login", httpContent);
            Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);
            var token = await response.Content.ReadAsStringAsync();
            Assert.IsTrue(!string.IsNullOrWhiteSpace(token));
        }

        [TestMethod()]
        public async Task Get_User_Info()
        {
            // Register user.
            HttpContent httpContent = new StringContent(string.Join("&", new string[]
            {
                "Email=" + Uri.EscapeDataString("infouser@user.com"),
                "Password=" + Uri.EscapeDataString("infouser"),
                "Nickname=" + Uri.EscapeDataString("InfoUser")
            }), Encoding.UTF8, "application/x-www-form-urlencoded");
            HttpResponseMessage response = await ApiTests.Config.Client.PostAsync("/Users/Register", httpContent);
            Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);

            // Login user.
            httpContent = new StringContent(string.Join("&", new string[]
            {
                "Email=" + Uri.EscapeDataString("infouser@user.com"),
                "Password=" + Uri.EscapeDataString("infouser")
            }), Encoding.UTF8, "application/x-www-form-urlencoded");
            response = await ApiTests.Config.Client.PostAsync("/Users/Login", httpContent);
            Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);
            var token = await response.Content.ReadAsStringAsync();
            Assert.IsTrue(!string.IsNullOrWhiteSpace(token));

            // Get info.
            Config.Client.SetToken(token);
            response = await Config.Client.GetAsync("/Users/Info");
            Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);
            var userInfoJson = await response.Content.ReadAsStringAsync();
            var userInfo = JsonSerializer.Deserialize<Api.Model.Response.User>(userInfoJson, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
            Assert.IsNotNull(userInfo);
            Assert.AreEqual("InfoUser", userInfo.Nickname);
        }
        [TestMethod()]
        public async Task Login_And_Logout()
        {
            // Register user.
            HttpContent httpContent = new StringContent(string.Join("&", new string[]
            {
                "Email=" + Uri.EscapeDataString("logout@user.com"),
                "Password=" + Uri.EscapeDataString("logout"),
                "Nickname=" + Uri.EscapeDataString("Logout")
            }), Encoding.UTF8, "application/x-www-form-urlencoded");
            HttpResponseMessage response = await ApiTests.Config.Client.PostAsync("/Users/Register", httpContent);
            Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);

            // Login user.
            httpContent = new StringContent(string.Join("&", new string[]
            {
                "Email=" + Uri.EscapeDataString("logout@user.com"),
                "Password=" + Uri.EscapeDataString("logout")
            }), Encoding.UTF8, "application/x-www-form-urlencoded");
            response = await ApiTests.Config.Client.PostAsync("/Users/Login", httpContent);
            Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);
            var token = await response.Content.ReadAsStringAsync();
            Assert.IsTrue(!string.IsNullOrWhiteSpace(token));

            // Logout.
            Config.Client.SetToken(token);
            response = await Config.Client.GetAsync("/Users/Logout");
            Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);

            // Get info. Expect to fail.
            response = await Config.Client.GetAsync("/Users/Info");
            Assert.AreEqual(HttpStatusCode.Unauthorized, response.StatusCode);
        }
    }
}