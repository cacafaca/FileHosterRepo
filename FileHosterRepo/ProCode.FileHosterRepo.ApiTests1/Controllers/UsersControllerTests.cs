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
using Microsoft.EntityFrameworkCore;

namespace ProCode.FileHosterRepo.Api.Controllers.Tests
{
    [TestClass]
    public class UserControllerTests
    {
        #region Constructors
        public UserControllerTests()
        {
            Config.RecreateDatabaseAsync().Wait();  // Need empty database at beginning of each test method.

            // Register administrator. In order to register user administrator needs to be registered.
            Config.Client.PostAsync("/Admin/Register",
                new StringContent(string.Join("&", new string[] {
                    "Email=" + Uri.EscapeDataString("admin@admin.com"),
                    "Password=" + Uri.EscapeDataString("admin")
                }), Encoding.UTF8, Config.HttpMediaTypeForm)).Wait();
        }
        #endregion

        [TestMethod()]
        public async Task Register_Single_User()
        {
            // Register user.
            HttpResponseMessage response = await Config.Client.PostAsync("/Users/Register",
                new StringContent(string.Join("&", new string[] {
                    "Email=" + Uri.EscapeDataString("singleuser@user.com"),
                    "Password=" + Uri.EscapeDataString("singleuser"),
                    "Nickname=" + Uri.EscapeDataString("SingleUser")
                }), Encoding.UTF8, Config.HttpMediaTypeForm));
            Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);
            var token = await response.Content.ReadAsStringAsync();
            Assert.IsTrue(!string.IsNullOrWhiteSpace(token));
            Assert.AreEqual(1, Config.DbContext.Users.Where(u => u.Role != Dal.Model.UserRole.Admin).Count());  // Expect one user.
        }

        [TestMethod()]
        public async Task Login_User_Successful()
        {
            // Register user.
            HttpResponseMessage response = await Config.Client.PostAsync("/Users/Register",
                new StringContent(string.Join("&", new string[] {
                    "Email=" + Uri.EscapeDataString("loginuser@user.com"),
                    "Password=" + Uri.EscapeDataString("loginuser"),
                    "Nickname=" + Uri.EscapeDataString("LoginUser")
                }), Encoding.UTF8, Config.HttpMediaTypeForm));
            Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);

