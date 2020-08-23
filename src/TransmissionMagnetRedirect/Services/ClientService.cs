using System;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace TransmissionMagnetRedirect.Services
{
    public class ClientService
    {
        private readonly ILogger<ClientService> logger;
        private readonly IOptions<TransmissionOptions> optionsAccessor;
        private readonly IHttpClientFactory clientFactory;

        public ClientService(ILogger<ClientService> logger, IOptions<TransmissionOptions> optionsAccessor, IHttpClientFactory clientFactory)
        {
            this.logger = logger;
            this.optionsAccessor = optionsAccessor;
            this.clientFactory = clientFactory;
        }

        public async Task<bool> Process(string magnet)
        {
            try
            {
                var options = optionsAccessor.Value;
                
                var client = clientFactory.CreateClient();

                if (!string.IsNullOrWhiteSpace(options.User) && !string.IsNullOrWhiteSpace(options.Password))
                {
                    client.DefaultRequestHeaders.Authorization =
                        new System.Net.Http.Headers.AuthenticationHeaderValue(
                            "Basic",
                            Convert.ToBase64String(System.Text.Encoding.UTF8.GetBytes($"{options.User}:{options.Password}")));
                }

                var url = $"http://{options.Host}:{options.Port}/transmission/rpc";
                logger.LogDebug("retrieving session id from transmission rpc at {url}", url);
                var response = await client.GetAsync(url);

                var sessionId = response.Headers.First(x => x.Key == "X-Transmission-Session-Id").Value.FirstOrDefault();
                logger.LogDebug("sending magnet link to transmission, using session id {sessionId}", sessionId);
                client.DefaultRequestHeaders.Add("X-Transmission-Session-Id", sessionId);
                response = await client.PostAsync(url, new StringContent($"{{\"method\":\"torrent-add\",\"arguments\":{{\"filename\":\"{magnet}\"}}}}"));

                response.EnsureSuccessStatusCode();
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "error processing magnet link");
                return false;
            }

            return true;
        }
    }
}