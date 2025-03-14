namespace DEH.Infrastructure.Catalog;

using Application.Abstractions.Catalog;
using Common;
using Microsoft.Azure.Purview.DataEstateHealth.Loggers;
using Microsoft.Purview.DataGovernance.Catalog.Model;
using Microsoft.Rest;
using Newtonsoft.Json;
using System.Web;

public class CatalogHttpClient(HttpClient httpClient, Uri baseUri, IDataEstateHealthRequestLogger logger)
    : ServiceClient<CatalogHttpClient>, ICatalogHttpClient
{
    private readonly Uri _baseUri = baseUri;

    private readonly IDataEstateHealthRequestLogger _logger = logger;

    private readonly HttpClient _client = httpClient;

    public async Task<List<Domain>> GetAllBusinessDomains(string accountId, string tenantId)
    {
        try
        {
            var allDomains = new List<Domain>();
            string? skipToken = null;
            bool hasMoreData = true;
            int pageCount = 0;

            while (hasMoreData)
            {
                string requestId = Guid.NewGuid()
                    .ToString();
                string url = $"{this._baseUri}businessdomains";

                if (!String.IsNullOrEmpty(skipToken))
                {
                    url += $"?$skipToken={skipToken}";
                }

                this.SetRequestHeaders(requestId, accountId, tenantId);

                this._logger.LogInformation($"Getting business domains for account {accountId}, page {++pageCount}, url: {url}, requestId: {requestId}");

                var response = await this._client.GetAsync(url)
                    .ConfigureAwait(false);
                response.EnsureSuccessStatusCode();

                string content = await response.Content.ReadAsStringAsync();

                var pagedResponse = JsonConvert.DeserializeObject<PagedResults<Domain>>(content);

                if (pagedResponse?.Value != null)
                {
                    var pageDomains = pagedResponse.Value.ToList();
                    allDomains.AddRange(pageDomains);

                    if (String.IsNullOrEmpty(pagedResponse.NextLink))
                    {
                        hasMoreData = false;
                    }
                    else
                    {
                        var nextLinkUri = new Uri(pagedResponse.NextLink);
                        var queryParams = HttpUtility.ParseQueryString(nextLinkUri.Query);
                        skipToken = queryParams["$skipToken"];

                        if (String.IsNullOrEmpty(skipToken))
                        {
                            hasMoreData = false;
                        }
                    }
                }
                else
                {
                    hasMoreData = false;
                }

                if (pageCount < 100)
                {
                    continue;
                }

                this._logger.LogWarning($"Reached maximum page limit when retrieving business domains for account {accountId}. Some data may be missing.");
                hasMoreData = false;
            }

            this._logger.LogInformation($"Retrieved a total of {allDomains.Count} business domains for account {accountId}");
            return allDomains;
        }
        catch (Exception ex)
        {
            this._logger.LogError($"Error retrieving business domains for account {accountId}: {ex.Message}", ex);
            throw;
        }
    }

    public async Task<List<ObjectiveWithAdditionalProperties>> GetAllOkrsForBusinessDomain(Guid businessDomainId, string accountId, string tenantId)
    {
        try
        {
            var allObjectives = new List<ObjectiveWithAdditionalProperties>();
            int skip = 0;
            const int pageSize = 500;
            bool hasMoreData = true;

            while (hasMoreData)
            {
                string requestId = Guid.NewGuid()
                    .ToString();
                string url = $"{this._baseUri}objectives?domainId={businessDomainId}&skip={skip}&top={pageSize}";

                this.SetRequestHeaders(requestId, accountId, tenantId);

                this._logger.LogInformation($"Getting OKRs for business domain {businessDomainId}, account {accountId}, page {skip / pageSize + 1}, url: {url}, requestId: {requestId}");

                var response = await this._client.GetAsync(url)
                    .ConfigureAwait(false);
                response.EnsureSuccessStatusCode();

                string content = await response.Content.ReadAsStringAsync();

                var pagedResponse = JsonConvert.DeserializeObject<PagedResults<ObjectiveWithAdditionalProperties>>(content);

                if (pagedResponse?.Value != null)
                {
                    var pageObjectives = pagedResponse.Value.ToList();
                    allObjectives.AddRange(pageObjectives);

                    if (pageObjectives.Count < pageSize ||
                        (pagedResponse.Count.HasValue && allObjectives.Count >= pagedResponse.Count.Value))
                    {
                        hasMoreData = false;
                    }
                }
                else
                {
                    hasMoreData = false;
                }

                skip += pageSize;

                if (skip <= 20000)
                {
                    continue;
                }

                this._logger.LogWarning($"Reached maximum page limit when retrieving OKRs for domain {businessDomainId}, account {accountId}. Some data may be missing.");
                hasMoreData = false;
            }

            this._logger.LogInformation($"Retrieved a total of {allObjectives.Count} OKRs for business domain {businessDomainId}, account {accountId}");
            return allObjectives;
        }
        catch (Exception ex)
        {
            this._logger.LogError($"Error retrieving OKRs for business domain {businessDomainId}, account {accountId}: {ex.Message}", ex);
            throw;
        }
    }

    public async Task<List<KeyResult>> GetAllKeyresultsForOkr(Guid okrId, string accountId, string tenantId)
    {
        try
        {
            var allKeyResults = new List<KeyResult>();
            int skip = 0;
            const int pageSize = 500;
            bool hasMoreData = true;

            while (hasMoreData)
            {
                string requestId = Guid.NewGuid()
                    .ToString();
                string url = $"{this._baseUri}objectives/{okrId}/keyResults?skip={skip}&top={pageSize}";

                this.SetRequestHeaders(requestId, accountId, tenantId);

                this._logger.LogInformation($"Getting key results for OKR {okrId}, account {accountId}, page {skip / pageSize + 1}, url: {url}, requestId: {requestId}");

                var response = await this._client.GetAsync(url)
                    .ConfigureAwait(false);
                response.EnsureSuccessStatusCode();

                string content = await response.Content.ReadAsStringAsync();

                var pagedResponse = JsonConvert.DeserializeObject<PagedResults<KeyResult>>(content);

                if (pagedResponse?.Value != null)
                {
                    var pageKeyResults = pagedResponse.Value.ToList();
                    allKeyResults.AddRange(pageKeyResults);

                    if (pageKeyResults.Count < pageSize ||
                        (pagedResponse.Count.HasValue && allKeyResults.Count >= pagedResponse.Count.Value))
                    {
                        hasMoreData = false;
                    }
                }
                else
                {
                    hasMoreData = false;
                }

                skip += pageSize;

                if (skip <= 10000)
                {
                    continue;
                }

                this._logger.LogWarning($"Reached maximum page limit when retrieving key results for OKR {okrId}, account {accountId}. Some data may be missing.");
                hasMoreData = false;
            }

            this._logger.LogInformation($"Retrieved a total of {allKeyResults.Count} key results for OKR {okrId}, account {accountId}");
            return allKeyResults;
        }
        catch (Exception ex)
        {
            this._logger.LogError($"Error retrieving key results for OKR {okrId}, account {accountId}: {ex.Message}", ex);
            throw;
        }
    }

    public async Task<List<CriticalDataElement>> GetAllCdesForBusinessDomain(Guid businessDomainId, string accountId, string tenantId)
    {
        try
        {
            var allCdes = new List<CriticalDataElement>();
            int skip = 0;
            const int pageSize = 500;
            bool hasMoreData = true;

            while (hasMoreData)
            {
                string requestId = Guid.NewGuid()
                    .ToString();
                string url = $"{this._baseUri}criticaldataelements?domainid={businessDomainId}&skip={skip}&top={pageSize}";

                this.SetRequestHeaders(requestId, accountId, tenantId);

                this._logger.LogInformation($"Getting critical data elements for business domain {businessDomainId}, account {accountId}, page {skip / pageSize + 1}, url: {url}, requestId: {requestId}");

                var response = await this._client.GetAsync(url)
                    .ConfigureAwait(false);
                response.EnsureSuccessStatusCode();

                string content = await response.Content.ReadAsStringAsync();

                var pagedResponse = JsonConvert.DeserializeObject<PagedResults<CriticalDataElement>>(content);

                if (pagedResponse?.Value != null)
                {
                    var pageCdes = pagedResponse.Value.ToList();
                    allCdes.AddRange(pageCdes);

                    if (pageCdes.Count < pageSize ||
                        (pagedResponse.Count.HasValue && allCdes.Count >= pagedResponse.Count.Value))
                    {
                        hasMoreData = false;
                    }
                }
                else
                {
                    hasMoreData = false;
                }

                skip += pageSize;

                if (skip <= 10000)
                {
                    continue;
                }

                this._logger.LogWarning($"Reached maximum page limit when retrieving CDEs for domain {businessDomainId}, account {accountId}. Some data may be missing.");
                hasMoreData = false;
            }

            this._logger.LogInformation($"Retrieved a total of {allCdes.Count} critical data elements for business domain {businessDomainId}, account {accountId}");
            return allCdes;
        }
        catch (Exception ex)
        {
            this._logger.LogError($"Error retrieving critical data elements for business domain {businessDomainId}, account {accountId}: {ex.Message}", ex);
            throw;
        }
    }

    private void SetRequestHeaders(string requestId, string accountId, string tenantId)
    {
        this._client.DefaultRequestHeaders.Remove("x-ms-client-request-id");
        this._client.DefaultRequestHeaders.Remove("x-ms-client-tenant-id");
        this._client.DefaultRequestHeaders.Remove("x-ms-account-id");

        this._client.DefaultRequestHeaders.Add("x-ms-client-request-id", requestId);
        this._client.DefaultRequestHeaders.Add("x-ms-client-tenant-id", tenantId);
        this._client.DefaultRequestHeaders.Add("x-ms-account-id", accountId);
    }
}