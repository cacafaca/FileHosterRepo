﻿using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace ProCode.FileHosterRepo.Dal.DataAccess.Tests
{
    [TestClass()]
    public class FileHosterContextFactoryTests
    {
        [TestMethod()]
        public void CreateDbContextTest()
        {
            FileHosterRepoContextFactory contextFactory = new();
            var context = contextFactory.CreateDbContext(null); ; ;
            Assert.IsNotNull(context);
        }
    }
}
