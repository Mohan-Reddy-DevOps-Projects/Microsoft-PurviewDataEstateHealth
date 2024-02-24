// -----------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
// -----------------------------------------------------------

namespace Microsoft.Azure.Purview.DataEstateHealth.ProvisioningService.Configurations;

using System;
using System.Linq;
using Microsoft.Azure.Purview.DataEstateHealth.Configurations;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;

/// <summary>
/// Partner configuration after being deserialized.
/// </summary>
/// <typeparam name="TPartnerDetails"></typeparam>
public class PartnerConfig<TPartnerDetails> where TPartnerDetails : IPartnerDetails
{
    /// <summary>
    /// Gets the certificate name.
    /// </summary>
    public string CertificateName { get; }

    /// <summary>
    /// Gets the enabled partners.
    /// </summary>
    public TPartnerDetails[] Partners => this.rawPartners
                  .Where(p => this.EnabledPartners.Any(e => string.Equals(p.Name, e, StringComparison.OrdinalIgnoreCase)))
                  .ToArray();

    /// <summary>
    /// All enabled partners.
    /// </summary>
    private string[] EnabledPartners { get; }

    /// <summary>
    /// All partners details. (enabled/disabled)
    /// </summary>
    private readonly TPartnerDetails[] rawPartners;

    /// <summary>
    /// Initializes a new instance PartnerConfig class.
    /// </summary>
    public PartnerConfig(IOptions<PartnerConfiguration> partnerConfiguration)
    {
        this.CertificateName = partnerConfiguration.Value.CertificateName;
        this.EnabledPartners = partnerConfiguration.Value.EnabledPartners?.Split(new string[] { ";" }, StringSplitOptions.RemoveEmptyEntries)
            ?.Select(n => n.Trim())
            ?.ToArray() ?? Array.Empty<string>();
        PartnerDetailsSerializationSettings serializationSettings = new();
        this.rawPartners = JsonConvert.DeserializeObject<TPartnerDetails[]>(partnerConfiguration.Value.Collection, serializationSettings);
    }
}
