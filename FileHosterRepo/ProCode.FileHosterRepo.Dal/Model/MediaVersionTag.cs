using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ProCode.FileHosterRepo.Dal.Model
{
    public class MediaVersionTag
    {
        // FK version
        public int MediaVersionId { get; set; }
        [ForeignKey("MediaPartId")]
        public MediaVersion MediaVersion { get; set; }
        // FK tag
        public int MediaTagId { get; set; }
        [ForeignKey("MediaTagId")]
        public MediaTag MediaTag { get; set; }
    }
}
