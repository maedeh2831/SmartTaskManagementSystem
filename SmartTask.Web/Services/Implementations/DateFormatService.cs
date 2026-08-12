using System.Globalization;
using SmartTask.Web.Infrastructure.Interfaces;
using SmartTask.Web.Models.Enums;
using SmartTask.Web.Services.Interfaces;

namespace SmartTask.Web.Services.Implementations
{
    public class DateFormatService : IDateFormatService
    {
        private readonly ICurrentUserService _currentUserService;
        private static readonly PersianCalendar _persianCalendar = new();

        public DateFormatService(ICurrentUserService currentUserService)
        {
            _currentUserService = currentUserService;
        }

        public bool IsJalali
            => (_currentUserService.CurrentUser?.DateFormat ?? DateFormatType.Jalali) == DateFormatType.Jalali;

        public string ToDisplayString(DateTime? date, bool includeTime = false)
            => date.HasValue ? ToDisplayString(date.Value, includeTime) : "-";

        public string ToDisplayString(DateTime date, bool includeTime = false)
        {
            var result = IsJalali ? FormatJalali(date) : date.ToString("yyyy/MM/dd");

            if (includeTime)
                result += " " + date.ToString("HH:mm");

            return result;
        }

        private static string FormatJalali(DateTime date)
        {
            var year = _persianCalendar.GetYear(date);
            var month = _persianCalendar.GetMonth(date);
            var day = _persianCalendar.GetDayOfMonth(date);
            return $"{year:D4}/{month:D2}/{day:D2}";
        }
    }
}