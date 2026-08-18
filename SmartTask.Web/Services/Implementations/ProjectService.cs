using Microsoft.EntityFrameworkCore;
using SmartTask.Web.Data.Context;
using SmartTask.Web.Infrastructure.Interfaces;
using SmartTask.Web.Models.Entities;
using SmartTask.Web.Models.Enums;
using SmartTask.Web.Services.Interfaces;

namespace SmartTask.Web.Services.Implementations;

public class ProjectService : BaseService<Project>, IProjectService
{
    private readonly ApplicationDbContext _context;

    public ProjectService(
        IGenericRepository<Project> repository,
        IUnitOfWork unitOfWork,
        ApplicationDbContext context)
        : base(repository, unitOfWork)
    {
        _context = context;
    }

    public async Task<Project?> GetDetailsAsync(int id)
    {
        return await _context.Projects
            .Include(x => x.Members.Where(m => m.ViewState))
                .ThenInclude(m => m.ApplicationUser)
            .Include(x => x.ProjectTeams.Where(pt => pt.ViewState))
                .ThenInclude(pt => pt.Team)
            .FirstOrDefaultAsync(x => x.Id == id && x.ViewState);
    }

    public async Task<bool> ExistsByKeyAsync(int workspaceId, string key, int? excludeId = null)
    {
        var query = _repository.Query()
            .Where(x => x.WorkspaceId == workspaceId && x.Key == key && x.ViewState);

        if (excludeId.HasValue)
            query = query.Where(x => x.Id != excludeId.Value);

        return await query.AnyAsync();
    }

    public async Task<bool> CanManageProjectsAsync(int workspaceId, int userId)
    {
        return await _context.Workspaces
            .Where(w => w.Id == workspaceId)
            .AnyAsync(w =>
                w.OwnerId == userId ||
                w.Members.Any(m =>
                    m.ApplicationUserId == userId &&
                    m.ViewState &&
                    (m.Role == WorkspaceRoleType.Owner || m.Role == WorkspaceRoleType.Admin)));
    }

    public async Task<bool> CanManageProjectAsync(int projectId, int userId)
    {
        var workspaceId = await _repository.Query()
            .Where(x => x.Id == projectId)
            .Select(x => x.WorkspaceId)
            .FirstOrDefaultAsync();

        if (workspaceId == 0)
            return false;

        // Check if user can manage workspace
        if (await CanManageProjectsAsync(workspaceId, userId))
            return true;

        // Check if user is project manager
        return await _context.ProjectMembers
            .AnyAsync(x =>
                x.ProjectId == projectId &&
                x.ApplicationUserId == userId &&
                x.ViewState &&
                x.Role == ProjectRoleType.Manager);
    }

    public new async Task DeleteAsync(int id)
    {
        await _context.Projects
            .Where(x => x.Id == id)
            .ExecuteUpdateAsync(p => p.SetProperty(x => x.ViewState, false));
    }

    public async Task ArchiveAsync(int id)
    {
        var updated = await _context.Projects
            .Where(x => x.Id == id && x.ViewState && !x.IsArchived)
            .ExecuteUpdateAsync(p => p
                .SetProperty(x => x.IsArchived, true)
                .SetProperty(x => x.ChangeDate, DateTime.Now));

        if (updated == 0)
            throw new InvalidOperationException("پروژه یافت نشد یا قبلاً بایگانی‌شده است.");
    }

    public async Task RestoreAsync(int id)
    {
        var updated = await _context.Projects
            .Where(x => x.Id == id && x.ViewState && x.IsArchived)
            .ExecuteUpdateAsync(p => p
                .SetProperty(x => x.IsArchived, false)
                .SetProperty(x => x.ChangeDate, DateTime.Now));

        if (updated == 0)
            throw new InvalidOperationException("پروژه یافت نشد یا قبلاً بازیابی‌شده است.");
    }

    public async Task UpdatePreferencesAsync(int id, string color, string icon)
    {
        var updated = await _context.Projects
            .Where(x => x.Id == id && x.ViewState)
            .ExecuteUpdateAsync(p => p
                .SetProperty(x => x.Color, color)
                .SetProperty(x => x.Icon, icon)
                .SetProperty(x => x.ChangeDate, DateTime.Now));

        if (updated == 0)
            throw new InvalidOperationException("پروژه یافت نشد.");
    }
}
