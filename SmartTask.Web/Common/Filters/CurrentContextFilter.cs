using Microsoft.AspNetCore.Mvc.Filters;
using SmartTask.Web.Infrastructure.Interfaces;

namespace SmartTask.Web.Common.Filters
{
    public class CurrentContextFilter : IActionFilter
    {
        private readonly ICurrentContextService _currentContextService;

        public CurrentContextFilter(ICurrentContextService currentContextService)
        {
            _currentContextService = currentContextService;
        }

        public void OnActionExecuting(ActionExecutingContext context)
        {
            if (context.ActionArguments.TryGetValue("workspaceId", out var workspaceObj)
                && workspaceObj is int workspaceId)
            {
                _currentContextService.SetCurrentWorkspace(workspaceId);
            }

            if (context.ActionArguments.TryGetValue("projectId", out var projectObj)
                && projectObj is int projectId)
            {
                _currentContextService.SetCurrentProject(projectId);
            }
        }

        public void OnActionExecuted(ActionExecutedContext context)
        {
        }
    }
}