using System;
using System.Threading;
using k8s;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Rest;
using Microsoft.Rest.Serialization;
using Microsoft.Rest.TransientFaultHandling;
using Newtonsoft.Json.Serialization;
using Steeltoe.Kubernetes.InformersBase;

namespace Steeltoe.Kubernetes.InformersCore
{
    public static class Extensions
    {
        public static IServiceCollection AddKubernetesClient(this IServiceCollection services, Func<KubernetesClientConfiguration> configProvider)
        {
            var config = configProvider();
            services.AddHttpClient("DefaultName")
                .AddTypedClient<IKubernetes>((httpClient, serviceProvider) =>
                {
                    httpClient.Timeout = Timeout.InfiniteTimeSpan;

                    var kubernetes = new k8s.Kubernetes(config, httpClient);
                    kubernetes.DeserializationSettings.ContractResolver = new ReadOnlyJsonContractResolver() {NamingStrategy = new CamelCaseNamingStrategy()};
                    kubernetes.SerializationSettings.ContractResolver = new ReadOnlyJsonContractResolver() {NamingStrategy = new CamelCaseNamingStrategy()};
                    return kubernetes;
                })
                .AddHttpMessageHandler(() => new KubernetesClientHandler(TimeSpan.FromSeconds(100)))
                .AddHttpMessageHandler(() => new RetryDelegatingHandler { RetryPolicy = new RetryPolicy<HttpStatusCodeErrorDetectionStrategy>(new ExponentialBackoffRetryStrategy()) })
                .AddHttpMessageHandler(KubernetesClientConfiguration.CreateWatchHandler)
                .ConfigurePrimaryHttpMessageHandler(config.CreateDefaultHttpClientHandler);
            
            services.AddSingleton<IKubernetesGenericClient, KubernetesGenericClient>(); // todo: should this be singleton?

            return services;
        }

        public static IServiceCollection AddKubernetesInformers(this IServiceCollection services)
        {
            services.AddTransient(typeof(KubernetesInformer<>));
            services.AddSingleton(typeof(IKubernetesInformer<>), typeof(SharedKubernetesInformer<>));
            return services;
        }
    }
}
