using SmartTask.Web.Infrastructure.Interfaces;
using SmartTask.Web.Models.Entities;
using SmartTask.Web.Services.Interfaces;

namespace SmartTask.Web.Services.Implementations
{
    public class WorkspaceService
        : BaseService<Workspace>, IWorkspaceService
    {
        public WorkspaceService(
            IGenericRepository<Workspace> repository,
            IUnitOfWork unitOfWork)
            : base(repository, unitOfWork)
        {
        }
    }
}