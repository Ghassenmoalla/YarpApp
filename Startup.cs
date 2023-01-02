using Microsoft.ApplicationInsights.Extensibility;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Yarp.ReverseProxy;
using Microsoft.ApplicationInsights.Extensibility;
using CustomInitializer.Telemetry;
namespace ReverseProxy
{
    public class Startup
    {
     
        public IConfiguration Configuration { get; }
        private readonly IConfiguration _configuration;

        public Startup(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public void ConfigureServices(IServiceCollection services)
        {
            services
                .AddReverseProxy()
            .LoadFromConfig(_configuration.GetSection("ReverseProxy"));
            services.AddApplicationInsightsTelemetry();
            services.AddSingleton<ITelemetryInitializer, MyTelemetryInitializer>();
        }

        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseRouting();
            app.UseEndpoints(endpoints =>
            {
                endpoints.MapReverseProxy();
            });
        }
    }
}
