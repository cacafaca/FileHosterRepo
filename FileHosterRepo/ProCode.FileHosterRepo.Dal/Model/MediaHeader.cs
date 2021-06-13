using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ProCode.FileHosterRepo.Dal.Model
{
    [Table("media_header")]
    public class MediaHeader
    {
        // Primary key
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int MediaHeaderId { get; set; }

        // Non PK fields
        [Required]
        [StringLength(200)]
        public string Name { get; set; }
        public int Year { get; set; }
        [Column(TypeName = "text")] // <= 2^16=65535 characters
        public string Description { get; set; }
        public Uri ReferenceLink { get; set; }

        public int UserId { get; set; }
        [ForeignKey("UserId")]
        public virtual User User { get; set; }

    }
}
