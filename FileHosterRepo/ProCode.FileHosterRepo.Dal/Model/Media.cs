using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ProCode.FileHosterRepo.Dal.Model
{
    public class Media
    {
        // Primary key
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int MediaId { get; set; }

        // Non PK fields
        [Required]
        [StringLength(200)]
        public string Name { get; set; }
        [Column(TypeName = "text")] // <= 2^16=65535 characters
        public string Description { get; set; }
        public Uri ReferenceLink { get; set; }
    }
}
