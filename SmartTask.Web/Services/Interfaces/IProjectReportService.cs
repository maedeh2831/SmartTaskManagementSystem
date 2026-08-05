using SmartTask.Web.Models.ViewModels.Report;

namespace SmartTask.Web.Services.Interfaces
{
    public interface IProjectReportService
    {
        Task<ProjectReportViewModel?> GetReportAsync(int projectId, DateTime? fromDate, DateTime? toDate);
    }
}