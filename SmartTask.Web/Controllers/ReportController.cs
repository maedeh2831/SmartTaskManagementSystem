using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SmartTask.Web.Data.Context;
using SmartTask.Web.Infrastructure.Interfaces;
using SmartTask.Web.Models.Enums;
using SmartTask.Web.Models.ViewModels.Workspace;

namespace SmartTask.Web.Controllers;

[Authorize]
public class ReportController : BaseController
{
    private readonly ApplicationDbContext _context;

    public ReportController(ApplicationDbContext context, ICurrentUserService currentUser)
        : base(currentUser)
    {
        _context = context;
    }

    public async Task<IActionResult> Index()
    {
        var userId = CurrentUser.UserId;

        var workspaces = await _context.Workspaces
            .Where(x => x.ViewState
                && x.Members.Any(m => m.ApplicationUserId == userId && m.ViewState))
            .Include(x => x.Members)
            .Include(x => x.Projects)
            .OrderByDescending(x => x.CreateDate)
            .Select(x => new WorkspaceListItemViewModel
            {
                Id = x.Id,
                Name = x.Name,
                Description = x.Description,
                Color = x.Color ?? "#4F46E5",
                Logo = x.Logo,
                Visibility = x.Visibility == VisibilityType.Private ? "خصوصی" : "عمومی",
                MembersCount = x.Members.Count(m => m.ViewState),
                ProjectsCount = x.Projects.Count,
                CreateDate = x.CreateDate
            })
            .ToListAsync();

        var vm = new WorkspaceIndexViewModel
        {
            Workspaces = workspaces
        };

        return View(vm);
    }
}