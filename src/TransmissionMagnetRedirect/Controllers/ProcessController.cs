using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using TransmissionMagnetRedirect.Services;

namespace TransmissionMagnetRedirect.Controllers
{
    public class ProcessController : Controller
    {
        private readonly ILogger<ProcessController> logger;
        private readonly TransmissionOptions options;
        private readonly ClientService clientService;

        public ProcessController(ILogger<ProcessController> logger, IOptions<TransmissionOptions> optionsAccessor, ClientService clientService)
        {
            this.logger = logger;
            this.options = optionsAccessor.Value;
            this.clientService = clientService;
        }

        public async Task<IActionResult> Magnet(string id, bool foreground = false)
        {        
            await clientService.Process(id);

            if (foreground)
            {
                return View(new LinkInfo { Link = id, Options = options });
            }

            return NoContent();
        }

    }
}
