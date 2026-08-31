/*
| Module      : Gamification
| DTO         : EquippedCosmeticsDto
| Purpose     : اقلام فعال کاربر برای اعمال در ظاهر برنامه
*/

namespace SmartTask.Web.Models.ViewModels.Gamification
{
    public class EquippedCosmeticsDto
    {
        /// <summary>رنگ حاشیه آواتار فعال</summary>
        public string? AvatarBorderColor { get; set; }

        /// <summary>آیکن حاشیه آواتار فعال</summary>
        public string? AvatarBorderIcon { get; set; }

        /// <summary>نام حاشیه آواتار فعال</summary>
        public string? AvatarBorderName { get; set; }

        /// <summary>کمیابی حاشیه فعال (۱ تا ۵) برای جلوه‌های ویژه</summary>
        public int AvatarBorderRarity { get; set; }

        /// <summary>نشان فعال</summary>
        public string? BadgeIcon { get; set; }
        public string? BadgeName { get; set; }
        public string? BadgeColor { get; set; }

        /// <summary>پوسته فعال</summary>
        public string? ThemeName { get; set; }
        public string? ThemeColor { get; set; }

        /// <summary>کلید پوسته برای اعمال در CSS (مثل ocean-blue)</summary>
        public string? ThemeSlug { get; set; }

        /// <summary>نام مزایای فعال</summary>
        public List<string> ActivePerks { get; set; } = new();

        public bool HasAny =>
            AvatarBorderColor != null
            || BadgeIcon != null
            || ThemeSlug != null
            || ActivePerks.Count > 0;
    }
}
