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

    public async Task<bool> CanManageOffroadTaskAsync(int offroadTaskId, int userId)
    {
        var task = await _repository.Query()
            .FirstOrDefaultAsync(x => x.Id == offroadTaskId);

        if (task == null)
            return false;

        if (task.CreatedByUserId == userId)
            return true;

        return await _projectService.CanManageProjectAsync(task.ProjectId, userId);
    }

    public async Task ChangeStatusAsync(int id, OffroadStatusType status)
    {
        var task = await _context.OffroadTasks.FirstOrDefaultAsync(x => x.Id == id);
        if (task == null) return;

        task.Status = status;
        task.ChangeDate = DateTime.Now;
        await _context.SaveChangesAsync();
    }

    public async Task ChangePriorityAsync(int id, OffroadPriorityType priority)
    {
        var task = await _context.OffroadTasks.FirstOrDefaultAsync(x => x.Id == id);
        if (task == null) return;

        task.Priority = priority;
        task.ChangeDate = DateTime.Now;
        await _context.SaveChangesAsync();
    }

    public async Task AssignAsync(int id, int? userId)
    {
        var task = await _context.OffroadTasks.FirstOrDefaultAsync(x => x.Id == id);
        if (task == null) return;

        task.AssignedToUserId = userId;
        task.ChangeDate = DateTime.Now;
        await _context.SaveChangesAsync();
    }

    public new async Task DeleteAsync(int id)
    {
        var task = await _context.OffroadTasks.FirstOrDefaultAsync(x => x.Id == id);
        if (task == null) return;

        task.ViewState = false;
        await _context.SaveChangesAsync();
    }
}