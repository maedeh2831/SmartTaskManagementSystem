using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using SmartTask.Web.Data.Context;
using SmartTask.Web.Models.Entities;
using SmartTask.Web.Services.Interfaces;

namespace SmartTask.Web.Services.Implementations
{
    public class ProjectTeamService : IProjectTeamService
    {
        private readonly ApplicationDbContext _context;

        public ProjectTeamService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<List<(int ProjectId, string ProjectName)>> GetProjectsForTeamAsync(int teamId)
        {
            return await _context.ProjectTeams
                .Where(x => x.TeamId == teamId && x.ViewState)
                .Include(x => x.Project)
                .Select(x => new { x.ProjectId, x.Project.Name })
                .ToListAsync()
                .ContinueWith(t => t.Result.Select(x => (x.ProjectId, x.Name)).ToList());
        }

        public async Task<List<SelectListItem>> GetAvailableProjectsAsync(int workspaceId, int teamId)
        {
            var assignedProjectIds = await _context.ProjectTeams
                .Where(x => x.TeamId == teamId && x.ViewState)
                .Select(x => x.ProjectId)
                .ToListAsync();

            return await _context.Projects
                .Where(x =>
                    x.WorkspaceId == workspaceId &&
                    x.ViewState &&
                    !assignedProjectIds.Contains(x.Id))
                .Select(x => new SelectListItem
                {
                    Value = x.Id.ToString(),
                    Text = x.Name
                })
                .ToListAsync();
        }

        public async Task AssignTeamToProjectAsync(int projectId, int teamId)
        {
            var existing = await _context.ProjectTeams
                .FirstOrDefaultAsync(x => x.ProjectId == projectId && x.TeamId == teamId);

            if (existing != null)
            {
                if (existing.ViewState)
                    return;

                existing.ViewState = true;
                existing.AssignedDate = DateTime.Now;
                existing.ChangeDate = DateTime.Now;

                await _context.SaveChangesAsync();
                return;
            }

            var projectTeam = new ProjectTeam
            {
                ProjectId = projectId,
                TeamId = teamId,
                AssignedDate = DateTime.Now,
                CreatedDate = DateTime.Now,
                ViewState = true
            };

            await _context.ProjectTeams.AddAsync(projectTeam);
            await _context.SaveChangesAsync();
        }

        public async Task RemoveTeamFromProjectAsync(int projectId, int teamId)
        {
            var projectTeam = await _context.ProjectTeams
                .FirstOrDefaultAsync(x =>
                    x.ProjectId == projectId &&
                    x.TeamId == teamId &&
                    x.ViewState);

            if (projectTeam == null)
                return;

            projectTeam.ViewState = false;
            projectTeam.ChangeDate = DateTime.Now;

            await _context.SaveChangesAsync();
        }
    }
}