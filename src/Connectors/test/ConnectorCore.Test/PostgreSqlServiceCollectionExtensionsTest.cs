// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the Apache 2.0 License.
// See the LICENSE file in the project root for more information.

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Steeltoe.Common.HealthChecks;
using Steeltoe.Extensions.Configuration.CloudFoundry;
using System;
using Xunit;

namespace Steeltoe.Connector.PostgreSql.Test;

/// <summary>
/// Tests for the extension method that adds just the health check.
/// </summary>
public class PostgreSqlServiceCollectionExtensionsTest
{
    public PostgreSqlServiceCollectionExtensionsTest()
    {
        Environment.SetEnvironmentVariable("VCAP_APPLICATION", null);
        Environment.SetEnvironmentVariable("VCAP_SERVICES", null);
    }

    [Fact]
    public void AddPostgreSqlHealthContributor_ThrowsIfServiceCollectionNull()
    {
        const IServiceCollection services = null;
        const IConfigurationRoot config = null;

        var ex = Assert.Throws<ArgumentNullException>(() => services.AddPostgresHealthContributor(config));
        Assert.Contains(nameof(services), ex.Message);

        var ex2 = Assert.Throws<ArgumentNullException>(() => services.AddPostgresHealthContributor(config, "foobar"));
        Assert.Contains(nameof(services), ex2.Message);
    }

    [Fact]
    public void AddPostgreSqlHealthContributor_ThrowsIfConfigurationNull()
    {
        IServiceCollection services = new ServiceCollection();
        const IConfigurationRoot config = null;

        var ex = Assert.Throws<ArgumentNullException>(() => services.AddPostgresHealthContributor(config));
        Assert.Contains(nameof(config), ex.Message);

        var ex2 = Assert.Throws<ArgumentNullException>(() => services.AddPostgresHealthContributor(config, "foobar"));
        Assert.Contains(nameof(config), ex2.Message);
    }

    [Fact]
    public void AddPostgreSqlHealthContributor_ThrowsIfServiceNameNull()
    {
        IServiceCollection services = new ServiceCollection();
        const IConfigurationRoot config = null;
        const string serviceName = null;

        var ex = Assert.Throws<ArgumentNullException>(() => services.AddPostgresHealthContributor(config, serviceName));
        Assert.Contains(nameof(serviceName), ex.Message);
    }

    [Fact]
    public void AddPostgreSqlHealthContributor_NoVCAPs_AddsIHealthContributor()
    {
        IServiceCollection services = new ServiceCollection();
        var config = new ConfigurationBuilder().Build();

        services.AddPostgresHealthContributor(config);

        var service = services.BuildServiceProvider().GetService<IHealthContributor>();
        Assert.NotNull(service);
    }

    [Fact]
    public void AddPostgreSqlHealthContributor_WithServiceName_NoVCAPs_ThrowsConnectorException()
    {
        IServiceCollection services = new ServiceCollection();
        var config = new ConfigurationBuilder().Build();

        var ex = Assert.Throws<ConnectorException>(() => services.AddPostgresHealthContributor(config, "foobar"));
        Assert.Contains("foobar", ex.Message);
    }

    [Fact]
    public void AddPostgreSqlHealthContributor_AddsRelationalHealthContributor()
    {
        IServiceCollection services = new ServiceCollection();
        var builder = new ConfigurationBuilder();
        builder.AddCloudFoundry();
        var config = builder.Build();

        services.AddPostgresHealthContributor(config);
        var healthContributor = services.BuildServiceProvider().GetService<IHealthContributor>() as RelationalDbHealthContributor;

        Assert.NotNull(healthContributor);
    }
}
