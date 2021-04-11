using Microsoft.EntityFrameworkCore;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using ProCode.FileHosterRepo.Dal.DataAccess;
using System;
using System.Collections.Generic;
using System.Text;

namespace ProCode.FileHosterRepo.Dal.DataAccess.Tests
{
    [TestClass()]
    public class FileHosterContextTests
    {
        [TestMethod()]
        public void FileHosterContextTest()
        {
            FileHosterContextFactory contextFactory = new();
            var context = contextFactory.CreateDbContext(null); ; ;
            Assert.IsNotNull(context);
            Assert.IsTrue(context.Database.CanConnect());
        }
    }
}