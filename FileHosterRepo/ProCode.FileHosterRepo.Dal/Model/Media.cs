using System;
using System.ComponentModel.DataAnnotations;

namespace ProCode.FileHosterRepo.Dal.Model
{
    public class Media
    {
        public int Id { get; set; }
        [Required]
        [StringLength(500)]
        public string Name { get; set; }
        [Required]
        public User User { get; set; }
        public DateTime Created { get; set; }
    }
}
