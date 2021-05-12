using FakeItEasy;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using ProCode.FileHosterRepo.ApiTests;
using ProCode.FileHosterRepo.Dal.DataAccess;
using System;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text.Json;
using System.Threading.Tasks;

namespace ProCode.FileHosterRepo.Api.Controllers.Tests
{
    [TestClass()]
    public class AdminControllerTests
    {
        [TestMethod()]
        public async Task Register_Admin_On_A_Cleen_Database()
        {
            ApiTests.Config.DbContext.Database.EnsureDeleted();   // Drop database, so I can test from clean start.
            ApiTests.Config.DbContext.Database.EnsureCreated();   // Recreate database.

            // Register administrator.
            string contentStr = string.Join("&", new string[] {
                "Email=" + Uri.EscapeDataString("admin@admin.com"),
                "Password=" + Uri.EscapeDataString("admin")
            });
            HttpContent httpContent = new StringContent(contentStr);
            httpContent.Headers.ContentType = new MediaTypeHeaderValue("application/x-www-form-urlencoded");
            HttpResponseMessage response = await ApiTests.Config.Client.PostAsync("/Admin/Register", httpContent);
            var responseToken = await response.Content.ReadAsStringAsync();

            Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);
            Assert.IsTrue(!string.IsNullOrWhiteSpace(responseToken));
            Assert.AreEqual(1, ApiTests.Config.DbContext.Users.Count());  // Expect one user.
            Assert.AreEqual(1, ApiTests.Config.DbContext.Users.Where(u => u.Role == Dal.Model.UserRole.Admin).Count()); // Expect one administrator.
        }

        [TestMethod()]
        public async Task Register_Two_Admins_And_Fail()
        {
            ApiTests.Config.DbContext.Database.EnsureDeleted();   // Drop database, so I can test from clean start.
            ApiTests.Config.DbContext.Database.EnsureCreated();   // Recreate database.

            // Register first administrator.
            string contentStr = string.Join("&", new string[] {
                "Email=" + Uri.EscapeDataString("admin1@admin.com"),
                "Password=" + Uri.EscapeDataString("admin1")
            });
            HttpContent httpContent = new StringContent(contentStr);
            httpContent.Headers.ContentType = new MediaTypeHeaderValue("application/x-www-form-urlencoded");
            HttpResponseMessage response = await ApiTests.Config.Client.PostAsync("/Admin/Register", httpContent);
            Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);

            // Register second administrator.
            contentStr = string.Join("&", new string[] {
                "Email=" + Uri.EscapeDataString("admin2@admin.com"),
                "Password=" + Uri.EscapeDataString("admin2")
            });
            httpContent = new StringContent(contentStr);
            httpContent.Headers.ContentType = new MediaTypeHeaderValue("application/x-www-form-urlencoded");
            response = await ApiTests.Config.Client.PostAsync("/Admin/Register", httpContent);
            Assert.AreEqual(HttpStatusCode.Conflict, response.StatusCode);
            var responseStr = await response.Content.ReadAsStringAsync();
            Assert.IsTrue(responseStr.Contains("admin1@admin.com"));
        }

        [TestMethod]
        public async Task Login_Successfull()
        {
            ApiTests.Config.DbContext.Database.EnsureDeleted();   // Drop database, so I can test from clean start.
            ApiTests.Config.DbContext.Database.EnsureCreated();   // Recreate database.

            // Register administrator.
            string contentStr = string.Join("&", new string[] {
                "Email=" + Uri.EscapeDataString("admin@admin.com"),
                "Password=" + Uri.EscapeDataString("admin")
            });
            HttpContent httpContent = new StringContent(contentStr);
            httpContent.Headers.ContentType = new MediaTypeHeaderValue("application/x-www-form-urlencoded");
            HttpResponseMessage response = await ApiTests.Config.Client.PostAsync("/Admin/Register", httpContent);
            Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);

            // Login administrator.
            contentStr = string.Join("&", new string[] {
                "Email=" + Uri.EscapeDataString("admin@admin.com"),
                "Password=" + Uri.EscapeDataString("admin")
            });
            httpContent = new StringContent(contentStr);
            httpContent.Headers.ContentType = new MediaTypeHeaderValue("application/x-www-form-urlencoded");
            response = await ApiTests.Config.Client.PostAsync("/Admin/Login", httpContent);
            Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);
            var token = await response.Content.ReadAsStringAsync();
            Assert.IsTrue(!string.IsNullOrWhiteSpace(token));
        }

        [TestMethod]
        public async Task Login_Unsuccessfull_Bad_Password()
        {
            ApiTests.Config.DbContext.Database.EnsureDeleted();   // Drop database, so I can test from clean start.
            ApiTests.Config.DbContext.Database.EnsureCreated();   // Recreate database.

            // Register administrator.
            string contentStr = string.Join("&", new string[] {
                "Email=" + Uri.EscapeDataString("admin@admin.com"),
                "Password=" + Uri.EscapeDataString("admin")
            });
            HttpContent httpContent = new StringContent(contentStr);
            httpContent.Headers.ContentType = new MediaTypeHeaderValue("application/x-www-form-urlencoded");
            HttpResponseMessage response = await ApiTests.Config.Client.PostAsync("/Admin/Register", httpContent);
            Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);

            // Login administrator with wrong password.
            contentStr = string.Join("&", new string[] {
                "Email=" + Uri.EscapeDataString("admin@admin.com"),
                "Password=" + Uri.EscapeDataString("badpass")
            });
            httpContent = new StringContent(contentStr);
            httpContent.Headers.ContentType = new MediaTypeHeaderValue("application/x-www-form-urlencoded");
            response = await ApiTests.Config.Client.PostAsync("/Admin/Login", httpContent);
            Assert.AreEqual(HttpStatusCode.Unauthorized, response.StatusCode);
        }

        [TestMethod]
        public async Task Get_Admin_Info()
        {
            ApiTests.Config.DbContext.Database.EnsureDeleted();   // Drop database, so I can test from clean start.
            ApiTests.Config.DbContext.Database.EnsureCreated();   // Recreate database.

            // Register administrator first.
            string contentStr = string.Join("&", new string[] {
                "Email=" + Uri.EscapeDataString("admin@admin.com"),
                "Password=" + Uri.EscapeDataString("admin"),
                "Nickname=" + Uri.EscapeDataString("Admin")
            });
            HttpContent httpContent = new StringContent(contentStr);
            //httpContent.Headers.ContentType.MediaType = "multipart/form-data";
            httpContent.Headers.ContentType = new MediaTypeHeaderValue("application/x-www-form-urlencoded");
            HttpResponseMessage response = await ApiTests.Config.Client.PostAsync("/Admin/Register", httpContent);
            var responseToken = await response.Content.ReadAsStringAsync();
            Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);

            // Read Administration info.
            Config.Client.SetToken(responseToken);
            response = await ApiTests.Config.Client.GetAsync("/Admin/Info");
            Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);
            var responseInfoStr = await response.Content.ReadAsStringAsync();
            Model.Response.User responseUser = JsonSerializer.Deserialize<Model.Response.User>(responseInfoStr, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });
            Assert.AreEqual("Admin", responseUser.Nickname);
            Assert.AreEqual(Dal.Model.UserRole.Admin, responseUser.Role);
            Assert.AreEqual(1, responseUser.Id);
        }
    }
}