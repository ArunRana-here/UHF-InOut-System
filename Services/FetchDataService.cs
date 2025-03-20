using System;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace InAndOut.Services
{
    public class FetchDataService : BackgroundService
    {
        private readonly ILogger<FetchDataService> _logger;
        private readonly HttpClient _httpClient;

        public FetchDataService(ILogger<FetchDataService> logger)
        {
            _logger = logger;
            _httpClient = new HttpClient();
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("FetchDataService is starting.");

            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    var response = await _httpClient.GetAsync("http://localhost:5290/api/reader/fetch");

                    if (response.IsSuccessStatusCode)
                    {
                        var data = await response.Content.ReadAsStringAsync();
                        _logger.LogInformation($"Fetched Data: {data}");
                        // You can process or broadcast the data here
                    }
                    else
                    {
                        _logger.LogWarning($"Failed to fetch data. Status Code: {response.StatusCode}");
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError($"Error fetching data: {ex.Message}");
                }

                await Task.Delay(5000, stoppingToken);  // Wait for 5 seconds before the next request
            }

            _logger.LogInformation("FetchDataService is stopping.");
        }

        public override void Dispose()
        {
            _httpClient.Dispose();
            base.Dispose();
        }
    }
}
