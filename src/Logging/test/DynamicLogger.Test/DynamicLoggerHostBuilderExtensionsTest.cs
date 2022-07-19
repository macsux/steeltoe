// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the Apache 2.0 License.
// See the LICENSE file in the project root for more information.

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Console;
using System.Linq;
using Xunit;

namespace Steeltoe.Extensions.Logging.DynamicLogger.Test;

public partial class DynamicLoggerHostBuilderExtensionsTest
{
    [Fact]
    public void AddDynamicLogging_IHostBuilder_AddsDynamicLogging()
    {
        var hostBuilder = new HostBuilder().AddDynamicLogging();

        var host = hostBuilder.Build();
        var loggerProviders = host.Services.GetServices<ILoggerProvider>();

        Assert.Single(loggerProviders);
        Assert.IsType<DynamicConsoleLoggerProvider>(loggerProviders.First());
    }

    [Fact]
    public void AddDynamicLogging_IHostBuilder_RemovesConsoleLogging()
    {
        var hostBuilder = new HostBuilder()
            .ConfigureLogging(ilb => ilb.AddConsole())
            .AddDynamicLogging();

        var host = hostBuilder.Build();
        var loggerProviders = host.Services.GetServices<ILoggerProvider>();

        Assert.Single(loggerProviders);
        Assert.IsType<DynamicConsoleLoggerProvider>(loggerProviders.First());
    }

    [Fact]
    public void AddDynamicLogging_IHostBuilder_RemovesConsoleLoggingDefaultBuilder()
    {
        var hostBuilder = Host.CreateDefaultBuilder()
            .ConfigureLogging(ilb => ilb.AddConsole())
            .AddDynamicLogging();

        var host = hostBuilder.Build();
        var loggerProviders = host.Services.GetServices<ILoggerProvider>();

        Assert.DoesNotContain(loggerProviders, lp => lp is ConsoleLoggerProvider);
        Assert.Contains(loggerProviders, lp => lp is DynamicConsoleLoggerProvider);
    }
}
