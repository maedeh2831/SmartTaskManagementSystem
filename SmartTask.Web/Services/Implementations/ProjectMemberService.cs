using Microsoft.EntityFrameworkCore;
using SmartTask.Web.Data.Context;
using SmartTask.Web.Models.Entities;
using SmartTask.Web.Models.Enums;
using SmartTask.Web.Services.Interfaces;

namespace SmartTask.Web.Services.Implementations;

public class ProjectMemberService : IProjectMemberService
{
    private readonly ApplicationDbContext _context;

    public ProjectMemberService(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<bool> IsMemberAsync(int projectId, int userId)
    {
        return await _context.ProjectMembers
            .AnyAsync(x => x.ProjectId == projectId && x.ApplicationUserId == userId && x.ViewState);
    }

    public async Task AddMemberAsync(int projectId, int userId, ProjectRoleType role)
    {
        var existing = await _context.ProjectMembers
            .FirstOrDefaultAsync(x => x.ProjectId == projectId && x.ApplicationUserId == userId);

        if (existing != null)
        {
            if (existing.ViewState)
                return;

            existing.ViewState = true;
            existing.Role = role;
            existing.JoinedDate = DateTime.Now;
            existing.ChangeDate = DateTime.Now;
            await _context.SaveChangesAsync();
            return;
        }

        _context.ProjectMembers.Add(new ProjectMember
        {
            ProjectId = projectId,
            ApplicationUserId = userId,
            Role = role,
            JoinedDate = DateTime.Now,
            CreatedDate = DateTime.Now,
            ViewState = true
        });

        await _context.SaveChangesAsync();
    }

    public async Task RemoveMemberAsync(int projectId, int userId)
    {
        var member = await _context.ProjectMembers
            .FirstOrDefaultAsync(x => x.ProjectId == projectId && x.ApplicationUserId == userId && x.ViewState);

        if (member == null)
            return;

        if (member.Role == ProjectRoleType.Owner)
        {
            var ownersCount = await _context.ProjectMembers
                .CountAsync(x => x.ProjectId == projectId && x.Role == ProjectRoleType.Owner && x.ViewState);

            if (ownersCount <= 1)
                throw new InvalidOperationException("امکان حذف تنها مالک پروژه وجود ندارد.");
        }

        member.ViewState = false;
        member.ChangeDate = DateTime.Now;
        await _context.SaveChangesAsync();
    }

    public async Task ChangeRoleAsync(int projectId, int userId, ProjectRoleType role)
    {
        var member = await _context.ProjectMembers
            .FirstOrDefaultAsync(x => x.ProjectId == projectId && x.ApplicationUserId == userId && x.ViewState);

        if (member == null)
            return;

        if (member.Role == ProjectRoleType.Owner && role != ProjectRoleType.Owner)
        {
            var ownersCount = await _context.ProjectMembers
                .CountAsync(x => x.ProjectId == projectId && x.Role == ProjectRoleType.Owner && x.ViewState);

            if (ownersCount <= 1)
                throw new InvalidOperationException("امکان تغییر نقش تنها مالک پروژه وجود ندارد.");
        }

        member.Role = role;
        member.ChangeDate = DateTime.Now;
        await _context.SaveChangesAsync();
    }
}