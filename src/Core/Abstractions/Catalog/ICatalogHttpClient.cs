namespace DEH.Application.Abstractions.Catalog;

using Microsoft.Purview.DataGovernance.Catalog.Model;

public interface ICatalogHttpClient
{
    /// <summary>
    ///     Gets all business domains across all pages.
    /// </summary>
    /// <param name="accountId">The account ID for which to get business domains.</param>
    /// <param name="tenantId">The tenant ID for which to get business domains.</param>
    /// <returns>A list of all business domains.</returns>
    Task<List<Domain>> GetAllBusinessDomains(string accountId, string tenantId);

    /// <summary>
    ///     Gets all objectives (OKRs) for a specific business domain across all pages.
    /// </summary>
    /// <param name="businessDomainId">The ID of the business domain.</param>
    /// <param name="accountId">The account ID for which to get OKRs.</param>
    /// <param name="tenantId">The tenant ID for which to get OKRs.</param>
    /// <returns>A list of all objectives for the business domain.</returns>
    Task<List<ObjectiveWithAdditionalProperties>> GetAllOkrsForBusinessDomain(Guid businessDomainId, string accountId, string tenantId);

    /// <summary>
    ///     Gets all key results for a specific objective (OKR) across all pages.
    /// </summary>
    /// <param name="okrId">The ID of the objective.</param>
    /// <param name="accountId">The account ID for which to get key results.</param>
    /// <param name="tenantId">The tenant ID for which to get key results.</param>
    /// <returns>A list of all key results for the objective.</returns>
    Task<List<KeyResult>> GetAllKeyresultsForOkr(Guid okrId, string accountId, string tenantId);

    /// <summary>
    ///     Gets all Critical Data Elements (CDEs) for a specific business domain across all pages.
    /// </summary>
    /// <param name="businessDomainId">The ID of the business domain.</param>
    /// <param name="accountId">The account ID for which to get CDEs.</param>
    /// <param name="tenantId">The tenant ID for which to get CDEs.</param>
    /// <returns>A list of all critical data elements for the business domain.</returns>
    Task<List<CriticalDataElement>> GetAllCdesForBusinessDomain(Guid businessDomainId, string accountId, string tenantId);
}