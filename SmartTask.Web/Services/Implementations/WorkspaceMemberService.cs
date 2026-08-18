using Microsoft.EntityFrameworkCore;
using SmartTask.Web.Data.Context;
using SmartTask.Web.Infrastructure.Interfaces;
using SmartTask.Web.Models.Entities;
using SmartTask.Web.Models.Enums;
using SmartTask.Web.Models.ViewModels.Workspace;
using SmartTask.Web.Services.Interfaces;

namespace SmartTask.Web.Services.Implementations;

public class WorkspaceMemberService
    : BaseService<WorkspaceMember>, IWorkspaceMemberService
{
    private readonly ApplicationDbContext _context;

    public WorkspaceMemberService(
        IGenericRepository<WorkspaceMember> repository,
        IUnitOfWork unitOfWork,
        ApplicationDbContext context)
        : base(repository, unitOfWork)
    {
        _context = context;
    }

    public async Task<List<WorkspaceMemberViewModel>> GetMembersAsync(
    int workspaceId,
    int currentUserId)
    {
        return await _context.WorkspaceMembers
            .Where(x => x.WorkspaceId == workspaceId && x.ViewState)
            .Include(x => x.ApplicationUser)
            .Select(x => new WorkspaceMemberViewModel
            {
                Id = x.Id,
                WorkspaceId = x.WorkspaceId,
                UserId = x.ApplicationUserId,
                FullName = x.ApplicationUser.FullName,
                Email = x.ApplicationUser.Email!,
                Avatar = x.ApplicationUser.Avatar,
                Role = x.Role,
                IsOwner = x.Role == WorkspaceRoleType.Owner,
                IsCurrentUser = x.ApplicationUserId == currentUserId
            })
            .OrderBy(x => x.Role)
            .ToListAsync();
    }

    public async Task InviteMemberAsync(
        int workspaceId,
        int userId,
        WorkspaceRoleType role)
    {
        var existingMember = await _context.WorkspaceMembers
            .FirstOrDefaultAsync(x =>
                x.WorkspaceId == workspaceId &&
                x.ApplicationUserId == userId);

        if (existingMember != null)
        {
            if (existingMember.ViewState)
                throw new Exception("این کاربر قبلاً عضو Workspace شده است.");

            existingMember.ViewState = true;
            existingMember.Role = role;
            existingMember.ChangeDate = DateTime.Now;

            await _context.SaveChangesAsync();
            return;
        }

        var member = new WorkspaceMember
        {
            WorkspaceId = workspaceId,
            ApplicationUserId = userId,
            Role = role,
            ViewState = true
        };

        await _context.WorkspaceMembers.AddAsync(member);
        await _context.SaveChangesAsync();
    }

    // OPTIMIZED: Use ExecuteUpdateAsync instead of load-modify-save
    public async Task RemoveMemberAsync(int memberId)
    {
        var role = await _context.WorkspaceMembers
            .Where(x => x.Id == memberId)
            .Select(x => x.Role)
            .FirstOrDefaultAsync();

        if (role == WorkspaceRoleType.Owner)
            throw new Exception("مالک Workspace قابل حذف نیست.");

        await _context.WorkspaceMembers
            .Where(x => x.Id == memberId)
            .ExecuteUpdateAsync(u => u
                .SetProperty(x => x.ViewState, false)
                .SetProperty(x => x.ChangeDate, DateTime.Now));
    }

    // OPTIMIZED: Use ExecuteUpdateAsync instead of load-modify-save
    public async Task ChangeRoleAsync(
        int memberId,
        WorkspaceRoleType role)
    {
        var currentRole = await _context.WorkspaceMembers
            .Where(x => x.Id == memberId)
            .Select(x => x.Role)
            .FirstOrDefaultAsync();

        if (currentRole == WorkspaceRoleType.Owner)
            throw new Exception("نقش مالک قابل تغییر نیست.");

        await _context.WorkspaceMembers
            .Where(x => x.Id == memberId)
            .ExecuteUpdateAsync(u => u
                .SetProperty(x => x.Role, role)
                .SetProperty(x => x.ChangeDate, DateTime.Now));
    }

    public async Task<bool> IsMemberAsync(
        int workspaceId,
        int userId)
    {
        return await _context.WorkspaceMembers
            .AnyAsync(x =>
                x.WorkspaceId == workspaceId &&
                x.ApplicationUserId == userId &&
                x.ViewState);
    }

    public async Task<bool> IsOwnerOrAdminAsync(
        int workspaceId,
        int userId)
    {
        return await _context.WorkspaceMembers
            .AnyAsync(x =>
                x.WorkspaceId == workspaceId &&
                x.ApplicationUserId == userId &&
                x.ViewState &&
                (x.Role == WorkspaceRoleType.Owner ||
                 x.Role == WorkspaceRoleType.Admin));
    }
}