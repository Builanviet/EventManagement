using Microsoft.AspNetCore.Mvc;

namespace WebClient.Controllers
{
    public class HomepageController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
        public IActionResult Calendar()
        {
            return View();
        }
    }
}
