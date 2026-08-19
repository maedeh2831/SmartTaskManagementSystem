using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SmartTask.Web.Data.Context;
using SmartTask.Web.Infrastructure.Interfaces;
using SmartTask.Web.Models.ViewModels.Search;

namespace SmartTask.Web.Controllers;

[Authorize]
public class SearchController : BaseController
{
    private readonly ApplicationDbContext _context;

    public SearchController(ApplicationDbContext context, ICurrentUserService currentUser)
        : base(currentUser)
    {
        _context = context;
    }

    [HttpGet]
    public async Task<IActionResult> GlobalSearch(string q)
    {
        if (string.IsNullOrWhiteSpace(q) || q.Trim().Length < 2)
            return Json(new List<GlobalSearchResultViewModel>());

        q = q.Trim();
        var userId = CurrentUser.UserId;

        var accessibleProjectIds = await _context.ProjectMembers
            .Where(x => x.ApplicationUserId == userId && x.ViewState)
            .Select(x => x.ProjectId)
            .ToListAsync();

        var results = new List<GlobalSearchResultViewModel>();

        // ---- پروژه‌ها ----
        var projectEntities = await _context.Projects
            .Where(x => accessibleProjectIds.Contains(x.Id) && x.ViewState && x.Name.Contains(q))
            .OrderByDescending(x => x.CreatedDate)
            .Take(5)
            .ToListAsync();

        results.AddRange(projectEntities.Select(x => new GlobalSearchResultViewModel
        {
            Type = "Project",
            Id = x.Id,
            Title = x.Name,
            SubTitle = x.Key,
            Icon = x.Icon ?? "fa-solid fa-diagram-project",
            Color = x.Color ?? "#4F46E5",
            Url = Url.Action("Details", "Project", new { id = x.Id })!
        }));

        // ---- User Story ها ----
        var storyEntities = await _context.UserStories
            .Where(x => accessibleProjectIds.Contains(x.ProjectId) && x.ViewState && x.Title.Contains(q))
            .Include(x => x.Project)
            .OrderByDescending(x => x.CreatedDate)
            .Take(5)
            .ToListAsync();

        results.AddRange(storyEntities.Select(x => new GlobalSearchResultViewModel
        {
            Type = "UserStory",
            Id = x.Id,
            Title = x.Title,
            SubTitle = x.Project.Name,
            Icon = "fa-solid fa-bookmark",
            Color = x.Project.Color ?? "#4F46E5",
            Url = Url.Action("Details", "UserStory", new { id = x.Id })!
        }));

        // ---- Task ها ----
        var taskEntities = await _context.TaskItems
            .Where(x => accessibleProjectIds.Contains(x.UserStory.ProjectId) && x.ViewState && x.Title.Contains(q))
            .Include(x => x.UserStory).ThenInclude(us => us.Project)
            .OrderByDescending(x => x.CreatedDate)
            .Take(5)
            .ToListAsync();

        results.AddRange(taskEntities.Select(x => new GlobalSearchResultViewModel
        {
            Type = "Task",
            Id = x.Id,
            Title = x.Title,
            SubTitle = $"{x.UserStory.Project.Name} · {x.UserStory.Title}",
            Icon = "fa-solid fa-square-check",
            Color = x.UserStory.Project.Color ?? "#4F46E5",
            Url = Url.Action("Details", "Task", new { id = x.Id })!
        }));

        // ---- اسپرینت‌ها ----
        var accessibleWorkspaceIds = await _context.WorkspaceMembers
            .Where(x => x.ApplicationUserId == userId && x.ViewState)
            .Select(x => x.WorkspaceId)
            .ToListAsync();

        var sprintEntities = await _context.Sprints
            .Where(x => accessibleProjectIds.Contains(x.ProjectId) && x.ViewState && x.Name.Contains(q))
            .Include(x => x.Project)
            .OrderByDescending(x => x.StartDate)
            .Take(5)
            .ToListAsync();

        results.AddRange(sprintEntities.Select(x => new GlobalSearchResultViewModel
        {
            Type = "Sprint",
            Id = x.Id,
            Title = x.Name,
            SubTitle = x.Project.Name,
            Icon = "fa-solid fa-bolt",
            Color = x.Project.Color ?? "#F59E0B",
            Url = Url.Action("Details", "Sprint", new { id = x.Id })!
        }));

        // ---- فضاهای کاری ----
        var workspaceEntities = await _context.Workspaces
            .Where(x => accessibleWorkspaceIds.Contains(x.Id) && x.ViewState && x.Name.Contains(q))
            .OrderByDescending(x => x.CreateDate)
            .Take(3)
            .ToListAsync();

        results.AddRange(workspaceEntities.Select(x => new GlobalSearchResultViewModel
        {
            Type = "Workspace",
            Id = x.Id,
            Title = x.Name,
            SubTitle = "فضای کاری",
            Icon = "fa-solid fa-layer-group",
            Color = "#6366F1",
            Url = Url.Action("Details", "Workspace", new { id = x.Id })!
        }));

        // ---- برچسب‌ها ----
        var labelEntities = await _context.Labels
            .Where(x => accessibleProjectIds.Contains(x.ProjectId) && x.ViewState && x.Name.Contains(q))
            .Include(x => x.Project)
            .Take(3)
            .ToListAsync();

        results.AddRange(labelEntities.Select(x => new GlobalSearchResultViewModel
        {
            Type = "Label",
            Id = x.Id,
            Title = x.Name,
            SubTitle = x.Project.Name,
            Icon = "fa-solid fa-tags",
            Color = x.Color ?? "#8B5CF6",
            Url = Url.Action("Index", "Label", new { projectId = x.ProjectId })!
        }));

        return Json(results);
    }

    [HttpGet]
    public async Task<IActionResult> ProjectsForQuickAdd()
    {
        var userId = CurrentUser.UserId;

        var memberships = await _context.ProjectMembers
            .Where(x => x.ApplicationUserId == userId && x.ViewState)
            .Include(x => x.Project).ThenInclude(p => p.Workspace)
            .ToListAsync();

        var projects = memberships
            .Select(x => x.Project)
            .Where(p => p.ViewState && !p.IsArchived)
            .OrderByDescending(p => p.CreatedDate)
            .Select(p => new QuickAddProjectViewModel
            {
                Id = p.Id,
                Name = p.Name,
                Key = p.Key,
                WorkspaceName = p.Workspace.Name,
                Color = p.Color ?? "#4F46E5",
                Icon = p.Icon ?? "fa-solid fa-diagram-project"
            })
            .ToList();

        return Json(projects);
    }

    [HttpGet]
    public async Task<IActionResult> StoriesForQuickAdd(int projectId)
    {
        var stories = await _context.UserStories
            .Where(x => x.ProjectId == projectId && x.ViewState)
            .OrderByDescending(x => x.CreatedDate)
            .Select(x => new QuickAddStoryViewModel
            {
                Id = x.Id,
                Title = x.Title
            })
            .ToListAsync();

        return Json(stories);
    }
}