/*
| Module      : Common
| Attribute   : MaxFileSizeAttribute
| Purpose     : اعتبارسنجی حداکثر حجم فایل آپلودی.
*/
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Http;

namespace SmartTask.Web.Common.Attributes
{
    public class MaxFileSizeAttribute : ValidationAttribute
    {
        private readonly int _maxSizeInBytes;

        public MaxFileSizeAttribute(int maxSizeInBytes)
        {
            _maxSizeInBytes = maxSizeInBytes;
        }

        protected override ValidationResult? IsValid(object? value, ValidationContext validationContext)
        {
            if (value is IFormFile file && file.Length > _maxSizeInBytes)
            {
                return new ValidationResult(
                    ErrorMessage ?? $"حجم فایل نباید بیشتر از {_maxSizeInBytes / 1024 / 1024} مگابایت باشد.");
            }

            return ValidationResult.Success;
        }
    }
}