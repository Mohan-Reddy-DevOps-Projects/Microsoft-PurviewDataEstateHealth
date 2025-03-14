namespace DEH.Infrastructure.Configurations;

using Microsoft.Azure.Purview.DataEstateHealth.Configurations;

public class CatalogAdapterConfiguration : BaseCertificateConfiguration
{
    public const string ConfigSectionName = "catalogApiService";

    public required string Endpoint { get; set; }
}