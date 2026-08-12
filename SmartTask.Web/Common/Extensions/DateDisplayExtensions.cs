using Microsoft.AspNetCore.Html;
using Microsoft.AspNetCore.Mvc.Rendering;
using SmartTask.Web.Services.Interfaces;

namespace SmartTask.Web.Common.Extensions
{
    public static class DateDisplayExtensions
    {
        public static IHtmlContent DisplayDate(this IHtmlHelper html, DateTime? date, bool includeTime = false)
        {
            var dateFormatService = html.ViewContext.HttpContext.RequestServices
                .GetRequiredService<IDateFormatService>();

            return new HtmlString(dateFormatService.ToDisplayString(date, includeTime));
        }

        public static IHtmlContent DisplayDate(this IHtmlHelper html, DateTime date, bool includeTime = false)
            => html.DisplayDate((DateTime?)date, includeTime);
    }
}