// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the Apache 2.0 License.
// See the LICENSE file in the project root for more information.

using System.Data;
using System.Data.Common;
using System.Reflection;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Steeltoe.Common.HealthChecks;

namespace Steeltoe.Connector;

internal static class BaseWebApplicationBuilderExtensions
{
    public static void RegisterConfigurationSource(IConfigurationBuilder configurationBuilder, ConnectionStringPostProcessor postProcessor)
    {
        var source = new ConnectionStringConfigurationSource2();
        source.RegisterPostProcessor(postProcessor);

        configurationBuilder.Add(source);
    }

    public static void RegisterNamedOptions<TOptions>(WebApplicationBuilder builder, string bindingType,
        Func<IServiceProvider, string, IHealthContributor> createHealthContributor)
        where TOptions : ConnectionStringOptions
    {
        string key = ConfigurationPath.Combine(ConnectionStringPostProcessor.ServiceBindingsConfigurationKey, bindingType);
        IConfigurationSection[] childSections = builder.Configuration.GetSection(key).GetChildren().ToArray();

        bool registerDefaultHealthContributor = !ContainsNamedServiceBindings(childSections);
        bool defaultHealthContributorRegistered = false;

        foreach (IConfigurationSection childSection in childSections)
        {
            string bindingName = childSection.Key;

            if (bindingName == ConnectionStringPostProcessor.DefaultBindingName)
            {
                builder.Services.Configure<TOptions>(childSection);

                if (registerDefaultHealthContributor)
                {
                    RegisterHealthContributor(builder.Services, string.Empty, createHealthContributor);
                    defaultHealthContributorRegistered = true;
                }
            }
            else
            {
                builder.Services.Configure<TOptions>(bindingName, childSection);
                RegisterHealthContributor(builder.Services, bindingName, createHealthContributor);
            }
        }

        if (registerDefaultHealthContributor && !defaultHealthContributorRegistered)
        {
            RegisterHealthContributor(builder.Services, string.Empty, createHealthContributor);
        }
    }

    private static bool ContainsNamedServiceBindings(IConfigurationSection[] sections)
    {
        if (sections.Length == 0)
        {
            return false;
        }

        if (sections.Length == 1 && sections[0].Key == ConnectionStringPostProcessor.DefaultBindingName)
        {
            return false;
        }

        return true;
    }

    private static void RegisterHealthContributor(IServiceCollection services, string bindingName,
        Func<IServiceProvider, string, IHealthContributor> createHealthContributor)
    {
        services.AddSingleton(typeof(IHealthContributor), serviceProvider => createHealthContributor(serviceProvider, bindingName));
    }

    public static IHealthContributor CreateRelationalHealthContributor<TOptions>(IServiceProvider serviceProvider, string bindingName, Type connectionType,
        string healthDisplayName, string healthHostNameKey)
        where TOptions : ConnectionStringOptions
    {
        var connection = (IDbConnection)ConnectionFactoryInvoker.CreateConnection<TOptions>(serviceProvider, bindingName, connectionType);
        string serviceName = $"{healthDisplayName}-{bindingName}";
        string hostName = GetHostNameFromConnectionString(connection.ConnectionString, healthHostNameKey);
        var logger = serviceProvider.GetRequiredService<ILogger<RelationalDbHealthContributor>>();

        return new RelationalDbHealthContributor(connection, serviceName, hostName, logger);
    }

    private static string GetHostNameFromConnectionString(string connectionString, string healthHostNameKey)
    {
        var builder = new DbConnectionStringBuilder
        {
            ConnectionString = connectionString
        };

        return (string)builder[healthHostNameKey];
    }

    public static void RegisterConnectionFactory<TOptions>(IServiceCollection services, Type connectionType, bool useSingletonConnection,
        Func<TOptions, string, object> createConnection)
        where TOptions : ConnectionStringOptions
    {
        Type connectionFactoryType = ConnectionFactoryInvoker.MakeConnectionFactoryType<TOptions>(connectionType);

        createConnection = WrapCreateConnection(createConnection, connectionType);

        services.AddSingleton(connectionFactoryType,
            serviceProvider =>
                ConnectionFactoryInvoker.CreateConnectionFactory(serviceProvider, connectionFactoryType, useSingletonConnection, createConnection));
    }

    private static Func<TOptions, string, object> WrapCreateConnection<TOptions>(Func<TOptions, string, object> createConnection, Type connectionType)
        where TOptions : ConnectionStringOptions
    {
        return (options, serviceBindingName) =>
        {
            try
            {
                if (createConnection != null)
                {
                    return createConnection(options, serviceBindingName);
                }

                return Activator.CreateInstance(connectionType, options.ConnectionString);
            }
            catch (TargetInvocationException exception)
            {
                throw exception.InnerException ?? exception;
            }
        };
    }
}