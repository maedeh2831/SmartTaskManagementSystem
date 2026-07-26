using Microsoft.AspNetCore.Mvc;
using SmartTask.Web.Infrastructure.Interfaces;

namespace SmartTask.Web.Controllers
{
    public abstract class BaseController : Controller
    {
        protected readonly ICurrentUserService CurrentUser;

        protected BaseController(ICurrentUserService currentUser)
        {
            CurrentUser = currentUser;
        }
    }
}