using Microsoft.EntityFrameworkCore;
using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ProCode.FileHosterRepo.Dal.Model
{
    [Index(nameof(MediaPartId), nameof(VersionId), nameof(LinkId), IsUnique = true)]
    public class MediaLink
    {
        // Primary key
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int MediaLinkId { get; set; }

        // Unique key
        public int MediaPartId { get; set; }
        [ForeignKey("MediaPartId")]
        public MediaPart MediaPart { get; set; }
        [Required]
        public int VersionId { get; set; }
        [Required]
        public int LinkId { get; set; }

        // Non PK fields
        [Required]
        public Uri Link { get; set; }
        public DateTime Created { get; set; }
        public int UserId { get; set; }
        [ForeignKey("UserId")]
        public User User { get; set; }
        [Column(TypeName = "text")] // <= 2^16=65535 characters
        public string VersionComment { get; set; }
    }
}
