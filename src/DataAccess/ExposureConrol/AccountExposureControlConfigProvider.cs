// -----------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
// -----------------------------------------------------------

namespace Microsoft.Azure.Purview.DataEstateHealth.DataAccess;

using Microsoft.Azure.Purview.DataEstateHealth.Loggers;
using Microsoft.Azure.Purview.ExposureControlLibrary;
using System.Collections.Generic;
using System.Text.Json;
using System.Text.Json.Serialization;

internal sealed class AccountExposureControlConfigProvider : IAccountExposureControlConfigProvider
{
    private readonly IDataEstateHealthRequestLogger logger;
    private readonly IExposureControlClient exposureControlClient;

    private const string Tag = "ExposureControl";

    private readonly JsonSerializerOptions options = new()
    {
        Converters = {
                new JsonStringEnumConverter(),
            },
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
    };

    /// <summary>
    /// Options bag used to hold all necessary fields when calling to the exposure control service.
    /// </summary>
    private class ExposureControlOptions
    {
        /// <summary>
        /// Options bag used to hold all necessary fields when calling to the exposure control service.
        /// </summary>
        /// <param name="featureName">Required feature name</param>
        /// <param name="defaultValueOnException">Optional default value wwhen an exception occurs</param>
        public ExposureControlOptions(string featureName, bool? defaultValueOnException = null)
        {
            this.FeatureName = featureName;
            this.DefaultValueOnException = defaultValueOnException;
            this.Dimensions = CreateMetricDimensions(featureName, defaultValueOnException);
        }

        /// <summary>
        /// The name of the feature.
        /// </summary>
        public string FeatureName { get; }

        /// <summary>
        /// The metric dimensions for the provided feature.
        /// </summary>
        public Dictionary<string, string> Dimensions { get; }

        /// <summary>
        /// [Optional] When an exception occurs retrieving the EC value, provide a default and swallow the exception.
        /// When value is null, the exception will be thrown.
        /// </summary>
        public bool? DefaultValueOnException { get; }

        /// <summary>
        /// [Optional] The account id.
        /// </summary>
        public string AccountId { get; set; }

        /// <summary>
        /// [Optional] The subscription id.
        /// </summary>
        public string SubscriptionId { get; set; }

        /// <summary>
        /// [Optional] The tenant id.
        /// </summary>
        public string TenantId { get; set; }

        /// <summary>
        /// Creates the default dimensions for the provided feature.
        /// </summary>
        /// <param name="featureName"></param>
        /// <param name="defaultValueOnException">Optional default value wwhen an exception occurs</param>
        /// <returns></returns>
        private static Dictionary<string, string> CreateMetricDimensions(string featureName, bool? defaultValueOnException = null)
        {
            return new Dictionary<string, string>()
                {
                    { ExposureControlDimensions.Action, featureName },
                    { ExposureControlDimensions.Value, defaultValueOnException?.ToString() }
                };
        }
    }

    private class ExposureControlDimensions
    {
        public const string Action = "Action";

        public const string Value = "Value";
    }

    public AccountExposureControlConfigProvider(
        IDataEstateHealthRequestLogger logger,
        IExposureControlClient exposureControlClient)
    {
        this.logger = logger;
        this.exposureControlClient = exposureControlClient;
    }

    /// <inheritdoc/>
    public bool IsDataGovHealthPBIUpgradeEnabled(string accountId, string subscriptionId, string tenantId)
    {
        ExposureControlOptions options = new(Features.DataGovHealthPBIUpgrade.ToString(), false)
        {
            AccountId = accountId,
            SubscriptionId = subscriptionId,
            TenantId = tenantId
        };
        return this.IsFeatureEnabled(options);
    }

    /// <inheritdoc/>
    public bool IsDataGovHealthScheduleQueueEnabled(string accountId, string subscriptionId, string tenantId)
    {
        ExposureControlOptions options = new(Features.DataGovHealthScheduleQueue.ToString(), false)
        {
            AccountId = accountId,
            SubscriptionId = subscriptionId,
            TenantId = tenantId
        };
        return this.IsFeatureEnabled(options);
    }

