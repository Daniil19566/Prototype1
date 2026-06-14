using Microsoft.AspNetCore.Mvc;

namespace Prototype.Controllers
{
    public class HomeController : Controller
    {
        public IActionResult Index() => View();
    }
}
