using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace TransmissionMagnetRedirect.Services
{
    public class WatcherService : IHostedService
    {
        private readonly ILogger<WatcherService> logger;
        private readonly IOptions<TransmissionOptions> optionsAccessor;
        private readonly IServiceScopeFactory scopeFactory;
        private readonly FileSystemWatcher watcher;

        public WatcherService(ILogger<WatcherService> logger, IOptions<TransmissionOptions> optionsAccessor, IServiceScopeFactory scopeFactory)
        {
            this.logger = logger;
            this.optionsAccessor = optionsAccessor;
            this.scopeFactory = scopeFactory;

            this.watcher = new FileSystemWatcher
            {
                Path = optionsAccessor.Value.MagnetWatchDir,
                // Watch for changes in LastAccess and LastWrite times, and
                // the renaming of files or directories.
                NotifyFilter = NotifyFilters.LastAccess
                             | NotifyFilters.LastWrite
                             | NotifyFilters.FileName
                             | NotifyFilters.DirectoryName
            };

            watcher.Created += OnChanged;
        }

        public Task StartAsync(CancellationToken cancellationToken)
        {
            ForceAll();
            watcher.EnableRaisingEvents = true;
            return Task.CompletedTask;
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            watcher.EnableRaisingEvents = false;
            return Task.CompletedTask;
        }

        private void OnChanged(object source, FileSystemEventArgs e)
        {
            logger.LogDebug("new file detected: {file} - {type} ", e.Name, e.ChangeType);
            Thread.Sleep(500);
            using (var scope = scopeFactory.CreateScope())
            {
                ProcessFile(e.FullPath, scope.ServiceProvider.GetRequiredService<ClientService>());
            }
        }

        private void ForceAll()
        {
            using (var scope = scopeFactory.CreateScope())
            {
                foreach (var file in Directory.GetFiles(optionsAccessor.Value.MagnetWatchDir))
                {
                    ProcessFile(file, scope.ServiceProvider.GetRequiredService<ClientService>());
                }
            }
        }

        private void ProcessFile(string fullPath, ClientService clientService)
        {
            // ignore torrent files
            if (fullPath.EndsWith(".torrent") || !File.Exists(fullPath))
            {
                return;
            }

            var content = File.ReadAllText(fullPath);
            if (content.StartsWith("magnet:"))
            {
                logger.LogInformation("processing {file}", fullPath);
                try
                {
                    if (clientService.Process(content).Result)
                    {
                        File.Delete(fullPath);
                    }
                }
                catch (Exception ex)
                {
                    logger.LogError(ex, "error processing magnet file {file}", fullPath);
                }
            }
        }
    }
}
