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

        // Basic media tables.
        public DbSet<Model.MediaHeader> Medias { get; set; }
        public DbSet<Model.MediaPart> MediaParts { get; set; }
        public DbSet<Model.MediaVersion> MediaVersions { get; set; }
        public DbSet<Model.MediaLink> MediaLinks { get; set; }
        public DbSet<Model.MediaTag> MediaTags { get; set; }

        // Links to tags
        public DbSet<Model.MediaHeaderTag> MediaHeaderTags { get; set; }
        public DbSet<Model.MediaPartTag> MediaPartTags { get; set; }
        public DbSet<Model.MediaVersionTag> MediaVersionTags { get; set; }
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
                .UseCollation("SQL_Latin1_General_CP1_CI_AS");                      // Case insensitive

            builder.Entity<Model.MediaHeaderTag>()
                .HasKey(vt => new { vt.MediaHeaderId, vt.MediaTagId });            // Composite PK
            
            builder.Entity<Model.MediaPartTag>()
                .HasKey(vt => new { vt.MediaPartId, vt.MediaTagId });            // Composite PK
            
            builder.Entity<Model.MediaVersionTag>()
                .HasKey(vt => new { vt.MediaVersionId, vt.MediaTagId });            // Composite PK

            builder.Entity<Model.MediaVersionTag>()
                .HasComment("Connection between MediaVersions and MediaTags tables.");
        }
        #endregion
    }
}