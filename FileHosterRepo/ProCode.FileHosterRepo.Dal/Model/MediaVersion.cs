using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ProCode.FileHosterRepo.Dal.Model
{
    public class MediaVersion
    {
        public int Id { get; set; }
        [Required]
        public Media Media { get; set; }
        [StringLength(200)]
        public string Name { get; set; }
        [Required]
        public User User { get; set; }
        public DateTime Created { get; set; }
    }
}
