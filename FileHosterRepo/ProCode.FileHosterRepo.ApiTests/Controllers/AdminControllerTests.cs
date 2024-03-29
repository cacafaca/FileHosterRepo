﻿using Microsoft.VisualStudio.TestTools.UnitTesting;
using ProCode.FileHosterRepo.ApiTests;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;

namespace ProCode.FileHosterRepo.Api.Controllers.Tests
{
    /// <summary>
    /// Tests Administrator API.
    /// Great video about integration testing at https://www.youtube.com/watch?v=7roqteWLw4s.
    /// </summary>
    [TestClass()]
    public class AdminControllerTests
    {
        public AdminControllerTests()
        {
            Config.RecreateDatabaseAsync().Wait();  // Need empty database for each test method.
        }

        [TestMethod()]
        public async Task Register_Admin_On_A_Cleen_Database()
        {
            // Register administrator.            
            var token = await RegisterAdminAsync();
            Assert.IsTrue(!string.IsNullOrWhiteSpace(token));
            Assert.AreEqual(1, Config.DbContext.Users.Count());  // Expect one user.
            Assert.AreEqual(1, Config.DbContext.Users.Where(u => u.Role == Common.User.UserRole.Admin).Count()); // Expect one administrator.
        }

        [TestMethod()]
        public async Task Register_Two_Admins_And_Fail()
        {
            // Register first administrator.
            HttpResponseMessage response = await Config.Client.PostAsync(Common.ApiRoutes.Admin.Register,
                new StringContent(
                    @"{ 
                        ""Email"": ""admin1@admin.com"",
                        ""Password"": ""admin1""
                    }",
                Encoding.UTF8, Config.HttpMediaTypeJson));
            Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);

