// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the Apache 2.0 License.
// See the LICENSE file in the project root for more information.

using System.Diagnostics.Metrics;
using System.Reflection;

namespace Steeltoe.Management.OpenTelemetry.Metrics;

public static class OpenTelemetryMetrics
{
    public static readonly AssemblyName AssemblyName = typeof(OpenTelemetryMetrics).Assembly.GetName();

    public static readonly string InstrumentationVersion = AssemblyName.Version.ToString();

    public static Meter Meter => new(InstrumentationName, InstrumentationVersion);

    public static string InstrumentationName { get; set; } = AssemblyName.Name;
}