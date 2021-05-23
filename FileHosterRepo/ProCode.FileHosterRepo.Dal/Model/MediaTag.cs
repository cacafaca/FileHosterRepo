using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ProCode.FileHosterRepo.Dal.Model
{
    [Table("media_tag")]
    [Index(nameof(Name), IsUnique = true)]
    public class MediaTag
    {
        // PK
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int MediaTagId { get; set; }

        // Bare data
        [StringLength(30)]
        public string Name { get; set; }

        public override string ToString()
        {
            return $"{Name}({MediaTagId})";
        }
    }
}
