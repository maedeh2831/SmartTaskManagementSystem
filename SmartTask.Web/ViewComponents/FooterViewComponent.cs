using Microsoft.AspNetCore.Mvc;

namespace SmartTask.Web.ViewComponents
{
    public class FooterViewComponent : ViewComponent
    {
        public IViewComponentResult Invoke()
        {
            return View("Default");
        }
    }
}