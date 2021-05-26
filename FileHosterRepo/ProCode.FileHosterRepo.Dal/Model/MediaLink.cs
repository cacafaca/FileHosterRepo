using Microsoft.EntityFrameworkCore;
using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ProCode.FileHosterRepo.Dal.Model
{
    [Table("media_link")]
    [Index(nameof(MediaVersionId), nameof(LinkOrderId), IsUnique = true)]
    public class MediaLink
    {
        // Primary key
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int MediaLinkId { get; set; }

        // Unique keyS
        public int MediaVersionId { get; set; }
        [ForeignKey("MediaVersionId")]
        public MediaVersion Version { get; set; }
        /// <summary>
        /// Order number of the link in the link list.
        /// </summary>
        [Required]
        public int LinkOrderId { get; set; }

        // Non PK fields
        [Required]
        public Uri Link { get; set; }
        public int UserId { get; set; }
        [ForeignKey("UserId")]
        public User User { get; set; }
    }
}