    /// <inheritdoc/>
    public bool IsDataGovHealthRunSetupSQLEnabled(string accountId, string subscriptionId, string tenantId)
    {
        ExposureControlOptions options = new(Features.DataGovHealthRunSetupSQL.ToString(), false)
        {
            AccountId = accountId,
            SubscriptionId = subscriptionId,
            TenantId = tenantId
        };
        return this.IsFeatureEnabled(options);
    }

    /// <inheritdoc/>
    public bool IsDataQualityProvisioningEnabled(string accountId, string subscriptionId, string tenantId)
    {
        ExposureControlOptions options = new(Features.DataGovDataQualityProvisioning.ToString(), false)
        {
            AccountId = accountId,
            SubscriptionId = subscriptionId,
            TenantId = tenantId
        };
        return this.IsFeatureEnabled(options);
    }

    /// <inheritdoc/>
    public bool IsDGDataHealthEnabled(string accountId, string subscriptionId, string tenantId)
    {
        ExposureControlOptions options = new(Features.DGDataHealth.ToString(), false)
        {
            AccountId = accountId,
            SubscriptionId = subscriptionId,
            TenantId = tenantId
        };
        return this.IsFeatureEnabled(options);
    }

    /// <inheritdoc/>
    public bool IsDataGovHealthTipsEnabled(string accountId, string subscriptionId, string tenantId)
    {
        ExposureControlOptions options = new(Features.DataGovHealthTips.ToString(), false)
        {
            AccountId = accountId,
            SubscriptionId = subscriptionId,
            TenantId = tenantId
        };
        return this.IsFeatureEnabled(options);
    }

    /// <inheritdoc/>
    public bool IsDataGovHealthProvisioningEnabled(string accountId, string subscriptionId, string tenantId)
    {
        ExposureControlOptions options = new(Features.DataGovHealthProvisioning.ToString(), false)
        {
            AccountId = accountId,
            SubscriptionId = subscriptionId,
            TenantId = tenantId
        };
        return this.IsFeatureEnabled(options);
    }

    private bool IsFeatureEnabled(ExposureControlOptions options)
    {
        try
        {
            bool result = this.exposureControlClient.IsFeatureEnabled(options.FeatureName, options.AccountId, options.SubscriptionId, options.TenantId);
            options.Dimensions[ExposureControlDimensions.Value] = result.ToString();

            return result;
        }
        catch (Exception ex)
        {
            this.logger.LogError($"{Tag} | Failed to retrieve exposure control for {options.FeatureName}", ex);

            if (options.DefaultValueOnException != null)
            {
                return (bool)options.DefaultValueOnException;
            }
            else
            {
                throw;
            }
        }
    }

    private string GetAllowList(ExposureControlOptions options, string defaultValue)
    {
        try
        {
            string result = this.exposureControlClient.GetAllowList(options.FeatureName, options.AccountId, options.SubscriptionId, options.TenantId);
            options.Dimensions[ExposureControlDimensions.Value] = result;

            return result;
        }
        catch (Exception ex)
        {
            this.logger.LogError($"{Tag} | Failed to retrieve exposure control for allowlist {options.FeatureName}", ex);

            if (!string.IsNullOrEmpty(defaultValue))
            {
                return defaultValue;
            }
            else
            {
                throw;
            }
            throw;
        }
    }

    private Dictionary<string, string> GetDictionnary(ExposureControlOptions options, Dictionary<string, string> defaultValue = null)
    {
        try
        {
            Dictionary<string, string> result = this.exposureControlClient.GetDictionaryValue(options.FeatureName);
            options.Dimensions[ExposureControlDimensions.Value] = string.Join(",\n ", result.Select(x => $"{x.Key}:{x.Value}"));

            return result;
        }
        catch (Exception ex)
        {
            this.logger.LogError($"{Tag} | Failed to retrieve exposure control for dictionary {options.FeatureName}", ex);

            if (defaultValue is not null)
            {
                return defaultValue;
            }
            else
            {
                throw;
            }
            throw;
        }
    }
}
