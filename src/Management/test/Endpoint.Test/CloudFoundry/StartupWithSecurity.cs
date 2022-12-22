// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the Apache 2.0 License.
// See the LICENSE file in the project root for more information.

using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Steeltoe.Management.Endpoint.CloudFoundry;
using Steeltoe.Management.Endpoint.Hypermedia;
using Steeltoe.Management.Endpoint.Info;

namespace Steeltoe.Management.Endpoint.Test.CloudFoundry;

public class StartupWithSecurity
{
    public IConfiguration Configuration { get; set; }

    public StartupWithSecurity(IConfiguration configuration)
    {
        Configuration = configuration;
    }

    public void ConfigureServices(IServiceCollection services)
    {
        services.AddRouting();
        services.AddCloudFoundryActuator(Configuration);
        services.AddHypermediaActuator(Configuration);
        services.AddInfoActuator(Configuration);
    }

    public void Configure(IApplicationBuilder app)
    {
        app.UseCloudFoundrySecurity();
        app.UseRouting();

        app.UseEndpoints(endpoints =>
        {
            endpoints.Map<CloudFoundryEndpoint>();
            endpoints.Map<InfoEndpoint>();
        });
    }
}