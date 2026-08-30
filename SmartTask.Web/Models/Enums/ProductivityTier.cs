/*
| Module      : Gamification
| Enum        : ProductivityTier
| Purpose     : تعریف سطح‌های بهره‌وری (درجه‌بندی کاربران)
*/

namespace SmartTask.Web.Models.Enums
{
    public enum ProductivityTier
    {
        /// <summary>
        /// نیاز به بهبود - 0 تا 40
        /// </summary>
        Bronze = 0,

        /// <summary>
        /// سازگار - 41 تا 60
        /// </summary>
        Silver = 1,

        /// <summary>
        /// بهره‌وری بالا - 61 تا 80
        /// </summary>
        Gold = 2,

        /// <summary>
        /// بسیار بهره‌ور - 81 تا 94
        /// </summary>
        Platinum = 3,

        /// <summary>
        /// نخبه - 95 تا 100
        /// </summary>
        Diamond = 4
    }
}
