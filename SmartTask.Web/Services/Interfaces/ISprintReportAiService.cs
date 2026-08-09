using SmartTask.Web.Models.ViewModels.SprintReport;

namespace SmartTask.Web.Services.Interfaces;

public interface ISprintReportAiService
{
    Task<List<SprintReportViewModel>> GetReportsAsync(int sprintId);
    Task<SprintReportViewModel> GenerateReportAsync(int sprintId, int currentUserId);
}