using Microsoft.EntityFrameworkCore;
using SmartTask.Web.Data.Context;
using SmartTask.Web.Infrastructure.Interfaces;
using SmartTask.Web.Models.Entities;
using SmartTask.Web.Models.Enums;
using SmartTask.Web.Services.Interfaces;

namespace SmartTask.Web.Services.Implementations;

public class OffroadTaskService : BaseService<OffroadTask>, IOffroadTaskService
{
    private readonly ApplicationDbContext _context;
    private readonly IProjectService _projectService;

    public OffroadTaskService(
        IGenericRepository<OffroadTask> repository,
        IUnitOfWork unitOfWork,
        ApplicationDbContext context,
        IProjectService projectService)
        : base(repository, unitOfWork)
    {
        _context = context;
        _projectService = projectService;
    }

    public async Task<List<OffroadTask>> GetByProjectAsync(int projectId)
    {
        return await _context.OffroadTasks
            .Where(x => x.ProjectId == projectId && x.ViewState)
            .Include(x => x.CreatedByUser)
            .Include(x => x.AssignedToUser)
            .OrderByDescending(x => x.CreatedDate)
            .ToListAsync();
    }

    // OPTIMIZED: Project only needed fields instead of loading full entity
    public async Task<bool> CanManageOffroadTaskAsync(int offroadTaskId, int userId)
    {
        var taskInfo = await _context.OffroadTasks
            .Where(x => x.Id == offroadTaskId && x.ViewState)
            .Select(x => new { x.CreatedByUserId, x.ProjectId })
            .FirstOrDefaultAsync();

        if (taskInfo == null) return false;
        if (taskInfo.CreatedByUserId == userId) return true;
        return await _projectService.CanManageProjectAsync(taskInfo.ProjectId, userId);
    }

    // OPTIMIZED: Use ExecuteUpdateAsync for all update operations
    public async Task ChangeStatusAsync(int id, OffroadStatusType status)
    {
        await _context.OffroadTasks
            .Where(x => x.Id == id && x.ViewState)
            .ExecuteUpdateAsync(u => u
                .SetProperty(x => x.Status, status)
                .SetProperty(x => x.ChangeDate, DateTime.Now));
    }

    public async Task ChangePriorityAsync(int id, OffroadPriorityType priority)
    {
        await _context.OffroadTasks
            .Where(x => x.Id == id && x.ViewState)
            .ExecuteUpdateAsync(u => u
                .SetProperty(x => x.Priority, priority)
                .SetProperty(x => x.ChangeDate, DateTime.Now));
    }

    public async Task AssignAsync(int id, int? userId)
    {
        await _context.OffroadTasks
            .Where(x => x.Id == id && x.ViewState)
            .ExecuteUpdateAsync(u => u
                .SetProperty(x => x.AssignedToUserId, userId)
                .SetProperty(x => x.ChangeDate, DateTime.Now));
    }

    // OPTIMIZED: Use ExecuteUpdateAsync for soft delete
    public new async Task DeleteAsync(int id)
    {
        await _context.OffroadTasks
            .Where(x => x.Id == id)
            .ExecuteUpdateAsync(u => u
                .SetProperty(x => x.ViewState, false)
                .SetProperty(x => x.ChangeDate, DateTime.Now));
    }
}