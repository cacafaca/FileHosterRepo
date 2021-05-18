using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ProCode.FileHosterRepo.Dal.Model
{
    public class User
    {
        // Primary key
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int UserId { get; set; }

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
        public bool Logged { get; set; }
    }
}
