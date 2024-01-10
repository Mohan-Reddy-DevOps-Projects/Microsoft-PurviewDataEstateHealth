// -----------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
// -----------------------------------------------------------

namespace Microsoft.Azure.Purview.DataEstateHealth.Core;

using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.OData.Query;
using Microsoft.Azure.Purview.DataEstateHealth.Common;
using Microsoft.Azure.Purview.DataEstateHealth.DataAccess;
using Microsoft.Azure.Purview.DataEstateHealth.Models;
using Microsoft.DGP.ServiceBasics.Components;
using Microsoft.DGP.ServiceBasics.Services.FieldInjection;
using Microsoft.OData.Edm;
using Microsoft.OData.UriParser;
using Microsoft.Purview.DataGovernance.DataLakeAPI;
using Microsoft.Purview.DataGovernance.DataLakeAPI.Entities;

[Component(typeof(IDataQualityScoreComponent), ServiceVersion.V1)]
internal class DataQualityScoreComponent :
    BaseComponent<IDataQualityContext>,
    IDataQualityScoreComponent
{
#pragma warning disable 649
    [Inject]
    private readonly IDatasetsComponent datasetsComponent;

    [Inject]
    private readonly IODataModelProvider modelProvider;

    [Inject]
    private readonly IRequestHeaderContext requestHeaderContext;

    [Inject]
    private readonly IDataQualityScoreRepository dataQualityScoreRepository;
#pragma warning restore 649

    public DataQualityScoreComponent(IDataQualityContext context, int version)
        : base(context, version)
    {
    }

    [Initialize]
    public void Initialize()
    {
    }

    /// <inheritdoc />
    public async Task<DataQualityScoresModel> Get(CancellationToken cancellationToken)
    {
        if (this.Context.DataAssetId != Guid.Empty)
        {
            ODataQueryOptions<DataQualityScoreEntity> query = this.GetOptions();

            SynapseSqlContext context = await this.datasetsComponent.GetContext(cancellationToken);
            Func<IQueryable<DataQualityScoreEntity>> x = () => query.ApplyTo(this.GetScoreForAssetId(context, this.Context.DataAssetId)) as IQueryable<DataQualityScoreEntity>;
            IQueryable <DataQualityScoreEntity> score = this.datasetsComponent.GetDataset(x) as IQueryable<DataQualityScoreEntity>;

            return score.Select(x => new DataQualityScoresModel()
            {
                    QualityScore = x.QualityScore,
                
            }).Single();
        }

        if (this.Context.DataProductId != Guid.Empty)
        {
            // Fetch data product's data quality score by id
            return await this.dataQualityScoreRepository.GetSingle(
                new DataProductDataQualityScoreKey(
                    this.Context.DataProductId,
                    this.Context.BusinessDomainId,
                    this.Context.AccountId,
                    new Guid(this.requestHeaderContext.CatalogId)),
                cancellationToken);
        }

        // Fetch business domain's data quality score by id
        return await this.dataQualityScoreRepository.GetSingle(
            new DomainDataQualityScoreKey(
                this.Context.BusinessDomainId,
                this.Context.AccountId,
                new Guid(this.requestHeaderContext.CatalogId)),
            cancellationToken);
    }

    private ODataQueryOptions<DataQualityScoreEntity> GetOptions()
    {
        IEdmModel model = this.modelProvider.GetEdmModel(this.requestHeaderContext.ApiVersion);
        IEdmEntitySet entitySet = model.FindDeclaredEntitySet("DataQualityScore");

        ODataPath odataPath = new(new EntitySetSegment(entitySet));
        ODataQueryContext context = new(model, typeof(DataQualityScoreEntity), odataPath);

        return new(context, this.requestHeaderContext.HttpContent.Request);
    }

    private IQueryable GetScoreForAssetId(SynapseSqlContext dbContext, Guid assetId)
    {
        return dbContext.DataQualityScores.Where(score => score.DataAssetId == assetId);
    }
}
