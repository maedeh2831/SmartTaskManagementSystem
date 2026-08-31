/*
| Module      : Gamification
| Enum        : TransactionType
| Purpose     : انواع تراکنش‌های کیف پول کاربر
*/

namespace SmartTask.Web.Models.Enums
{
    public enum TransactionType
    {
        Earned = 1,
        Spent = 2,
        Refund = 3,
        Bonus = 4,
        Penalty = 5
    }
}
