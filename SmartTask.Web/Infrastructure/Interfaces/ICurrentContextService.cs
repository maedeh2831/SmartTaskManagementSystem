namespace SmartTask.Web.Infrastructure.Interfaces
{
    public interface ICurrentContextService
    {
        int? CurrentWorkspaceId { get; }
        int? CurrentProjectId { get; }

        void SetCurrentWorkspace(int workspaceId);
        void SetCurrentProject(int projectId);
        void ClearContext();
    }
}