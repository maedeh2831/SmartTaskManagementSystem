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
        var query = _repository
            .Query()
            .Where(x =>
            x.WorkspaceId == workspaceId &&
            x.Key == key &&
            x.ViewState);

        if (excludeId.HasValue)
            query = query.Where(x => x.Id != excludeId.Value);

        return await query.AnyAsync();
    }

    public async Task<bool> CanManageProjectsAsync(int workspaceId, int userId)
    {
        var isOwner = await _context.Workspaces
            .AnyAsync(x => x.Id == workspaceId && x.OwnerId == userId);

        if (isOwner)
            return true;

        return await _context.WorkspaceMembers
            .AnyAsync(x =>
                x.WorkspaceId == workspaceId &&
                x.ApplicationUserId == userId &&
                x.ViewState &&
                (x.Role == WorkspaceRoleType.Owner || x.Role == WorkspaceRoleType.Admin));
    }

    public async Task<bool> CanManageProjectAsync(int projectId, int userId)
    {
        var project = await _repository
            .Query()
            .FirstOrDefaultAsync(x => x.Id == projectId);

        if (project == null)
            return false;

        if (await CanManageProjectsAsync(project.WorkspaceId, userId))
            return true;

        return await _context.ProjectMembers
            .AnyAsync(x =>
                x.ProjectId == projectId &&
                x.ApplicationUserId == userId &&
                x.ViewState &&
                x.Role == ProjectRoleType.Manager);
    }

    public new async Task DeleteAsync(int id)
    {
        var project = await _context.Projects
            .FirstOrDefaultAsync(x => x.Id == id);

        if (project == null)
            return;

        project.ViewState = false;
        await _context.SaveChangesAsync();
    }

    public async Task ArchiveAsync(int id)
    {
        var project = await _context.Projects
            .FirstOrDefaultAsync(x => x.Id == id && x.ViewState);

        if (project == null)
            throw new Exception("پروژه یافت نشد.");

        if (project.IsArchived)
            return;

        project.IsArchived = true;
        project.ChangeDate = DateTime.Now;

        await _context.SaveChangesAsync();
    }

    public async Task RestoreAsync(int id)
    {
        var project = await _context.Projects
            .FirstOrDefaultAsync(x => x.Id == id && x.ViewState);

        if (project == null)
            throw new Exception("پروژه یافت نشد.");

        if (!project.IsArchived)
            return;

        project.IsArchived = false;
        project.ChangeDate = DateTime.Now;

        await _context.SaveChangesAsync();
    }

    public async Task UpdatePreferencesAsync(int id, string color, string icon)
    {
        var project = await _context.Projects
            .FirstOrDefaultAsync(x => x.Id == id && x.ViewState);

        if (project == null)
            throw new Exception("پروژه یافت نشد.");

        project.Color = color;
        project.Icon = icon;
        project.ChangeDate = DateTime.Now;

        await _context.SaveChangesAsync();
    }
}