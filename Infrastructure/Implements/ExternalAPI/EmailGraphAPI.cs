using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Graph;
using Azure.Identity;

namespace Infrastructure.Implements.ExternalAPI;

public static class EmailGraphAPI
{
    public static IServiceCollection AddGraphApiEmail(this IServiceCollection services, ConfigurationManager configuration)
    {
        var emailConfig = configuration.GetSection("GraphApiSettings");

        var clientId = emailConfig["ClientId"];
        var tenantId = emailConfig["TenantId"];
        var clientSecret = emailConfig["ClientSecret"];

        var credential = new ClientSecretCredential(tenantId, clientId, clientSecret);

        var graphClient = new GraphServiceClient(credential);

        services.AddSingleton<GraphServiceClient>(graphClient);

        return services;
    }
}