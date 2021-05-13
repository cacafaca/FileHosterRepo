using Microsoft.EntityFrameworkCore;

namespace ProCode.FileHosterRepo.Dal.DataAccess
{
    public class FileHosterRepoContext : DbContext
    {
        #region Constructors
        public FileHosterRepoContext(DbContextOptions options) : base(options) { }
        #endregion

        #region Properties
        public DbSet<Model.User> Users { get; set; }
        public DbSet<Model.Media> Medias { get; set; }
        public DbSet<Model.MediaVersion> MediaVersions { get; set; }
        public DbSet<Model.MediaLink> MediaLinks { get; set; }
        #endregion

        #region Methods
        protected override void OnModelCreating(ModelBuilder builder)
        {
            builder.Entity<Model.User>()
                .HasIndex(u => u.Email)
                .IsUnique();
            builder.Entity<Model.User>()
                .HasIndex(u => u.Nickname)
                .IsUnique();
        }
        #endregion
    }
}