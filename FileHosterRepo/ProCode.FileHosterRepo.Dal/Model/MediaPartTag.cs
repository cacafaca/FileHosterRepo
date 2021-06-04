using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ProCode.FileHosterRepo.Dal.Model
{
    [Table("media_part_tag")]
    public class MediaPartTag
    {
        // FK version
        public int MediaPartId { get; set; }
        [ForeignKey("MediaPartId")]
        public virtual MediaPart MediaPart { get; set; }
        
        // FK tag
        public int MediaTagId { get; set; }
        [ForeignKey("MediaTagId")]
        public virtual MediaTag MediaTag { get; set; }
    }
}
