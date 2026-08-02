using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SmartTask.Web.Data.Context;
using SmartTask.Web.Infrastructure.Interfaces;
using SmartTask.Web.Models.Enums;
using SmartTask.Web.Models.ViewModels.TaskBoard;
using SmartTask.Web.Services.Interfaces;

namespace SmartTask.Web.Controllers;

[Authorize]
public class TaskBoardController : BaseController
{
    private readonly ITaskService _taskService;
    private readonly IUserStoryService _userStoryService;
    private readonly ILabelService _labelService;
    private readonly ApplicationDbContext _context;

    public TaskBoardController(
        ITaskService taskService,
        IUserStoryService userStoryService,
        ILabelService labelService,
        ICurrentUserService currentUser,
        ApplicationDbContext context)
        : base(currentUser)
    {
        _taskService = taskService;
        _userStoryService = userStoryService;
        _labelService = labelService;
        _context = context;
    }

    public async Task<IActionResult> Index(
        int projectId,
        int? assigneeId,
        TaskPriorityType? priority,
        TaskType? type,
        int? labelId)
    {
        var project = await _context.Projects.FirstOrDefaultAsync(x => x.Id == projectId);
        if (project == null)
            return NotFound();

        var tasks = await _taskService.GetProjectBoardAsync(projectId, assigneeId, priority, type, labelId);

        var projectMembers = await _context.ProjectMembers
            .Where(x => x.ProjectId == projectId && x.ViewState)
            .Include(x => x.ApplicationUser)
            .ToListAsync();

        var labels = await _labelService.GetByProjectAsync(projectId);

        var vm = new TaskBoardViewModel
        {
            ProjectId = projectId,
            ProjectName = project.Name,
            CanManage = await _userStoryService.CanManageBacklogAsync(projectId, CurrentUser.UserId),
            SelectedAssigneeId = assigneeId,
            SelectedPriority = priority,
            SelectedType = type,
            SelectedLabelId = labelId,
            AvailableAssignees = projectMembers.Select(x => new BoardFilterOptionViewModel
            {
                Id = x.ApplicationUserId,
                Name = x.ApplicationUser.FullName
            }).ToList(),
            AvailableLabels = labels.Select(x => new BoardFilterOptionViewModel
            {
                Id = x.Id,
                Name = x.Name
            }).ToList(),
            Tasks = tasks.Select(x => new TaskBoardItemViewModel
            {
                Id = x.Id,
                Title = x.Title,
                Status = x.Status,
                Priority = x.Priority,
                Type = x.Type,
                Estimate = x.Estimate,
                DueDate = x.DueDate,
                UserStoryId = x.UserStoryId,
                UserStoryTitle = x.UserStory.Title,
                AssigneeNames = x.Assignments.Select(a => a.ApplicationUser.FullName).ToList(),
                Labels = x.TaskLabels.Select(tl => new BoardLabelBadgeViewModel
                {
                    Name = tl.Label.Name,
                    Color = tl.Label.Color
                }).ToList()
            }).ToList()
        };

        return View(vm);
    }
}