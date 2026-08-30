using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SmartTask.Web.Infrastructure.Interfaces;

namespace SmartTask.Web.Controllers;

[Authorize]
public class SimulationMVCController : BaseController
{
    public SimulationMVCController(ICurrentUserService currentUser)
        : base(currentUser) { }

    public IActionResult Index() => View();
}
