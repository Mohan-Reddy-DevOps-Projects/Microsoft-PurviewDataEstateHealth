// -----------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
// -----------------------------------------------------------

namespace Microsoft.Azure.Purview.DataEstateHealth.Common;

using Microsoft.DGP.ServiceBasics.BaseModels;
using Microsoft.WindowsAzure.ResourceStack.Common.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;

/// <summary>
/// Defines service versions.
/// </summary>
public class ServiceVersion : ValueObject<ServiceVersion>
{
    /// <summary>
    /// Service Version 1.
    /// </summary>
    public const int V1 = 1;

    /// <summary>
    /// Label for Service Version 1.
    /// </summary>
    public const string LabelV1 = "2023-10-01-preview";

    /// <summary>
    /// Service Version 2.
    /// </summary>
    public const int V2 = 2;

    /// <summary>
    /// Label for Service Version 2.
    /// </summary>
    public const string LabelV2 = "2024-02-01-preview";

    /// <summary>
    /// Dictionary with versions of the service registered.
    /// </summary>
    private static readonly IDictionary<int, ServiceVersion> versionMap = new Dictionary<int, ServiceVersion>()
    {
        { V1, new ServiceVersion { Numeric = V1, Label = LabelV1 } },
        { V2, new ServiceVersion { Numeric = V2, Label = LabelV2 } },
    };

    /// <summary>
    /// Numeric Version of the Service
    /// </summary>
    public int Numeric { get; init; }

    /// <summary>
    /// Label for the Service
    /// </summary>
    public string Label { get; init; }

    /// <summary>
    /// Retrieve ServiceVersion by Numeric Version.
    /// </summary>
    /// <param name="numericVersion"></param>
    /// <returns>The matching ServiceVersion</returns>
    /// <exception cref="ArgumentException">If the version was not found.</exception>
    public static ServiceVersion From(int numericVersion)
    {
        if (!ServiceVersion.versionMap.TryGetValue(numericVersion, out ServiceVersion serviceVersion))
        {
            throw new ArgumentException("Version not found", nameof(numericVersion));
        }

        return serviceVersion;
    }

    /// <summary>
    /// Retrieve ServiceVersion by Textual Label.
    /// </summary>
    /// <param name="versionLabel"></param>
    /// <returns>The matching ServiceVersion</returns>
    /// <exception cref="ArgumentException">If the version was not found.</exception>
    public static ServiceVersion From(string versionLabel)
    {
        ServiceVersion version;

        try
        {
            version = ServiceVersion.versionMap.Values.Single(v => v.Label.EqualsOrdinalInsensitively(versionLabel));
        }
        catch (Exception)
        {
            throw new ArgumentException("Version not found", nameof(versionLabel));
        }

        return version;
    }

    /// <summary>
    /// Returns the textual label corresponding to the numeric service version.
    /// </summary>
    /// <param name="numericVersion">The numeric version of the version.</param>
    /// <returns>The textual label for the service version.</returns>
    /// <exception cref="ArgumentException">If the version was not found.</exception>
    public static string LabelFrom(int numericVersion)
    {
        return ServiceVersion.From(numericVersion).Label;
    }

    /// <summary>
    /// Returns the numeric service version corresponding to the textual label.
    /// </summary>
    /// <param name="versionLabel">The textual label of the service version.</param>
    /// <returns>The numeric version for the service version.</returns>
    /// <exception cref="ArgumentException">If the version was not found.</exception>
    public static int NumericFrom(string versionLabel)
    {
        return ServiceVersion.From(versionLabel).Numeric;
    }

    /// <summary>
    /// Overload for less than operator.
    /// </summary>
    /// <param name="lhs">A <see cref="ServiceVersion"/>.</param>
    /// <param name="rhs">A <see cref="ServiceVersion"/></param>
    /// <returns>True if the first ServiceVersion parameter is less than the second ServiceVersion parameter.</returns>
    public static bool operator <(ServiceVersion lhs, ServiceVersion rhs) => lhs.Numeric < rhs.Numeric;

    /// <summary>
    /// Overload for greater than operator.
    /// </summary>
    /// <param name="lhs">A <see cref="ServiceVersion"/>.</param>
    /// <param name="rhs">A <see cref="ServiceVersion"/></param>
    /// <returns>True if the first ServiceVersion parameter is greater than the second ServiceVersion parameter.</returns>
    public static bool operator >(ServiceVersion lhs, ServiceVersion rhs) => lhs.Numeric > rhs.Numeric;

    /// <summary>
    /// Overload for less than or equal to operator.
    /// </summary>
    /// <param name="lhs">A <see cref="ServiceVersion"/>.</param>
    /// <param name="rhs">A <see cref="ServiceVersion"/></param>
    /// <returns>True if first ServiceVersion parameter is less than or equal to the second ServiceVersion parameter.</returns>
    public static bool operator <=(ServiceVersion lhs, ServiceVersion rhs) => lhs.Numeric <= rhs.Numeric;

    /// <summary>
    /// Overload for greater than or equal to operator.
    /// </summary>
    /// <param name="lhs">A <see cref="ServiceVersion"/>.</param>
    /// <param name="rhs">A <see cref="ServiceVersion"/></param>
    /// <returns>True if first ServiceVersion parameter is greater than or equal to the second ServiceVersion parameter.</returns>
    public static bool operator >=(ServiceVersion lhs, ServiceVersion rhs) => lhs.Numeric >= rhs.Numeric;

    /// <inheritdoc/>
    protected override bool EqualsCore(ServiceVersion obj)
    {
        return obj is ServiceVersion version &&
               this.Numeric == version.Numeric &&
               this.Label.EqualsOrdinalInsensitively(version.Label);
    }

    /// <inheritdoc/>
    protected override int GetHashCodeCore()
    {
        return HashCode.Combine(this.Numeric, this.Label);
    }

    /// <inheritdoc/>
    public override string ToString()
    {
        return this.Numeric.ToString();
    }
}
