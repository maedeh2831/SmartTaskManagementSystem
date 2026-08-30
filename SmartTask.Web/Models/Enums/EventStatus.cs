/*
| Module      : Gamification
| Enum        : EventStatus
| Purpose     : وضعیت‌های ممکن برای رویدادهای فصلی
*/

namespace SmartTask.Web.Models.Enums
{
    public enum EventStatus
    {
        Scheduled = 0,
        Active = 1,
        Paused = 2,
        Ended = 3,
        Cancelled = 4
    }
}
