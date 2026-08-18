using System.Globalization;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.AspNetCore.Razor.TagHelpers;
using SmartTask.Web.Services.Interfaces;

namespace SmartTask.Web.Common.Helpers
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

            var explicitId = output.Attributes["id"]?.Value?.ToString();
            var baseId = !string.IsNullOrEmpty(explicitId) ? explicitId : propertyName.Replace(".", "_");

            output.Attributes.RemoveAll(DateForAttributeName);
            output.Attributes.RemoveAll(IncludeTimeAttributeName);

            if (!_dateFormatService.IsJalali)
            {
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

    [HtmlTargetElement("input", Attributes = DatePickerNameTagHelper.NameAttributeNamePublic)]
    public class DatePickerNameTagHelper : TagHelper
    {
        public const string NameAttributeNamePublic = "asp-date-name";
        private const string NameAttributeName = "asp-date-name";
        private const string IncludeTimeAttributeName = "asp-include-time";
        private const string ValueAttributeName = "asp-date-value";
        private readonly IDateFormatService _dateFormatService;
        public DatePickerNameTagHelper(IDateFormatService dateFormatService)
        {
            _dateFormatService = dateFormatService;
        }
        [HtmlAttributeName(NameAttributeName)]
        public string DateName { get; set; } = default!;
        [HtmlAttributeName(IncludeTimeAttributeName)]
        public bool IncludeTime { get; set; } = false;
        [HtmlAttributeName(ValueAttributeName)]
        public DateTime? Value { get; set; }
        public override void Process(TagHelperContext context, TagHelperOutput output)
        {
            var explicitId = output.Attributes["id"]?.Value?.ToString();
            var baseId = !string.IsNullOrEmpty(explicitId) ? explicitId : DateName;
            output.Attributes.RemoveAll(NameAttributeName);
            output.Attributes.RemoveAll(IncludeTimeAttributeName);
            output.Attributes.RemoveAll(ValueAttributeName);
            if (!_dateFormatService.IsJalali)
            {
                output.Attributes.SetAttribute("type", IncludeTime ? "datetime-local" : "date");
                output.Attributes.SetAttribute("name", DateName);
                output.Attributes.SetAttribute("id", baseId);
                if (Value.HasValue)
                {
                    var format = IncludeTime ? "yyyy-MM-ddTHH:mm" : "yyyy-MM-dd";
                    output.Attributes.SetAttribute("value", Value.Value.ToString(format, CultureInfo.InvariantCulture));
                }
                return;
            }
            var cssClass = output.Attributes["class"]?.Value?.ToString() ?? "form-control";
            var displayValue = Value.HasValue ? _dateFormatService.ToDisplayString(Value.Value, IncludeTime) : "";
            var hiddenValue = Value.HasValue ? Value.Value.ToString("yyyy-MM-ddTHH:mm:ss", CultureInfo.InvariantCulture) : "";
            var html = $@"<input type=""text""
    id=""{baseId}_display""
    class=""{cssClass} date-picker-display""
    data-hidden-target=""#{baseId}""
    data-include-time=""{(IncludeTime ? "true" : "false")}""
    autocomplete=""off""
    value=""{displayValue}"" />
<input type=""hidden"" id=""{baseId}"" name=""{DateName}"" value=""{hiddenValue}"" />";
            output.TagName = null;
            output.Attributes.Clear();
            output.Content.SetHtmlContent(html);
        }
    }
}