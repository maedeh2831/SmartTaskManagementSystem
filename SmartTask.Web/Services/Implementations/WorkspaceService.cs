using Microsoft.EntityFrameworkCore;
using SmartTask.Web.Data.Context;
using SmartTask.Web.Infrastructure.Interfaces;
using SmartTask.Web.Models.Entities;
using SmartTask.Web.Services.Interfaces;

namespace SmartTask.Web.Services.Implementations;

public class WorkspaceService
    : BaseService<Workspace>, IWorkspaceService
{

    private readonly ApplicationDbContext _context;

    public WorkspaceService(
        IGenericRepository<Workspace> repository,
        IUnitOfWork unitOfWork,
        ApplicationDbContext context)
        : base(repository, unitOfWork)
    {
        _context = context;
    }

    public async Task<Workspace?> GetDetailsAsync(int id)
    {
        return await _context.Workspaces

            .Include(x => x.Owner)

            .Include(x => x.Members)

            .Include(x => x.Projects)

            .FirstOrDefaultAsync(x => x.Id == id);
    }

    public async Task<bool> ExistsByNameAsync(
        string name,
        int? excludeId = null)
    {
        var query = _repository
            .Query()
            .Where(x => x.Name == name);


        if (excludeId.HasValue)
        {
            query = query.Where(x => x.Id != excludeId.Value);
        }


        return await query.AnyAsync();
    }

    public async Task<bool> IsOwnerAsync(
        int workspaceId,
        int userId)
    {
        return await _repository
            .Query()
            .AnyAsync(x =>
                x.Id == workspaceId &&
                x.OwnerId == userId);
    }

    public new async Task DeleteAsync(int id)
    {
        var workspace = await _context.Workspaces
            .FirstOrDefaultAsync(x => x.Id == id);

        if (workspace == null)
            return;


        workspace.ViewState = false;

        await _context.SaveChangesAsync();
    }

    public async Task UpdateSettingsAsync(
    int workspaceId,
    string? logoPath,
    string color,
    string timeZone)
    {
        var workspace = await _context.Workspaces
            .FirstOrDefaultAsync(x => x.Id == workspaceId);

        if (workspace == null)
            throw new Exception("فضای کاری یافت نشد.");

        if (!string.IsNullOrWhiteSpace(logoPath))
            workspace.Logo = logoPath;

        workspace.Color = color;
        workspace.TimeZone = timeZone;
        workspace.ChangeDate = DateTime.Now;

        await _context.SaveChangesAsync();
    }

}