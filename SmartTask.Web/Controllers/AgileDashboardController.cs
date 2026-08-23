using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SmartTask.Web.Data.Context;
using SmartTask.Web.Infrastructure.Interfaces;
using SmartTask.Web.Models.Enums;
using SmartTask.Web.Models.ViewModels.AgileDashboard;
using SmartTask.Web.Services.Interfaces;

namespace SmartTask.Web.Controllers;

[Authorize]
public class AgileDashboardController : BaseController
{
    private readonly ISprintService _sprintService;
    private readonly ApplicationDbContext _context;

    public AgileDashboardController(
        ISprintService sprintService,
        ICurrentUserService currentUser,
        ApplicationDbContext context)
        : base(currentUser)
    {
        _sprintService = sprintService;
        _context = context;
    }

    public async Task<IActionResult> Index(int projectId, int? sprintId)
    {
        var project = await _context.Projects
            .FirstOrDefaultAsync(x => x.Id == projectId);

        if (project == null)
            return NotFound();

        if (!await IsProjectMemberAsync(_context, projectId))
        {
            TempData["Error"] = "شما عضو این پروژه نیستید.";
            return RedirectToAction("Index", "Workspace");
        }

        var allSprints = await _context.Sprints
            .Where(x => x.ProjectId == projectId && x.ViewState)
            .OrderByDescending(x => x.StartDate)
            .ToListAsync();

        if (!allSprints.Any())
        {
            TempData["Error"] = "برای مشاهده داشبورد، ابتدا باید حداقل یک اسپرینت ایجاد کنید.";
            return RedirectToAction("Index", "Sprint", new { projectId });
        }

        var selectedSprint =
            (sprintId.HasValue ? allSprints.FirstOrDefault(x => x.Id == sprintId.Value) : null) ??
            allSprints.FirstOrDefault(x => x.Status == SprintStatusType.Active) ??
            allSprints.First();

        var stories = await _context.UserStories
            .Where(x => x.SprintId == selectedSprint.Id && x.ViewState)
            .ToListAsync();

        var vm = new AgileDashboardViewModel
        {
            ProjectId = projectId,
            ProjectName = project.Name,
            SprintId = selectedSprint.Id,
            SprintName = selectedSprint.Name,
            SprintStatus = selectedSprint.Status,
            StartDate = selectedSprint.StartDate,
            EndDate = selectedSprint.EndDate,
            Capacity = selectedSprint.Capacity,
            TotalStories = stories.Count,
            DoneStories = stories.Count(x => x.Status == StoryStatusType.Done),
            InProgressStories = stories.Count(x => x.Status == StoryStatusType.InProgress),
            TodoStories = stories.Count(x => x.Status == StoryStatusType.New || x.Status == StoryStatusType.Ready),
            TotalPoints = stories.Sum(x => x.StoryPoint),
            DonePoints = stories.Where(x => x.Status == StoryStatusType.Done).Sum(x => x.StoryPoint),
            BurndownPoints = await _sprintService.GetBurndownDataAsync(selectedSprint.Id),
            VelocityPoints = await _sprintService.GetVelocityDataAsync(projectId),
            AvailableSprints = allSprints.Select(x => new SprintOptionViewModel
            {
                Id = x.Id,
                Name = x.Name
            }).ToList()
        };

        return View(vm);
    }
}