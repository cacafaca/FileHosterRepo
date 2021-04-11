using Microsoft.VisualStudio.TestTools.UnitTesting;
using ProCode.FileHosterRepo.Dal.DataAccess;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ProCode.FileHosterRepo.Dal.DataAccess.Tests
{
    [TestClass()]
    public class FileHosterContextFactoryTests
    {
        [TestMethod()]
        public void CreateDbContextTest()
        {
            FileHosterContextFactory contextFactory = new();
            var context = contextFactory.CreateDbContext(null); ; ;
            Assert.IsNotNull(context);
        }
    }
}