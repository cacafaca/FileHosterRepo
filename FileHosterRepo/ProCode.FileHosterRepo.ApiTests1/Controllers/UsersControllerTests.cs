using FakeItEasy;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using ProCode.FileHosterRepo.ApiTests;
using System;
using System.Linq;
using System.Net;
using System.Net.Http;
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
                new StringContent(
                    @"{ 
                        ""Email"": ""admin@admin.com"",
                        ""Password"": ""admin""
                    }",
                Encoding.UTF8, Config.HttpMediaTypeJson)).Wait();
        }
        #endregion

        [TestMethod()]
        public async Task Register_Single_User()
        {
            // Register user.
            HttpResponseMessage response = await Config.Client.PostAsync("/Users/Register",
                new StringContent(
                    @"{ 
                        ""Email"": ""user@user.com"",
                        ""Password"": ""user"",
                        ""Nickname"": ""User""
                    }",
                Encoding.UTF8, Config.HttpMediaTypeJson));
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
                new StringContent(
                    @"{ 
                        ""Email"": ""user@user.com"",
                        ""Password"": ""user"",
                        ""Nickname"": ""User""
                    }",
                Encoding.UTF8, Config.HttpMediaTypeJson));
            Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);

            // Login user.
            response = await Config.Client.PostAsync("/Users/Login",
                new StringContent(
                    @"{ 
                        ""Email"": ""user@user.com"",
                        ""Password"": ""user""
                    }",
                Encoding.UTF8, Config.HttpMediaTypeJson));
            Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);
            var token = await response.Content.ReadAsStringAsync();
            Assert.IsTrue(!string.IsNullOrWhiteSpace(token));
        }

        [TestMethod()]
        public async Task Login_User_Unsuccessful_Bad_Password()
        {
            // Register user.
            HttpResponseMessage response = await Config.Client.PostAsync("/Users/Register",
                new StringContent(
                    @"{ 
                        ""Email"": ""user@user.com"",
                        ""Password"": ""user"",
                        ""Nickname"": ""User""
                    }",
                Encoding.UTF8, Config.HttpMediaTypeJson));
            Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);

            // Login user.
            response = await Config.Client.PostAsync("/Users/Login",
                new StringContent(
                    @"{ 
                        ""Email"": ""user@user.com"",
                        ""Password"": ""bad_password""
                    }",
                Encoding.UTF8, Config.HttpMediaTypeJson));
            Assert.AreEqual(HttpStatusCode.Unauthorized, response.StatusCode);
        }

        [TestMethod()]
        public async Task Register_And_Logout()
        {
            // Register user.
            HttpResponseMessage response = await Config.Client.PostAsync("/Users/Register",
                new StringContent(
                    @"{ 
                        ""Email"": ""user@user.com"",
                        ""Password"": ""user"",
                        ""Nickname"": ""User""
                    }",
                Encoding.UTF8, Config.HttpMediaTypeJson));
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
                new StringContent(
                    @"{ 
                        ""Email"": ""user@user.com"",
                        ""Password"": ""user"",
                        ""Nickname"": ""User""
                    }",
                Encoding.UTF8, Config.HttpMediaTypeJson));
            Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);

            // Login user.
            response = await Config.Client.PostAsync("/Users/Login",
                new StringContent(
                    @"{ 
                        ""Email"": ""user@user.com"",
                        ""Password"": ""user""
                    }",
                Encoding.UTF8, Config.HttpMediaTypeJson));
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
                new StringContent(
                    @"{ 
                        ""Email"": ""user@user.com"",
                        ""Password"": ""user"",
                        ""Nickname"": ""User""
                    }",
                Encoding.UTF8, Config.HttpMediaTypeJson));
            Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);
            var token = await response.Content.ReadAsStringAsync();

            // Update user's nickname and password.
            var userBefore = (await Config.DbContext.Users.SingleOrDefaultAsync(
                u => u.Email == "user@user.com" &&
                u.Role != Dal.Model.UserRole.Admin));
            Config.Client.SetToken(token);
            response = await Config.Client.PatchAsync("/Users/Update",
                new StringContent(
                    @"{ 
                        ""Password"": ""new_password"",
                        ""Nickname"": ""New_Nickname""
                    }",
                Encoding.UTF8, Config.HttpMediaTypeJson));
            Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);
            var userAfter = (await Config.DbContext.Users.AsNoTracking()
                .SingleOrDefaultAsync(u =>
                    u.Email == "user@user.com" &&
                    u.Role != Dal.Model.UserRole.Admin));
            Assert.AreNotEqual(userBefore.Nickname, userAfter.Nickname);
            Assert.AreNotEqual(userBefore.Password, userAfter.Password);
        }

        [TestMethod()]
        public async Task Get_User_Info()
        {
            // Register user.
            HttpResponseMessage response = await Config.Client.PostAsync("/Users/Register",
                new StringContent(
                    @"{ 
                        ""Email"": ""user@user.com"",
                        ""Password"": ""user"",
                        ""Nickname"": ""User""
                    }",
                Encoding.UTF8, Config.HttpMediaTypeJson));
            Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);

            // Login user.
            response = await Config.Client.PostAsync("/Users/Login",
                new StringContent(
                    @"{ 
                        ""Email"": ""user@user.com"",
                        ""Password"": ""user""
                    }",
                Encoding.UTF8, Config.HttpMediaTypeJson));
            Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);
            var token = await response.Content.ReadAsStringAsync();

            // Get info.
            Config.Client.SetToken(token);
            response = await Config.Client.GetAsync("/Users/Info");
            Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);
            var userInfoJson = await response.Content.ReadAsStringAsync();
            var userInfo = JsonSerializer.Deserialize<Api.Model.Response.User>(userInfoJson, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
            Assert.IsNotNull(userInfo);
            Assert.AreEqual("User", userInfo.Nickname);
        }
        [TestMethod()]
        public async Task Delete_User()
        {
            // Register user.
            HttpResponseMessage response = await Config.Client.PostAsync("/Users/Register",
                new StringContent(
                    @"{ 
                        ""Email"": ""user@user.com"",
                        ""Password"": ""user"",
                        ""Nickname"": ""User""
                    }",
                Encoding.UTF8, Config.HttpMediaTypeJson));
            Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);
            var token = await response.Content.ReadAsStringAsync();

            // Delete user.
            Config.Client.SetToken(token);
            response = await Config.Client.DeleteAsync("/Users/Delete");
            Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);

            Assert.IsNull(await Config.DbContext.Users.SingleOrDefaultAsync(
                u => u.Email == "user@user.com"));
        }
    }
}