using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ProCode.FileHosterRepo.Dal.Model
{
    public class MediaLink
    {
        public int Id { get; set; }
        [Required]
        public MediaVersion MediaVersion { get; set; }
        [Required]
        public int LinkOrder { get; set; }
        [Required]
        [StringLength(2000)]
        public string Link { get; set; }
        public DateTime Created { get; set; }
    }
}