            // Register second administrator.
            response = await Config.Client.PostAsync(Common.ApiRoutes.Admin.Register,
                new StringContent(
                    @"{ 
                        ""Email"": ""admin2@admin.com"",
                        ""Password"": ""admin2""
                    }",
                Encoding.UTF8, Config.HttpMediaTypeJson));
            Assert.AreEqual(HttpStatusCode.Conflict, response.StatusCode);
            var responseStr = await response.Content.ReadAsStringAsync();
            Assert.IsTrue(responseStr.Contains("admin1@admin.com"));
        }

        [TestMethod]
        public async Task Login_Successful()
        {
            // Register administrator.
            await RegisterAdminAsync();

            // Login administrator.
            var response = await Config.Client.PostAsync(Common.ApiRoutes.Admin.Login,
                new StringContent(
                    @"{ 
                        ""Email"": ""admin@admin.com"",
                        ""Password"": ""admin""
                    }",
                Encoding.UTF8, Config.HttpMediaTypeJson));
            Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);
            var token = await response.Content.ReadAsStringAsync();
            Assert.IsTrue(!string.IsNullOrWhiteSpace(token));
        }

        [TestMethod]
        public async Task Login_Unsuccessfull_Bad_Password()
        {
            // Register administrator.
            HttpResponseMessage response = await Config.Client.PostAsync(Common.ApiRoutes.Admin.Register,
                new StringContent(
                    @"{ 
                        ""Email"": ""admin@admin.com"",
                        ""Password"": ""admin""
                    }",
                Encoding.UTF8, Config.HttpMediaTypeJson));
            Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);

            // Login administrator with wrong password.
            response = await Config.Client.PostAsync(Common.ApiRoutes.Admin.Login,
                new StringContent(
                    @"{ 
                        ""Email"": ""admin@admin.com"",
                        ""Password"": ""badpassword""
                    }",
                Encoding.UTF8, Config.HttpMediaTypeJson));
            Assert.AreEqual(HttpStatusCode.Unauthorized, response.StatusCode);
        }

        [TestMethod]
        public async Task Register_And_Logout()
        {
            // Register administrator.
            HttpResponseMessage response = await Config.Client.PostAsync(Common.ApiRoutes.Admin.Register,
                new StringContent(
                    @"{ 
                        ""Email"": ""admin@admin.com"",
                        ""Password"": ""admin""
                    }",
                Encoding.UTF8, Config.HttpMediaTypeJson));
            Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);
            var token = await response.Content.ReadAsStringAsync();

            // Logout administrator.
            Config.Client.SetToken(token);
            response = await Config.Client.GetAsync(Common.ApiRoutes.Admin.Logout);
            Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);

            // Try to get Administrator info. Expect to fail.
            response = await Config.Client.GetAsync(Common.ApiRoutes.Admin.Info);
            Assert.AreEqual(HttpStatusCode.Unauthorized, response.StatusCode);
            var errorMsg = await response.Content.ReadAsStringAsync();
            Common.Util.Trace(errorMsg);
        }

        [TestMethod]
        public async Task Login_And_Logout()
        {
            // Register administrator.
            HttpResponseMessage response = await Config.Client.PostAsync(Common.ApiRoutes.Admin.Register,
                new StringContent(
                    @"{ 
                        ""Email"": ""admin@admin.com"",
                        ""Password"": ""admin""
                    }",
                Encoding.UTF8, Config.HttpMediaTypeJson));
            Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);

            // Login administrator.
            response = await Config.Client.PostAsync(Common.ApiRoutes.Admin.Login,
                new StringContent(
                    @"{ 
                        ""Email"": ""admin@admin.com"",
                        ""Password"": ""admin""
                    }",
                Encoding.UTF8, Config.HttpMediaTypeJson));
            Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);
            var token = await response.Content.ReadAsStringAsync();

            // Logout administrator.
            Config.Client.SetToken(token);
            response = await Config.Client.GetAsync(Common.ApiRoutes.Admin.Logout);
            Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);

            // Try to get Administrator info. Expect to fail.
            response = await Config.Client.GetAsync(Common.ApiRoutes.Admin.Info);
            Assert.AreEqual(HttpStatusCode.Unauthorized, response.StatusCode);
            var errorMsg = await response.Content.ReadAsStringAsync();
            Common.Util.Trace(errorMsg);
        }

        [TestMethod]
        public async Task Update_Password()
        {
            // Register administrator.
            HttpResponseMessage response = await Config.Client.PostAsync(Common.ApiRoutes.Admin.Register,
                new StringContent(
                    @"{ 
                        ""Email"": ""admin@admin.com"",
                        ""Password"": ""admin""
                    }",
                Encoding.UTF8, Config.HttpMediaTypeJson));
            Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);
            var token = await response.Content.ReadAsStringAsync();

            // Update administrator password.
            var passwordBefore = (await Config.DbContext.Users.
                SingleOrDefaultAsync(u => u.Role == Common.User.UserRole.Admin)).Password;
            Config.Client.SetToken(token);
            response = await Config.Client.PatchAsync(Common.ApiRoutes.Admin.Update,
                new StringContent(
                    @"{ 
                        ""Password"": ""updated_admin_password""
                    }",
                Encoding.UTF8, Config.HttpMediaTypeJson));
            Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);
            var passwordAfter = (await Config.DbContext.Users.AsNoTracking()
                .SingleOrDefaultAsync(u => u.Role == Common.User.UserRole.Admin)).Password;
            Assert.AreNotEqual(passwordBefore, passwordAfter);
        }

        [TestMethod]
        public async Task Get_Admin_Info()
        {
            // Register administrator.
            HttpResponseMessage response = await Config.Client.PostAsync(Common.ApiRoutes.Admin.Register,
                new StringContent(
                    @"{ 
                        ""Email"": ""admin@admin.com"",
                        ""Password"": ""admin""
                    }",
                Encoding.UTF8, Config.HttpMediaTypeJson));
            Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);
            var token = await response.Content.ReadAsStringAsync();

            // Read Administration info.
            Config.Client.SetToken(token);
            response = await Config.Client.GetAsync(Common.ApiRoutes.Admin.Info);
            Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);
            var responseInfoStr = await response.Content.ReadAsStringAsync();
            Common.Api.Response.User responseUser = JsonSerializer.Deserialize<Common.Api.Response.User>(responseInfoStr, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });
            Assert.AreEqual("Admin", responseUser.Nickname);
            Assert.AreEqual(Common.User.UserRole.Admin, responseUser.Role);
            Assert.AreEqual(1, responseUser.UserId);
        }

        #region Methods
        private static async Task<string> RegisterAdminAsync()
        {
            HttpResponseMessage response = await Config.Client.PostAsync(Common.ApiRoutes.Admin.Register,
                new StringContent(JsonSerializer.Serialize(new Common.Api.Request.User { Email= "admin@admin.com", Password= "admin" }),
                    Encoding.UTF8, Config.HttpMediaTypeJson));
            Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);
            return await response.Content.ReadAsStringAsync();
        }
        #endregion
    }
}