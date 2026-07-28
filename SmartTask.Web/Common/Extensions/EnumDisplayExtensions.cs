using System.ComponentModel.DataAnnotations;
using System.Reflection;

namespace SmartTask.Web.Common.Extensions
{
    public static class EnumDisplayExtensions
    {
        public static string GetDisplayName(this Enum value)
        {
            var field = value.GetType().GetField(value.ToString());
            if (field == null)
                return value.ToString();

            var attribute = field.GetCustomAttribute<DisplayAttribute>();
            return attribute?.Name ?? value.ToString();
        }
    }
}