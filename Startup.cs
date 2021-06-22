using System;
using Azure.Identity;
using Microsoft.Azure.Functions.Extensions.DependencyInjection;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Configuration.AzureAppConfiguration;
using Microsoft.FeatureManagement;

[assembly: FunctionsStartup(typeof(azureappcfg.Startup))]
namespace azureappcfg {
    public class Startup : FunctionsStartup
    {
        public override void ConfigureAppConfiguration(IFunctionsConfigurationBuilder builder)
        {
            var appConfigConnStr = Environment.GetEnvironmentVariable("AppConfigConnStr");
            var appEnv = Environment.GetEnvironmentVariable("Environment");

             
            builder.ConfigurationBuilder
              .AddJsonFile("local.settings.json", optional: true, reloadOnChange: true)
              .AddEnvironmentVariables()
            .AddAzureAppConfiguration(o =>
              o.Connect(appConfigConnStr)
               .Select(KeyFilter.Any, LabelFilter.Null)
               .Select(KeyFilter.Any, "protoduction")
               //.Select(KeyFilter.Any, appEnv )
               .UseFeatureFlags()
               .ConfigureKeyVault(kv => kv.SetCredential(new DefaultAzureCredential())))
              .Build();

            System.Console.Out.WriteLine(appEnv);
            System.Console.Out.WriteLine("here");
        }

        public override void Configure(IFunctionsHostBuilder builder)
        {
            builder.Services.AddAzureAppConfiguration();
            builder.Services.AddFeatureManagement();
        }
    }
}