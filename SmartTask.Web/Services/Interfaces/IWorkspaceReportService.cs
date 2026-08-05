using SmartTask.Web.Models.ViewModels.Report;

namespace SmartTask.Web.Services.Interfaces
{
    public interface IWorkspaceReportService
    {
        Task<WorkspaceReportViewModel> GetReportAsync(int workspaceId, DateTime? fromDate, DateTime? toDate);
    }
}