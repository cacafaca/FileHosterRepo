using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ProCode.FileHosterRepo.Dal.Model
{
    public class User
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }
        [Required]
        [StringLength(200)]
        public string Email { get; set; }
        [Required]
        [StringLength(100)]        
        public string Password { get; set; }
        [StringLength(50)]
        public string Nickname { get; set; }
        public DateTime? Created { get; set; }
        public UserRole Role { get; set; }
    }

    public enum UserRole
    {
        SuperAdmin = 0,
        Moderator = 10,
        TrustedUser = 20,
        PlainUser = 30
    }
}
