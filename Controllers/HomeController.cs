using Microsoft.AspNetCore.Mvc;

namespace SocialX.Controllers
{
    public class HomeController : Controller
    {
        public IActionResult Index()
        {
            // Redirigir siempre al feed de Posts (página de inicio)
            return RedirectToAction("Index", "Posts");
        }
    }
}
