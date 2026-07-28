/*
| Module      : Common
| Attribute   : AllowedExtensionsAttribute
| Purpose     : اعتبارسنجی فرمت مجاز فایل آپلودی.
*/
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Http;

namespace SmartTask.Web.Common.Attributes
{
    public class AllowedExtensionsAttribute : ValidationAttribute
    {
        private readonly string[] _extensions;

        public AllowedExtensionsAttribute(string[] extensions)
        {
            _extensions = extensions;
        }

        protected override ValidationResult? IsValid(object? value, ValidationContext validationContext)
        {
            if (value is IFormFile file)
            {
                var extension = Path.GetExtension(file.FileName).ToLowerInvariant();

                if (!_extensions.Contains(extension))
                {
                    return new ValidationResult(ErrorMessage ?? "فرمت فایل مجاز نیست.");
                }
            }

            return ValidationResult.Success;
        }
    }
}