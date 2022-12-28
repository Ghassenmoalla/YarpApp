using System.Collections.Generic;
using System.Threading;
using Microsoft.Extensions.Primitives;
using Yarp.ReverseProxy.Configuration;
using Yarp.ReverseProxy.LoadBalancing;
using Yarp.ReverseProxy.Transforms;

namespace ReverseProxy
{
    public class CustomProxyConfigProvider : IProxyConfigProvider
    {
        private CustomMemoryConfig _config;

        public CustomProxyConfigProvider()
        {
            // Load a basic configuration
            // Should be based on your application needs.
            var routeConfig = new RouteConfig
            {
                RouteId = "route1",
              
                ClusterId = "cluster1",
                Match = new RouteMatch
                {
                    Path = "/api/service/{**catch-all}"
                }
            };
            var routeConfig1 = new RouteConfig
            {
                RouteId = "route2",

                ClusterId = "cluster2",
                Match = new RouteMatch
                {
                    Path = "/"
                }
            };
            routeConfig = routeConfig
                .WithTransformPathRemovePrefix(prefix: "/api/service")
                .WithTransformResponseHeader(headerName: "Source", value: "YARP", append: true);
            routeConfig1 = routeConfig1
                    .WithTransformPathRemovePrefix(prefix: "/")
                .WithTransformResponseHeader(headerName: "Source", value: "YARP", append: true);

            var routeConfigs = new[] { routeConfig };
            var routeConfigs1 = new[] { routeConfig1 };
            var clusterConfigs = new[]
            {
                new ClusterConfig
                {
                    ClusterId = "cluster1",

                    LoadBalancingPolicy = LoadBalancingPolicies.RoundRobin,
                    Destinations = new Dictionary<string, DestinationConfig>
                    {
                        { "destination1", new DestinationConfig { Address = "https://ghassenapp1.azurewebsites.net" } },
                        { "destination2", new DestinationConfig { Address = "https://ghassenapp.azurewebsites.net" } }
                    }
                }
                new ClusterConfig
                {
                    ClusterId = "cluster2",

                    LoadBalancingPolicy = LoadBalancingPolicies.RoundRobin,
                    Destinations = new Dictionary<string, DestinationConfig>
                    {
                        { "destination1", new DestinationConfig { Address = "https://learn.microsoft.com/en-us/aspnet/core/host-and-deploy/health-checks?view=aspnetcore-7.0" } },
                       
                    }
                }
            };


            _config = new CustomMemoryConfig(routeConfigs, clusterConfigs);
        }

        public IProxyConfig GetConfig() => _config;

        /// <summary>
        /// By calling this method from the source we can dynamically adjust the proxy configuration.
        /// Since our provider is registered in DI mechanism it can be injected via constructors anywhere.
        /// </summary>
        public void Update(IReadOnlyList<RouteConfig> routes, IReadOnlyList<ClusterConfig> clusters)
        {
            var oldConfig = _config;
            _config = new CustomMemoryConfig(routes, clusters);
            oldConfig.SignalChange();
        }

        private class CustomMemoryConfig : IProxyConfig
        {
            private readonly CancellationTokenSource _cts = new CancellationTokenSource();

            public CustomMemoryConfig(IReadOnlyList<RouteConfig> routes, IReadOnlyList<ClusterConfig> clusters)
            {
                Routes = routes;
                Clusters = clusters;
                ChangeToken = new CancellationChangeToken(_cts.Token);
            }

            public IReadOnlyList<RouteConfig> Routes { get; }

            public IReadOnlyList<ClusterConfig> Clusters { get; }

            public IChangeToken ChangeToken { get; }

            internal void SignalChange()
            {
                _cts.Cancel();
            }
        }
    }
}
