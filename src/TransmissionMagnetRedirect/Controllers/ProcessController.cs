using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using TransmissionMagnetRedirect.Models;

namespace TransmissionMagnetRedirect.Controllers
{
    public class ProcessController : Controller
    {
        private readonly ILogger<ProcessController> logger;
        private readonly TransmissionOptions options;
        private readonly IHttpClientFactory clientFactory;

        public ProcessController(ILogger<ProcessController> logger, IOptions<TransmissionOptions> optionsAccessor, IHttpClientFactory clientFactory)
        {
            this.logger = logger;
            this.options = optionsAccessor.Value;
            this.clientFactory = clientFactory;
        }

        public async Task<IActionResult> Magnet(string id, bool foreground = false)
        {
            try
            {
                var client = clientFactory.CreateClient();

                if (!string.IsNullOrWhiteSpace(options.User) && !string.IsNullOrWhiteSpace(options.Password))
                {
                    client.DefaultRequestHeaders.Authorization =
                        new System.Net.Http.Headers.AuthenticationHeaderValue(
                            "Basic",
                            Convert.ToBase64String(System.Text.Encoding.UTF8.GetBytes($"{options.User}:{options.Password}")));
                }

                var url = $"http://{options.Host}/transmission/rpc";
                logger.LogDebug("retrieving session id from transmission rpc at {url}", url);
                var response = await client.GetAsync(url);

                var sessionId = response.Headers.First(x => x.Key == "X-Transmission-Session-Id").Value.FirstOrDefault();
                logger.LogDebug("sending magnet link to transmission, using session id {sessionId}", sessionId);
                client.DefaultRequestHeaders.Add("X-Transmission-Session-Id", sessionId);
                response = await client.PostAsync(url, new StringContent($"{{\"method\":\"torrent-add\",\"arguments\":{{\"filename\":\"{id}\"}}}}"));

                response.EnsureSuccessStatusCode();
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "error processing magnet link");
            }

            if (foreground)
            {
                return View(new LinkInfo { Link = id, Options = options });
            }

            return NoContent();
        }

    }
}
