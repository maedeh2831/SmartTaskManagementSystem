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

    public async Task<List<WorkspaceMemberViewModel>> GetMembersAsync(int workspaceId)
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
                IsOwner = x.Role == WorkspaceRoleType.Owner
            })
            .OrderBy(x => x.Role)
            .ToListAsync();
    }

    public async Task InviteMemberAsync(
        int workspaceId,
        int userId,
        WorkspaceRoleType role)
    {
        var exists = await _context.WorkspaceMembers
            .AnyAsync(x =>
                x.WorkspaceId == workspaceId &&
                x.ApplicationUserId == userId &&
                x.ViewState);

        if (exists)
            throw new Exception("این کاربر قبلاً عضو Workspace شده است.");

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

    public async Task RemoveMemberAsync(int memberId)
    {
        var member = await _context.WorkspaceMembers
            .FirstOrDefaultAsync(x => x.Id == memberId);

        if (member == null)
            return;

        if (member.Role == WorkspaceRoleType.Owner)
            throw new Exception("مالک Workspace قابل حذف نیست.");

        member.ViewState = false;
        member.ChangeDate = DateTime.Now;

        await _context.SaveChangesAsync();
    }

    public async Task ChangeRoleAsync(
        int memberId,
        WorkspaceRoleType role)
    {
        var member = await _context.WorkspaceMembers
            .FirstOrDefaultAsync(x => x.Id == memberId);

        if (member == null)
            return;

        if (member.Role == WorkspaceRoleType.Owner)
            throw new Exception("نقش مالک قابل تغییر نیست.");

        member.Role = role;
        member.ChangeDate = DateTime.Now;

        await _context.SaveChangesAsync();
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