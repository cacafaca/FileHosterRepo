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
        public DbSet<Model.MediaPart> MediaParts { get; set; }
        public DbSet<Model.MediaVersion> MediaVersions { get; set; }
        public DbSet<Model.MediaLink> MediaLinks { get; set; }
        #endregion

        #region Methods
        protected override void OnModelCreating(ModelBuilder builder)
        {
            // Email and Nickname must be unique. No point allowing duplicate nicknames. Will create confusion.
            builder.Entity<Model.User>()
                .HasIndex(u => u.Email)
                .IsUnique();
            builder.Entity<Model.User>()
                .HasIndex(u => u.Nickname)
                .IsUnique();

            builder.Entity<Model.MediaTag>()
                .Property(c => c.Name)
                .UseCollation("SQL_Latin1_General_CP1_CI_AS");      // Case insensitive
        }
        #endregion
    }
}