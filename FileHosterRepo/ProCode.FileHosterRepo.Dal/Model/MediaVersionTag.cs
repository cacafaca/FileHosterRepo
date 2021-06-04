using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ProCode.FileHosterRepo.Dal.Model
{
    [Table("media_version_tag")]
    public class MediaVersionTag
    {
        // FK version
        public int MediaVersionId { get; set; }
        [ForeignKey("MediaVersionId")]
        public virtual MediaVersion MediaVersion { get; set; }
        // FK tag
        public int MediaTagId { get; set; }
        [ForeignKey("MediaTagId")]
        public virtual MediaTag MediaTag { get; set; }
    }
}
