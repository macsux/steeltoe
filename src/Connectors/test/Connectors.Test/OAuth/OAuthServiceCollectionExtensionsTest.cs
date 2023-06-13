// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the Apache 2.0 License.
// See the LICENSE file in the project root for more information.

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Steeltoe.Common.TestResources;
using Steeltoe.Configuration.CloudFoundry;
using Steeltoe.Connectors.OAuth;
using Xunit;

namespace Steeltoe.Connectors.Test.OAuth;

public class OAuthServiceCollectionExtensionsTest
{
    [Fact]
    public void AddOAuthServiceOptions_ThrowsIfServiceCollectionNull()
    {
        const IServiceCollection services = null;
        const IConfigurationRoot configurationRoot = null;

        var ex = Assert.Throws<ArgumentNullException>(() => services.AddOAuthServiceOptions(configurationRoot));
        Assert.Contains(nameof(services), ex.Message, StringComparison.Ordinal);

        var ex2 = Assert.Throws<ArgumentNullException>(() => services.AddOAuthServiceOptions(configurationRoot, "foobar"));
        Assert.Contains(nameof(services), ex2.Message, StringComparison.Ordinal);
    }

    [Fact]
    public void AddOAuthServiceOptions_ThrowsIfConfigurationNull()
    {
        IServiceCollection services = new ServiceCollection();
        const IConfigurationRoot configuration = null;

        var ex = Assert.Throws<ArgumentNullException>(() => services.AddOAuthServiceOptions(configuration));
        Assert.Contains(nameof(configuration), ex.Message, StringComparison.Ordinal);

        var ex2 = Assert.Throws<ArgumentNullException>(() => services.AddOAuthServiceOptions(configuration, "foobar"));
        Assert.Contains(nameof(configuration), ex2.Message, StringComparison.Ordinal);
    }

    [Fact]
    public void AddOAuthServiceOptions_ThrowsIfServiceNameNull()
    {
        IServiceCollection services = new ServiceCollection();
        const IConfigurationRoot configurationRoot = null;
        const string serviceName = null;

        var ex = Assert.Throws<ArgumentNullException>(() => services.AddOAuthServiceOptions(configurationRoot, serviceName));
        Assert.Contains(nameof(serviceName), ex.Message, StringComparison.Ordinal);
    }

    [Fact]
    public void AddOAuthServiceOptions_NoVCAPs_AddsOAuthOptions()
    {
        IServiceCollection services = new ServiceCollection();
        IConfigurationRoot configurationRoot = new ConfigurationBuilder().Build();

        services.AddOAuthServiceOptions(configurationRoot);

        var service = services.BuildServiceProvider().GetService<IOptions<OAuthServiceOptions>>();
        Assert.NotNull(service);
    }

    [Fact]
    public void AddOAuthServiceOptions_WithServiceName_NoVCAPs_ThrowsConnectorException()
    {
        IServiceCollection services = new ServiceCollection();
        IConfigurationRoot configurationRoot = new ConfigurationBuilder().Build();

        var ex = Assert.Throws<ConnectorException>(() => services.AddOAuthServiceOptions(configurationRoot, "foobar"));
        Assert.Contains("foobar", ex.Message, StringComparison.Ordinal);
    }

