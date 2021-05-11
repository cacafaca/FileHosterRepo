using FakeItEasy;
using Microsoft.EntityFrameworkCore;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using ProCode.FileHosterRepo.Api.Controllers;
using ProCode.FileHosterRepo.Dal.DataAccess;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ProCode.FileHosterRepo.Api.Controllers.Tests
{
    [TestClass()]
    public class AdminControllerTests
    {
        [TestMethod()]
        public void Register_Admin_On_A_Cleen_Database()
        {
            var context = ApiTests.Config.GetDbContext();
            context.Database.EnsureDeleted();   // Drop database, so I can test from clean start.
            context.Database.EnsureCreated();   // Recreate database.

            var controller = new AdminController(context, ApiTests.Config.GetJwtAuthenticationManager());

            var token = controller.Register(new Model.Request.UserRegister()
            {
                Email = "admin@admin.com",
                Nickname = "Admin",
                Password = "admin"
            }).Result;

            Assert.IsTrue(!string.IsNullOrWhiteSpace(token.Value));
            Assert.AreEqual(1, context.Users.Count());  // Expect one user.
            Assert.AreEqual(1, context.Users.Where(u => u.Role == Dal.Model.UserRole.Admin).Count()); // Expect one administrator.
        }
    }
}