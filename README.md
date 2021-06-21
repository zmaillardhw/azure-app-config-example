# Using Azure App Configuration With Azure Functions

### Azure App Config - Configuration

1. Create Azure App Config instance
2. Get Connection String
3. Create Feature

### Azure Functions
1. Create property to store connection string (AppConfigConnStr in this example)
2. Create managed identity (If storing secrets)
3. Permiss to key vault with GET permissions (If storing secrets)

### Code
1. Add dependencies to project

```
dotnet add package Microsoft.Azure.AppConfiguration.AspNetCore -v 4.4.0
dotnet add package Microsoft.FeatureManagement.AspNetCore -v 2.3.0
dotnet add package Microsoft.Azure.Functions.Extensions -v 1.1.0
dotnet add package Microsoft.Extensions.DependencyInjection.Abstractions -v 3.1.16
dotnet add package Azure.Identity -v 1.4.0
```

2. Configure App Configuration

[Code](Startup.cs#L14)

App Settings For Azure Function
 - **AppConfigConnStr** - Connection String From Azure App Configuration
 - **Environment** - Environment for Azure Function instance (dev/test/prod, etc...).  Will correspond to the label names in Azure App Configuration.  If not set, will default to *test*.

Build up configuration as normal - `AddJsonFile` and `AddEnvironmentVariables`

`AddAzureAppConfiguration` binds the Function to the Azure App Configuration service.

```
var appConfigConnStr = Environment.GetEnvironmentVariable("AppConfigConnStr");
var appEnv = Environment.GetEnvironmentVariable("Environment") ?? "test";

builder.ConfigurationBuilder
  .AddJsonFile("local.settings.json", optional: true, reloadOnChange: true)
  .AddEnvironmentVariables()
  .AddAzureAppConfiguration(o =>
    o.Connect(appConfigConnStr)  // Connect to Azure App Config using Connection String
     .Select(KeyFilter.Any, LabelFilter.Null)  // Bring in all config settings not labeled
     .Select(KeyFilter.Any, "protoduction")    // ... Then merge protoduction config settings - will override un-labeled
     .Select(KeyFilter.Any, appEnv ) // ... Then merge config settings defined in `Environment` - will override un-labled and protoduction
     .UseFeatureFlags() // Add Support For Feature Flags
     .ConfigureKeyVault(kv => kv.SetCredential(new DefaultAzureCredential()))) // Add support for Key Vault using Managed Identity
  .Build();
```

3. Configuration Dependency Injection

[Code](Startup.cs#L36)

```
builder.Services.AddAzureAppConfiguration();
builder.Services.AddFeatureManagement();
```

4. Use Flags
[Code](TestTrigger.cs)
