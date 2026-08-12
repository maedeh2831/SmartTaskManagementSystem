namespace SmartTask.Web.Services.Interfaces
{
    public interface IDateFormatService
    {
        bool IsJalali { get; }
        string ToDisplayString(DateTime date, bool includeTime = false);
        string ToDisplayString(DateTime? date, bool includeTime = false);
    }
}