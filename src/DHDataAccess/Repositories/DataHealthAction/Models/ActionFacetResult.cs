namespace Microsoft.Purview.DataEstateHealth.DHDataAccess.Repositories.DataHealthAction.Models;

using Microsoft.Purview.DataEstateHealth.DHDataAccess.Attributes;
using Microsoft.Purview.DataEstateHealth.DHDataAccess.Repositories.Shared;
using Microsoft.Purview.DataEstateHealth.DHModels.Services.DataHealthAction;

public class ActionFacets
{
    [Facet(DataHealthActionWrapper.keyFindingType)]
    public FacetEntity? FindingType { get; set; }

    [Facet(DataHealthActionWrapper.keyFindingSubType)]
    public FacetEntity? FindingSubType { get; set; }

    [Facet(DataHealthActionWrapper.keyFindingName)]
    public FacetEntity? FindingName { get; set; }

    [Facet(DataHealthActionWrapper.keyAssignedTo)]
    public FacetEntity? AssignedTo { get; set; }

    [Facet(DataHealthActionWrapper.keyDomainId)]
    public FacetEntity? DomainId { get; set; }
}