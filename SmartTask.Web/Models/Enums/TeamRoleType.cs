/*
| Module      : Definitions
| Entity      : TeamRoleType
| Purpose     : تعیین نقش کاربران در تیم.
*/

using System.ComponentModel.DataAnnotations;

namespace SmartTask.Web.Models.Enums
{
    public enum TeamRoleType
    {
        [Display(Name = "رهبر تیم")]
        Leader = 1,

        [Display(Name = "عضو تیم")]
        Member = 2,

        [Display(Name = "مهمان")]
        Guest = 3
    }
}