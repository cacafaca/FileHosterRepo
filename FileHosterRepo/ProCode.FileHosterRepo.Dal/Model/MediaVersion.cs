using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ProCode.FileHosterRepo.Dal.Model
{
    [Table("media_version")]
    public class MediaVersion
    {
        // Primary key
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int MediaVersionId { get; set; }

        // FKs
        public int MediaPartId { get; set; }
        [ForeignKey("MediaPartId")]
        public virtual MediaPart MediaPart { get; set; }

        // Useful data
        public string VersionComment { get; set; }
        
        public int UserId { get; set; }
        [ForeignKey("UserId")]
        public virtual User User { get; set; }
        
        public DateTime Created { get; set; }
    }
}
