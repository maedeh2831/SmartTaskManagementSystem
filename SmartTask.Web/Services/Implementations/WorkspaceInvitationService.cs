using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using SmartTask.Web.Data.Context;
using SmartTask.Web.Infrastructure.Interfaces;
using SmartTask.Web.Models.Entities;
using SmartTask.Web.Models.Enums;
using SmartTask.Web.Models.ViewModels.Workspace;
using SmartTask.Web.Services.Email;
using SmartTask.Web.Services.Interfaces;
namespace SmartTask.Web.Services.Implementations;
public class WorkspaceInvitationService
    : BaseService<WorkspaceInvitation>, IWorkspaceInvitationService
{
    private readonly ApplicationDbContext _context;
    private readonly IEmailService _emailService;
    private readonly IWorkspaceMemberService _workspaceMemberService;
    private readonly IHttpContextAccessor _httpContextAccessor;

    public WorkspaceInvitationService(
        IGenericRepository<WorkspaceInvitation> repository,
        IUnitOfWork unitOfWork,
        ApplicationDbContext context,
        IEmailService emailService,
        IWorkspaceMemberService workspaceMemberService,
        IHttpContextAccessor httpContextAccessor)
        : base(repository, unitOfWork)
    {
        _context = context;
        _emailService = emailService;
        _workspaceMemberService = workspaceMemberService;
        _httpContextAccessor = httpContextAccessor;
    }

    public async Task<List<WorkspaceInvitationViewModel>> GetPendingInvitationsAsync(int workspaceId)
    {
        var expiredOnes = await _context.WorkspaceInvitations
            .Where(x =>
                x.WorkspaceId == workspaceId &&
                x.Status == WorkspaceInvitationStatusType.Pending &&
                x.ExpiryDate < DateTime.Now)
            .ToListAsync();

        foreach (var item in expiredOnes)
            item.Status = WorkspaceInvitationStatusType.Expired;

        if (expiredOnes.Any())
            await _context.SaveChangesAsync();

        return await _context.WorkspaceInvitations
            .Where(x =>
                x.WorkspaceId == workspaceId &&
                x.Status == WorkspaceInvitationStatusType.Pending)
            .Include(x => x.InvitedUser)
            .OrderByDescending(x => x.CreatedDate)
            .Select(x => new WorkspaceInvitationViewModel
            {
                Id = x.Id,
                WorkspaceId = x.WorkspaceId,
                Email = x.Email,
                FullName = x.InvitedUser != null
                    ? (x.InvitedUser.FirstName + " " + x.InvitedUser.LastName).Trim()
                    : null,
                Role = x.Role,
                Status = x.Status,
                CreateDate = x.CreatedDate,
                ExpiryDate = x.ExpiryDate,
                IsNewUser = x.InvitedUserId == null
            })
            .ToListAsync();
    }

    public async Task InviteAsync(
        int workspaceId,
        List<int> userIds,
        List<string> emails,
        WorkspaceRoleType role,
        int invitedByUserId)
    {
        var workspace = await _context.Workspaces
            .FirstOrDefaultAsync(x => x.Id == workspaceId);

        if (workspace == null)
            throw new Exception("فضای کاری یافت نشد.");

        var inviter = await _context.Users
            .FirstOrDefaultAsync(x => x.Id == invitedByUserId);

        var inviterName = inviter != null
            ? $"{inviter.FirstName} {inviter.LastName}".Trim()
            : "یکی از اعضای SmartTask";

        foreach (var userId in userIds ?? new List<int>())
        {
            var user = await _context.Users.FirstOrDefaultAsync(x => x.Id == userId);
            if (user == null)
                continue;

            await CreateOrReactivateInvitationAsync(
                workspaceId, user.Email!, user.Id, role,
                invitedByUserId, workspace.Name, inviterName, isNewUser: false);
        }

        var normalizedEmails = (emails ?? new List<string>())
            .Where(x => !string.IsNullOrWhiteSpace(x))
            .Select(x => x.Trim().ToLower())
            .Distinct()
            .ToList();

        foreach (var email in normalizedEmails)
        {
            var existingUser = await _context.Users
                .FirstOrDefaultAsync(x => x.Email!.ToLower() == email);

            await CreateOrReactivateInvitationAsync(
                workspaceId, email, existingUser?.Id, role,
                invitedByUserId, workspace.Name, inviterName,
                isNewUser: existingUser == null);
        }
    }

    private async Task CreateOrReactivateInvitationAsync(
        int workspaceId,
        string email,
        int? invitedUserId,
        WorkspaceRoleType role,
        int invitedByUserId,
        string workspaceName,
        string inviterName,
        bool isNewUser)
    {
        if (invitedUserId.HasValue)
        {
            var alreadyMember = await _context.WorkspaceMembers
                .AnyAsync(x =>
                    x.WorkspaceId == workspaceId &&
                    x.ApplicationUserId == invitedUserId.Value &&
                    x.ViewState);

            if (alreadyMember)
                return;
        }

        var existingInvitation = await _context.WorkspaceInvitations
            .FirstOrDefaultAsync(x =>
                x.WorkspaceId == workspaceId &&
                x.Email.ToLower() == email.ToLower());

        WorkspaceInvitation invitation;

        if (existingInvitation != null)
        {
            if (existingInvitation.Status == WorkspaceInvitationStatusType.Pending &&
                existingInvitation.ExpiryDate >= DateTime.Now)
                return;

            existingInvitation.Role = role;
            existingInvitation.Status = WorkspaceInvitationStatusType.Pending;
            existingInvitation.Token = Guid.NewGuid();
            existingInvitation.InvitedByUserId = invitedByUserId;
            existingInvitation.InvitedUserId = invitedUserId;
            existingInvitation.ExpiryDate = DateTime.Now.AddDays(3);
            existingInvitation.ChangeDate = DateTime.Now;
            existingInvitation.AcceptedDate = null;

            invitation = existingInvitation;
        }
        else
        {
            invitation = new WorkspaceInvitation
            {
                WorkspaceId = workspaceId,
                Email = email,
                InvitedUserId = invitedUserId,
                Role = role,
                Token = Guid.NewGuid(),
                Status = WorkspaceInvitationStatusType.Pending,
                InvitedByUserId = invitedByUserId,
                ExpiryDate = DateTime.Now.AddDays(3)
            };

            await _context.WorkspaceInvitations.AddAsync(invitation);
        }

        await _context.SaveChangesAsync();

        await SendInvitationEmailAsync(invitation, workspaceName, inviterName, isNewUser);
    }

    private async Task SendInvitationEmailAsync(
        WorkspaceInvitation invitation,
        string workspaceName,
        string inviterName,
        bool isNewUser)
    {
        var request = _httpContextAccessor.HttpContext!.Request;
        var baseUrl = $"{request.Scheme}://{request.Host}";

        var actionLink = isNewUser
            ? $"{baseUrl}/Account/Register?invitationToken={invitation.Token}"
            : $"{baseUrl}/WorkspaceMember/AcceptInvitation?token={invitation.Token}";

        var actionText = isNewUser
            ? "ثبت‌نام و پیوستن به Workspace"
            : "پذیرفتن دعوت";

        await _emailService.SendEmailAsync(
            invitation.Email,
            $"دعوت به فضای کاری {workspaceName}",
            $@"
                <h2>دعوت به همکاری در SmartTask 👋</h2>
                <p>{inviterName} شما را به فضای کاری «{workspaceName}» دعوت کرده است.</p>
                <p>
                    <a href='{actionLink}'
                       style='background:#4F46E5;color:white;padding:12px 25px;
                              text-decoration:none;border-radius:8px'>
                        {actionText}
                    </a>
                </p>
                <p>این دعوت‌نامه تا 3 روز دیگر معتبر است.</p>
                ");
    }

    public async Task<WorkspaceInvitation?> GetByTokenAsync(Guid token)
    {
        return await _context.WorkspaceInvitations
            .Include(x => x.Workspace)
            .FirstOrDefaultAsync(x => x.Token == token);
    }

    public async Task AcceptInvitationAsync(Guid token, int currentUserId)
    {
        var invitation = await _context.WorkspaceInvitations
            .FirstOrDefaultAsync(x => x.Token == token);

        if (invitation == null)
            throw new Exception("دعوت‌نامه یافت نشد.");

        if (invitation.Status != WorkspaceInvitationStatusType.Pending)
            throw new Exception("این دعوت‌نامه قبلاً پاسخ داده شده یا لغو شده است.");

        if (invitation.ExpiryDate < DateTime.Now)
        {
            invitation.Status = WorkspaceInvitationStatusType.Expired;
            await _context.SaveChangesAsync();
            throw new Exception("این دعوت‌نامه منقضی شده است.");
        }

        var currentUser = await _context.Users
            .FirstOrDefaultAsync(x => x.Id == currentUserId);

        if (currentUser == null ||
            !string.Equals(currentUser.Email, invitation.Email, StringComparison.OrdinalIgnoreCase))
        {
            throw new Exception("این دعوت‌نامه متعلق به حساب کاربری شما نیست.");
        }

        await _workspaceMemberService.InviteMemberAsync(
            invitation.WorkspaceId, currentUserId, invitation.Role);

        invitation.InvitedUserId = currentUserId;
        invitation.Status = WorkspaceInvitationStatusType.Accepted;
        invitation.AcceptedDate = DateTime.Now;

        await _context.SaveChangesAsync();
    }

    public async Task AcceptInvitationAfterRegisterAsync(Guid token, int newUserId)
    {
        var invitation = await _context.WorkspaceInvitations
            .FirstOrDefaultAsync(x => x.Token == token);

        if (invitation == null || invitation.Status != WorkspaceInvitationStatusType.Pending)
            return;

        if (invitation.ExpiryDate < DateTime.Now)
        {
            invitation.Status = WorkspaceInvitationStatusType.Expired;
            await _context.SaveChangesAsync();
            return;
        }

        await _workspaceMemberService.InviteMemberAsync(
            invitation.WorkspaceId, newUserId, invitation.Role);

        invitation.InvitedUserId = newUserId;
        invitation.Status = WorkspaceInvitationStatusType.Accepted;
        invitation.AcceptedDate = DateTime.Now;

        await _context.SaveChangesAsync();
    }

    public async Task CancelInvitationAsync(int invitationId)
    {
        var invitation = await _context.WorkspaceInvitations
            .FirstOrDefaultAsync(x => x.Id == invitationId);

        if (invitation == null)
            return;

        invitation.Status = WorkspaceInvitationStatusType.Cancelled;
        invitation.ChangeDate = DateTime.Now;

        await _context.SaveChangesAsync();
    }
}