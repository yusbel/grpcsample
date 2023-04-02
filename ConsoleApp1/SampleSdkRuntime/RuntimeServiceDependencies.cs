﻿using Microsoft.Azure.Cosmos;
using Microsoft.Extensions.Azure;
using Sample.Sdk.Core.Azure.ActiveDirectoryLibs.AppRegistration;
using Sample.Sdk.Core.Azure.ActiveDirectoryLibs.ServiceAccount;
using Sample.Sdk.Core.Azure.BlobLibs;
using Sample.Sdk.Core.Azure.Factory;
using Sample.Sdk.Core.Azure.KeyVaultLibs;
using Sample.Sdk.Data.Constants;
using Sample.Sdk.Interface;
using Sample.Sdk.Interface.Azure.ActiveDirectoryLibs;
using Sample.Sdk.Interface.Azure.BlobLibs;
using Sample.Sdk.Interface.Azure.Factory;
using Sample.Sdk.Interface.Azure.KeyVaultLibs;
using SampleSdkRuntime.HostedServices;
using SampleSdkRuntime.HostedServices.Interfaces;
using SampleSdkRuntime.Providers;
using SampleSdkRuntime.Providers.Data;
using SampleSdkRuntime.Providers.Interfaces;
using SampleSdkRuntime.Providers.Registration;
using SampleSdkRuntime.Providers.RuntimeObservers;
using static Sample.Sdk.Data.Enums.Enums;

namespace SampleSdkRuntime
{
    public static class RuntimeServiceDependencies
    {
        public static IServiceCollection AddRuntimeServices(this IServiceCollection services, IConfiguration config, IServiceContext serviceContext)
        {
            services.AddRuntimeServiceAzureClients(config, serviceContext);
            services.AddRuntimeServiceSettings(config);
            
            return services;
        }
        private static IServiceCollection AddRuntimeServiceSettings(this IServiceCollection services, IConfiguration configuration) 
        {
            services.AddTransient<IServiceCredentialProvider, ServiceCredentialProvider>();
            services.AddTransient<IServiceRegistrationProvider, ServiceRegistrationProvider>();
            services.AddTransient<IApplicationRegistration, ApplicationRegistrationProvider>();
            services.AddTransient<IKeyVaultPolicyProvider, KeyVaultPolicyProvider>();
            services.AddTransient<IGraphServiceClientFactory, GraphServiceClientFactory>();
            services.AddTransient<IArmClientFactory, ArmClientFactory>();
            services.AddTransient<IServicePrincipalProvider, ServicePrincipalProvider>();
            services.AddTransient<IRuntimeVerificationProvider, RuntimeVerificationProvider>();
            services.AddTransient<IRuntimeRepairProvider, RuntimeRepairProvider>();
            services.AddTransient<IAsyncObserver<RuntimeVerificationEvent, VerificationResult>, ServicePrincipalVerificationObserver>();
            services.AddTransient<IAsyncObserver<RuntimeVerificationEvent, VerificationResult>, AppRegVerifictionObserver>(); 
            services.AddTransient<IAsyncObserver<VerificationResult, VerificationRepairResult>, ServicePrincipalRepairObserver>();
            services.AddTransient<IAsyncObserver<VerificationResult, VerificationRepairResult>, AppRegRepairObserver>();
            services.AddTransient<IKeyVaultProvider, KeyVaultProvider>();
            services.AddTransient<IRuntimeVerificationService, RuntimeVerificationService>();
            services.AddTransient<IBlobProvider, BlobProvider>();
            services.AddSingleton(sp => 
            {
                var tokenClientFactory = sp.GetRequiredService<IClientOAuthTokenProviderFactory>();
                var clientSecretCredential = tokenClientFactory.GetClientSecretCredential();
                var cosmosDbClient = new CosmosClient("https://microservice-service-runtime.documents.azure.com:443", clientSecretCredential);
                
                return cosmosDbClient;
            });

            return services;
        }

        
        private static IServiceCollection AddRuntimeServiceAzureClients(this IServiceCollection services, IConfiguration config, IServiceContext serviceContext)
        {
            services.AddAzureClients(clientBuilder =>
            {
                var blobConnStrKey = $"{serviceContext.GetServiceInstanceName()}:{serviceContext.GetServiceBlobConnStrKey()}";
                clientBuilder.AddBlobServiceClient(config.GetSection(blobConnStrKey))
                                    .WithName(HostTypeOptions.ServiceInstance.ToString());
                clientBuilder.AddBlobServiceClient(config.GetSection(serviceContext.GetServiceRuntimeBlobConnStrKey()))
                                    .WithName(HostTypeOptions.Runtime.ToString());

                clientBuilder.AddConfigurationClient(Environment.GetEnvironmentVariable(ConfigVarConst.APP_CONFIG_CONN_STR));
                
                clientBuilder.UseCredential((services) =>
                {
                    var clientCredentialFactory = services.GetRequiredService<IClientOAuthTokenProviderFactory>();
                    clientCredentialFactory.TryGetOrCreateClientSecretCredentialWithDefaultIdentity(out var clientSecretCredential);
                    return clientSecretCredential;
                });
            });
            return services;
        }
    }
}
