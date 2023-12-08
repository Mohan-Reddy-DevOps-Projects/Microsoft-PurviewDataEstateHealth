// -----------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
// -----------------------------------------------------------

namespace Microsoft.Azure.Purview.DataEstateHealth.DataAccess;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Azure.Purview.ExposureControlLibrary;
using Microsoft.Azure.Purview.DataEstateHealth.Configurations;
using Microsoft.Azure.Purview.DataEstateHealth.Common;
using Microsoft.Azure.Purview.DataEstateHealth.Loggers;
using Microsoft.Extensions.Options;

internal class ExposureControlClient : IExposureControlClient
{
    private const string Tag = "ExposureControl";
    private IExposureControl exposureControl;
    private readonly IDataEstateHealthRequestLogger logger;
    private readonly ICertificateLoaderService certificateManager;
    private readonly ExposureControlConfiguration config;

    public ExposureControlClient(
        IDataEstateHealthRequestLogger logger,
        ICertificateLoaderService certificateManager,
        IOptions<ExposureControlConfiguration> config)
    {
        this.logger = logger;
        this.certificateManager = certificateManager;
        this.config = config.Value;
    }

    /// <inheritdoc/>
    public bool IsFeatureEnabled(string feature, string accountId, string subscriptionId, string tenantId)
    {
        bool result = exposureControl?.IsFeatureEnabled(feature, accountId, subscriptionId, tenantId) ?? false;
        this.logger.LogTrace($"{Tag} | Features: {feature}, IsEnabled: {result}");

        return result;
    }

    /// <inheritdoc/>
    public string GetAllowList(string allowList, string accountId, string subscriptionId, string tenantId)
    {
        string value = this.exposureControl?.GetAllowListValue(allowList, accountId, subscriptionId, tenantId) ?? string.Empty;
        this.logger.LogTrace($"{Tag} | AllowList: {allowList}, IsEnabled: {value}");

        return value;
    }

    /// <inheritdoc/>
    public Dictionary<string, string> GetDictionaryValue(string dictionaryName)
    {
        Dictionary<string, string> values = this.exposureControl?.GetDictionaryValue(dictionaryName) ?? new Dictionary<string, string>();
        this.logger.LogTrace($"{Tag} | Dictionary: {dictionaryName}, IsEnabled: {string.Join(",\n ", values.Select(x => $"{x.Key}:{x.Value}"))}");

        return values;
    }

    /// <inheritdoc/>
    public IExposureControl GetExposureControl()
    {
        return this.exposureControl;
    }

    /// <inheritdoc/>
    public async Task Initialize()
    {
        if (!this.config.Enabled)
        {
            this.logger.LogInformation($"{Tag}|Exposure Control is not enabled");
            return;
        }

        var exposureControlEnvironment = ExposureControlEnvironment.FromName(this.config.EnvironmentName);
        if (exposureControlEnvironment == null)
        {
            this.logger.LogWarning($"{Tag}|Environment not found. Exposure Control will not be used");
            return;
        }

        bool useEstsr = this.config.UseEstsr;
        this.logger.LogInformation($"{Tag}|Initializing Exposure Control with useESTSR: {useEstsr}.");

        ExposureControlClientCertificateCredential tokenProvider;

        try
        {
            tokenProvider = new(
            this.config.ClientId,
            exposureControlEnvironment,
            () => this.certificateManager.LoadAsync(this.config.CertificateName, CancellationToken.None).GetAwaiter().GetResult(),
            useEstsr);

            this.exposureControl = new ExposureControl(
                exposureControlEnvironment,
                tokenProvider,
                logger: new ExposureControlLogger(this.logger, this.config.LoggingLevel));
        }
        catch(Exception ex)
        {
            this.logger.LogCritical($"{Tag}|Exposure control failed to initialize.", ex);
            throw;
        }
        

        try
        {
            await this.exposureControl.SetupAsync(this.config.CacheRefreshIntervalInMinutes);
            this.logger.LogInformation($"{Tag}|Exposure control successfully initialized");
        }
        catch (Exception ex)
        {
            this.logger.LogCritical($"{Tag}|Exposure control failed to initialize.", ex);
            throw;
        }
    }
}
