using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace Aspc_QRCode.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        public HomeController(ILogger<HomeController> logger)
        {
            _logger = logger;
        }

        public IActionResult Index()
        {
            return RedirectToAction("Index", "QRCode");
        }

        public IActionResult Privacy()
        {
            return View();
        }
    }
}
