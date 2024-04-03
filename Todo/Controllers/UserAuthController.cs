using Microsoft.AspNetCore.Mvc;

namespace Todo.Controllers
{
    public class UserAuthController : Controller
    {
        public IActionResult Login()
        {
            return View();
        }
        public IActionResult Register()
        {
            return View();
        }
    }
}
