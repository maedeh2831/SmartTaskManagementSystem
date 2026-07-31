using Microsoft.AspNetCore.Mvc.Rendering;

namespace SmartTask.Web.Services.Interfaces;

public interface IProjectTeamService
{
    Task<List<(int ProjectId, string ProjectName)>> GetProjectsForTeamAsync(int teamId);
    Task<List<SelectListItem>> GetAvailableProjectsAsync(int workspaceId, int teamId);
    Task AssignTeamToProjectAsync(int projectId, int teamId);
    Task RemoveTeamFromProjectAsync(int projectId, int teamId);
}