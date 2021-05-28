using System.ComponentModel;

namespace ProCode.FileHosterRepo.Dto.Common
{
    public enum UserRole
    {
        /// <summary>
        /// God.
        /// </summary>
        [Description("Administrator")]
        Admin = 0,
        Moderator = 10,
        TrustedUser = 20,
        /// <summary>
        /// User that is just registered. Lowest life form. :)
        /// </summary>
        [Description("User")]
        User = 30
    }
}
