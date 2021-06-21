
using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Configuration.AzureAppConfiguration;
using Microsoft.Extensions.Logging;
using Microsoft.FeatureManagement;

namespace azureappcfg
{
    static class FeatureFlags {
        public const string IsBlueText = "IsBlueText";
    }

    public class TestTrigger
    {
        private readonly IFeatureManagerSnapshot _featureMgr;
        private readonly IConfigurationRefresher _configurationRefresher;
        private readonly string _contentApi;

        public TestTrigger(IConfiguration configuration, IFeatureManagerSnapshot featureMgr, IConfigurationRefresherProvider refresherProvider)
        {
            _featureMgr = featureMgr;
            _configurationRefresher = refresherProvider.Refreshers.First();
            _contentApi = configuration["Content:ContentServiceUrl"];
        }

        [FunctionName("Test")]
        public async Task<IActionResult> Run(
                [HttpTrigger(AuthorizationLevel.Anonymous, methods: "get", Route = null)] HttpRequest req,
                ILogger log)
        {
            try
            {
                await _configurationRefresher.TryRefreshAsync();

                bool useBlueText = await _featureMgr.IsEnabledAsync(FeatureFlags.IsBlueText);

                var style = useBlueText ? "color:#00F" : "color:#F00";

                return new ContentResult { Content = $"<html><body><p style='{style}'>{_contentApi}</p></body></html>", ContentType = "text/html" };
            }
            catch (Exception ex)
            {
                return new BadRequestObjectResult(ex);
            }
        }
    }
}