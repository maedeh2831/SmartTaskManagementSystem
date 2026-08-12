using System.Globalization;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.AspNetCore.Razor.TagHelpers;
using SmartTask.Web.Services.Interfaces;

namespace SmartTask.Web.Common.TagHelpers
{
    [HtmlTargetElement("input", Attributes = DateForAttributeName)]
    public class DatePickerTagHelper : TagHelper
    {
        private const string DateForAttributeName = "asp-date-for";
        private const string IncludeTimeAttributeName = "asp-include-time";

        private readonly IDateFormatService _dateFormatService;

        public DatePickerTagHelper(IDateFormatService dateFormatService)
        {
            _dateFormatService = dateFormatService;
        }

        [HtmlAttributeName(DateForAttributeName)]
        public ModelExpression DateFor { get; set; } = default!;

        [HtmlAttributeName(IncludeTimeAttributeName)]
        public bool IncludeTime { get; set; } = false;

        public override void Process(TagHelperContext context, TagHelperOutput output)
        {
            var propertyName = DateFor.Name;
            var rawValue = DateFor.Model as DateTime?;

            // اگه توی View یه id دستی داده شده (مثل id="sprintStart")، حفظش می‌کنیم
            var explicitId = output.Attributes["id"]?.Value?.ToString();
            var baseId = !string.IsNullOrEmpty(explicitId) ? explicitId : propertyName.Replace(".", "_");

            output.Attributes.RemoveAll(DateForAttributeName);
            output.Attributes.RemoveAll(IncludeTimeAttributeName);

            if (!_dateFormatService.IsJalali)
            {
                // میلادی: همون input بومی مرورگر، رفتار فعلی دست‌نخورده
                output.Attributes.SetAttribute("type", IncludeTime ? "datetime-local" : "date");
                output.Attributes.SetAttribute("name", propertyName);
                output.Attributes.SetAttribute("id", baseId);

                if (rawValue.HasValue)
                {
                    var format = IncludeTime ? "yyyy-MM-ddTHH:mm" : "yyyy-MM-dd";
                    output.Attributes.SetAttribute("value", rawValue.Value.ToString(format, CultureInfo.InvariantCulture));
                }
                return;
            }

            var cssClass = output.Attributes["class"]?.Value?.ToString() ?? "form-control";

            var displayValue = rawValue.HasValue
                ? _dateFormatService.ToDisplayString(rawValue.Value, IncludeTime)
                : "";

            var hiddenValue = rawValue.HasValue
                ? rawValue.Value.ToString("yyyy-MM-ddTHH:mm:ss", CultureInfo.InvariantCulture)
                : "";

            var html = $@"<input type=""text""
                        id=""{baseId}_display""
                        class=""{cssClass} date-picker-display""
                        data-hidden-target=""#{baseId}""
                        data-include-time=""{(IncludeTime ? "true" : "false")}""
                        autocomplete=""off""
                        value=""{displayValue}"" />
                    <input type=""hidden"" id=""{baseId}"" name=""{propertyName}"" value=""{hiddenValue}"" />";

            output.TagName = null;
            output.Attributes.Clear();
            output.Content.SetHtmlContent(html);
        }
    }
}