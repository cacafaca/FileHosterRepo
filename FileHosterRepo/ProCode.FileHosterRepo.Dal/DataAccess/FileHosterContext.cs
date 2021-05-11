using Microsoft.EntityFrameworkCore;

namespace ProCode.FileHosterRepo.Dal.DataAccess
{
    public class FileHosterContext : DbContext
    {
        public FileHosterContext(DbContextOptions options) : base(options) { }
        public DbSet<Model.User> Users { get; set; }
        public DbSet<Model.Media> Medias { get; set; }
        public DbSet<Model.MediaVersion> MediaVersions { get; set; }
        public DbSet<Model.MediaLink> MediaLinks { get; set; }
        public DbSet<Model.UserToken> UserTokens { get; set; }
    }
}
