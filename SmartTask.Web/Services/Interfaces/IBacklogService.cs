using SmartTask.Web.Models.Entities;

namespace SmartTask.Web.Services.Interfaces;

public interface IBacklogService : IBaseService<Backlog>
{
    Task<Backlog> GetOrCreateAsync(int projectId);
}