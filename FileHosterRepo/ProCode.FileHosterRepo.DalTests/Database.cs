using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Threading.Tasks;

namespace ProCode.FileHosterRepo.Dal.DataAccess.Tests
{
    [TestClass()]
    public class Database
    {
        [TestMethod()]
        public async Task EmptyDatabase()
        {
            FileHosterRepoContextFactory contextFactory = new();
            var context = contextFactory.CreateDbContext(null);
            Assert.IsNotNull(context);
            await context.Database.EnsureDeletedAsync();
        }
    }
}