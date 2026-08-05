using SmartTask.Web.Models.ViewModels.Report;

namespace SmartTask.Web.Services.Interfaces
{
    public interface IReportExportService
    {
        byte[] GenerateWorkspacePdf(WorkspaceReportViewModel model);
        byte[] GenerateWorkspaceExcel(WorkspaceReportViewModel model);
        byte[] GenerateProjectPdf(ProjectReportViewModel model);
        byte[] GenerateProjectExcel(ProjectReportViewModel model);
    }
}