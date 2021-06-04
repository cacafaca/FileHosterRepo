using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ProCode.FileHosterRepo.Dal.Model
{
    [Table("media_header_tag")]
    public class MediaHeaderTag
    {
        // FK version
        public int MediaHeaderId { get; set; }
        [ForeignKey("MediaHeaderId")]
        public virtual MediaHeader MediaHeader { get; set; }
        
        // FK tag
        public int MediaTagId { get; set; }
        [ForeignKey("MediaTagId")]
        public virtual MediaTag MediaTag { get; set; }
    }
}
