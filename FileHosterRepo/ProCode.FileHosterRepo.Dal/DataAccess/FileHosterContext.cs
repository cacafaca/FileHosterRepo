using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ProCode.FileHosterRepo.Dal.DataAccess
{
    public class FileHosterContext : DbContext
    {
        public FileHosterContext(DbContextOptions options) : base(options) { }
        public DbSet<Model.User> Users { get; set; }
        public DbSet<Model.Media> Medias { get; set; }
        public DbSet<Model.MediaVersion> MediaVersions { get; set; }
        public DbSet<Model.MediaLink> MediaLinks { get; set; }
    }
}
