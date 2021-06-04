using Microsoft.EntityFrameworkCore;
using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ProCode.FileHosterRepo.Dal.Model
{
    [Table("media_part")]
    [Index(nameof(MediaHeaderId), nameof(Season), nameof(Episode), IsUnique = true)]
    public class MediaPart
    {
        // Primary key
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int MediaPartId { get; set; }
        
        // Unique key
        public int MediaHeaderId { get; set; }
        [ForeignKey("MediaHeaderId")]
        public virtual MediaHeader MediaHeader { get; set; }
        [Required]
        public int Season { get; set; }
        [Required]
        public int Episode { get; set; }

        public int UserId { get; set; }
        [ForeignKey("UserId")]
        public virtual User User { get; set; }        
        
        [StringLength(200)]
        public string Name { get; set; }
        [Column(TypeName = "text")] // <= 2^16=65535 characters
        public string Description { get; set; }
        [Required]
        public DateTime Created { get; set; }
        public Uri ReferenceLink { get; set; }
    }
}