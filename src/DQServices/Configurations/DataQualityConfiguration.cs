namespace Microsoft.Purview.DataEstateHealth.DHModels;
using Microsoft.Azure.Purview.DataEstateHealth.Configurations;

public class DataQualityServiceConfiguration : BaseCertificateConfiguration
{
    public const string ConfigSectionName = "dataQualityService";

    public string Endpoint { get; set; }
}
