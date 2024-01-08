// -----------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
// -----------------------------------------------------------

namespace Microsoft.Azure.Purview.DataEstateHealth.DataAccess;

using Microsoft.OData.Edm;
using Microsoft.OData.ModelBuilder;
using Microsoft.Purview.DataGovernance.DataLakeAPI.Entities;

/// <summary>
/// The OData model provider. Used to retrieve the data model
/// </summary>
public interface IODataModelProvider
{
    /// <summary>
    /// Get the Edm model
    /// </summary>
    /// <param name="apiVersion"></param>
    /// <returns></returns>
    IEdmModel GetEdmModel(string apiVersion);
}

/// <summary>
/// The OData model provider. Used to retrieve the data model
/// </summary>
public class ODataModelProvider : IODataModelProvider
{
    /// <inheritdoc/>
    public IEdmModel GetEdmModel(string apiVersion) => BuildModel();

    /// <summary>
    /// Build Edm Model
    /// </summary>
    public static IEdmModel BuildModel()
    {
        ODataConventionModelBuilder modelBuilder = new ODataConventionModelBuilder();
        modelBuilder.EntitySet<BusinessDomainEntity>("BusinessDomain");
        return modelBuilder.GetEdmModel();
    }
}