    [Fact]
    public void AddOAuthServiceOptions_MultipleOAuthServices_ThrowsConnectorException()
    {
        const string env2 = @"
                {
                    ""p-identity"": [{
                        ""credentials"": {
                            ""client_id"": ""cb3efc76-bd22-46b3-a5ca-3aaa21c96073"",
                            ""client_secret"": ""92b5ebf0-c67b-4671-98d3-8e316fb11e30"",
                            ""auth_domain"": ""https://sso.login.system.testcloud.com""
                        },
                        ""syslog_drain_url"": null,
                        ""label"": ""p-identity"",
                        ""provider"": null,
                        ""plan"": ""sso"",
                        ""name"": ""mySSO"",
                        ""tags"": []
                    },
                    {
                        ""credentials"": {
                            ""client_id"": ""cb3efc76-bd22-46b3-a5ca-3aaa21c96073"",
                            ""client_secret"": ""92b5ebf0-c67b-4671-98d3-8e316fb11e30"",
                            ""auth_domain"": ""https://sso.login.system.testcloud.com""
                        },
                        ""syslog_drain_url"": null,
                        ""label"": ""p-identity"",
                        ""provider"": null,
                        ""plan"": ""sso"",
                        ""name"": ""mySSO2"",
                        ""tags"": []
                    }]
                }";

        using var appScope = new EnvironmentVariableScope("VCAP_APPLICATION", TestHelpers.VcapApplication);
        using var servicesScope = new EnvironmentVariableScope("VCAP_SERVICES", env2);

        IServiceCollection services = new ServiceCollection();

        var builder = new ConfigurationBuilder();
        builder.AddCloudFoundry();
        IConfigurationRoot configurationRoot = builder.Build();

        var ex = Assert.Throws<ConnectorException>(() => services.AddOAuthServiceOptions(configurationRoot));
        Assert.Contains("Multiple", ex.Message, StringComparison.Ordinal);
    }

    [Fact]
    public void AddOAuthServiceOptions_WithVCAPs_AddsOAuthOptions()
    {
        const string env2 = @"
                {
                    ""p-identity"": [{
                        ""credentials"": {
                            ""client_id"": ""cb3efc76-bd22-46b3-a5ca-3aaa21c96073"",
                            ""client_secret"": ""92b5ebf0-c67b-4671-98d3-8e316fb11e30"",
                            ""auth_domain"": ""https://sso.login.system.testcloud.com""
                        },
                        ""syslog_drain_url"": null,
                        ""label"": ""p-identity"",
                        ""provider"": null,
                        ""plan"": ""sso"",
                        ""name"": ""mySSO"",
                        ""tags"": []
                    }]
                }";

        using var appScope = new EnvironmentVariableScope("VCAP_APPLICATION", TestHelpers.VcapApplication);
        using var servicesScope = new EnvironmentVariableScope("VCAP_SERVICES", env2);

        IServiceCollection services = new ServiceCollection();

        var builder = new ConfigurationBuilder();
        builder.AddCloudFoundry();
        IConfigurationRoot configurationRoot = builder.Build();

        services.AddOAuthServiceOptions(configurationRoot);

        var service = services.BuildServiceProvider().GetService<IOptions<OAuthServiceOptions>>();
        Assert.NotNull(service);

        OAuthServiceOptions opts = service.Value;
        Assert.NotNull(opts);

        Assert.Equal("cb3efc76-bd22-46b3-a5ca-3aaa21c96073", opts.ClientId);
        Assert.Equal("92b5ebf0-c67b-4671-98d3-8e316fb11e30", opts.ClientSecret);
        Assert.Equal($"https://sso.login.system.testcloud.com{OAuthConnectorDefaults.DefaultAccessTokenUri}", opts.AccessTokenUrl);
        Assert.Equal($"https://sso.login.system.testcloud.com{OAuthConnectorDefaults.DefaultJwtTokenKey}", opts.JwtKeyUrl);
        Assert.Equal($"https://sso.login.system.testcloud.com{OAuthConnectorDefaults.DefaultCheckTokenUri}", opts.TokenInfoUrl);
        Assert.Equal($"https://sso.login.system.testcloud.com{OAuthConnectorDefaults.DefaultAuthorizationUri}", opts.UserAuthorizationUrl);
        Assert.Equal($"https://sso.login.system.testcloud.com{OAuthConnectorDefaults.DefaultUserInfoUri}", opts.UserInfoUrl);
        Assert.NotNull(opts.Scope);
        Assert.Equal(0, opts.Scope.Count);
    }
}
