using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SmartTask.Web.Infrastructure.Interfaces;
using SmartTask.Web.Models.ViewModels.Activity;
using SmartTask.Web.Services.Interfaces;

namespace SmartTask.Web.Controllers;

[Authorize]
public class ActivityController : BaseController
{
    private readonly IActivityLogService _activityLogService;

    public ActivityController(IActivityLogService activityLogService, ICurrentUserService currentUser)
        : base(currentUser)
    {
        _activityLogService = activityLogService;
    }

    public async Task<IActionResult> Index()
    {
        var activities = await _activityLogService.GetUserActivitiesAsync(CurrentUser.UserId, 100);

        var model = new ActivityIndexViewModel
        {
            Activities = activities.Select(x => new ActivityItemViewModel
            {
                Id = x.Id,
                Action = x.Action,
                Description = x.Description,
                ActivityDate = x.ActivityDate,
                TaskItemId = x.TaskItemId,
                TaskTitle = x.TaskItem?.Title
            }).ToList()
        };

        return View(model);
    }
}