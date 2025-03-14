namespace DEH.Application.Abstractions.Catalog;

public interface ICatalogHttpClientFactory
{
    ICatalogHttpClient GetClient();
}