            // Login user.
            response = await Config.Client.PostAsync("/Users/Login",
                new StringContent(string.Join("&", new string[] {
                    "Email=" + Uri.EscapeDataString("loginuser@user.com"),
                    "Password=" + Uri.EscapeDataString("loginuser")
                }), Encoding.UTF8, Config.HttpMediaTypeForm));
            Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);
            var token = await response.Content.ReadAsStringAsync();
            Assert.IsTrue(!string.IsNullOrWhiteSpace(token));
        }

        [TestMethod()]
        public async Task Login_User_Unsuccessful_Bad_Password()
        {
            // Register user.
            HttpResponseMessage response = await Config.Client.PostAsync("/Users/Register",
                new StringContent(string.Join("&", new string[] {
                    "Email=" + Uri.EscapeDataString("loginuser@user.com"),
                    "Password=" + Uri.EscapeDataString("loginuser"),
                    "Nickname=" + Uri.EscapeDataString("LoginUser")
                }), Encoding.UTF8, Config.HttpMediaTypeForm));
            Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);

            // Login user.
            response = await Config.Client.PostAsync("/Users/Login",
                new StringContent(string.Join("&", new string[] {
                    "Email=" + Uri.EscapeDataString("loginuser@user.com"),
                    "Password=" + Uri.EscapeDataString("badpassword")
                }), Encoding.UTF8, Config.HttpMediaTypeForm));
            Assert.AreEqual(HttpStatusCode.Unauthorized, response.StatusCode);
        }

        [TestMethod()]
        public async Task Register_And_Logout()
        {
            // Register user.
            HttpResponseMessage response = await Config.Client.PostAsync("/Users/Register",
                new StringContent(string.Join("&", new string[] {
                    "Email=" + Uri.EscapeDataString("logout@user.com"),
                    "Password=" + Uri.EscapeDataString("logout"),
                    "Nickname=" + Uri.EscapeDataString("Logout")
                }), Encoding.UTF8, Config.HttpMediaTypeForm));
            Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);
            var token = await response.Content.ReadAsStringAsync();

            // Logout.
            Config.Client.SetToken(token);
            response = await Config.Client.GetAsync("/Users/Logout");
            Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);

            // Get info. Expect to fail.
            response = await Config.Client.GetAsync("/Users/Info");
            Assert.AreEqual(HttpStatusCode.Unauthorized, response.StatusCode);
        }

        [TestMethod()]
        public async Task Login_And_Logout()
        {
            // Register user.
            HttpResponseMessage response = await Config.Client.PostAsync("/Users/Register",
                new StringContent(string.Join("&", new string[] {
                    "Email=" + Uri.EscapeDataString("logout@user.com"),
                    "Password=" + Uri.EscapeDataString("logout"),
                    "Nickname=" + Uri.EscapeDataString("Logout")
                }), Encoding.UTF8, Config.HttpMediaTypeForm));
            Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);

            // Login user.
            response = await Config.Client.PostAsync("/Users/Login",
                new StringContent(string.Join("&", new string[] {
                    "Email=" + Uri.EscapeDataString("logout@user.com"),
                    "Password=" + Uri.EscapeDataString("logout")
                }), Encoding.UTF8, Config.HttpMediaTypeForm));
            Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);
            var token = await response.Content.ReadAsStringAsync();

            // Logout.
            Config.Client.SetToken(token);
            response = await Config.Client.GetAsync("/Users/Logout");
            Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);

            // Get info. Expect to fail.
            response = await Config.Client.GetAsync("/Users/Info");
            Assert.AreEqual(HttpStatusCode.Unauthorized, response.StatusCode);
        }

        [TestMethod()]
        public async Task Update_Nickname_And_Password()
        {
            // Register user.
            HttpResponseMessage response = await Config.Client.PostAsync("/Users/Register",
                new StringContent(string.Join("&", new string[] {
                    "Email=" + Uri.EscapeDataString("update@user.com"),
                    "Password=" + Uri.EscapeDataString("firstpass"),
                    "Nickname=" + Uri.EscapeDataString("FirstNickname")
                }), Encoding.UTF8, Config.HttpMediaTypeForm));
            Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);
            var token = await response.Content.ReadAsStringAsync();

            // Update user's nickname and password.
            var userBefore = (await Config.DbContext.Users.SingleOrDefaultAsync(
                u => u.Email == "update@user.com" &&
                u.Role != Dal.Model.UserRole.Admin));
            Config.Client.SetToken(token);
            response = await Config.Client.PatchAsync("/Users/Update",
                new StringContent(string.Join("&", new string[] {
                    "Nickname=" + Uri.EscapeDataString("UpdatedNickname"),
                    "Password=" + Uri.EscapeDataString("updatedpassword")
                }), Encoding.UTF8, Config.HttpMediaTypeForm));
            Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);
            var userAfter = (await Config.DbContext.Users.AsNoTracking()
                .SingleOrDefaultAsync(u =>
                    u.Email == "update@user.com" &&
                    u.Role != Dal.Model.UserRole.Admin));
            Assert.AreNotEqual(userBefore.Nickname, userAfter.Nickname);
            Assert.AreNotEqual(userBefore.Password, userAfter.Password);
        }

        [TestMethod()]
        public async Task Get_User_Info()
        {
            // Register user.
            HttpResponseMessage response = await Config.Client.PostAsync("/Users/Register",
                new StringContent(string.Join("&", new string[] {
                    "Email=" + Uri.EscapeDataString("infouser@user.com"),
                    "Password=" + Uri.EscapeDataString("infouser"),
                    "Nickname=" + Uri.EscapeDataString("InfoUser")
                }), Encoding.UTF8, Config.HttpMediaTypeForm));
            Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);

            // Login user.
            response = await Config.Client.PostAsync("/Users/Login",
                new StringContent(string.Join("&", new string[] {
                    "Email=" + Uri.EscapeDataString("infouser@user.com"),
                    "Password=" + Uri.EscapeDataString("infouser")
                }), Encoding.UTF8, Config.HttpMediaTypeForm));
            Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);
            var token = await response.Content.ReadAsStringAsync();

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
        public async Task Delete_User()
        {
            // Register user.
            HttpResponseMessage response = await Config.Client.PostAsync("/Users/Register",
                new StringContent(string.Join("&", new string[] {
                    "Email=" + Uri.EscapeDataString("deleteuser@user.com"),
                    "Password=" + Uri.EscapeDataString("deleteuser"),
                    "Nickname=" + Uri.EscapeDataString("DeleteUser")
                }), Encoding.UTF8, Config.HttpMediaTypeForm));
            Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);
            var token = await response.Content.ReadAsStringAsync();

            // Delete user.
            Config.Client.SetToken(token);
            response = await Config.Client.DeleteAsync("/Users/Delete");
            Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);

            Assert.IsNull(await Config.DbContext.Users.SingleOrDefaultAsync(
                u => u.Email == "deleteuser@user.com"));
        }
    }
}