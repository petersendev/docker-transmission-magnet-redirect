using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using TransmissionMagnetRedirect.Models;

namespace TransmissionMagnetRedirect.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> logger;
        private readonly TransmissionOptions options;

        public HomeController(ILogger<HomeController> logger, IOptions<TransmissionOptions> optionsAccessor)
        {
            this.logger = logger;
            this.options = optionsAccessor.Value;
        }

        public IActionResult Index()
        {
            return View(options);
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
