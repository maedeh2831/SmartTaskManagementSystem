using SmartTask.Web.Infrastructure.Interfaces;

namespace SmartTask.Web.Infrastructure.Services
{
    public class CurrentContextService : ICurrentContextService
    {
        private const string WorkspaceKey = "CurrentWorkspaceId";
        private const string ProjectKey = "CurrentProjectId";

        private readonly IHttpContextAccessor _httpContextAccessor;

        public CurrentContextService(IHttpContextAccessor httpContextAccessor)
        {
            _httpContextAccessor = httpContextAccessor;
        }

        private ISession Session => _httpContextAccessor.HttpContext!.Session;

        public int? CurrentWorkspaceId => Session.GetInt32(WorkspaceKey);
        public int? CurrentProjectId => Session.GetInt32(ProjectKey);

        public void SetCurrentWorkspace(int workspaceId)
        {
            Session.SetInt32(WorkspaceKey, workspaceId);
        }

        public void SetCurrentProject(int projectId)
        {
            Session.SetInt32(ProjectKey, projectId);
        }

        public void ClearContext()
        {
            Session.Remove(WorkspaceKey);
            Session.Remove(ProjectKey);
        }
    }
}