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
            ODataQueryOptions<AssetQualityScoreEntity> assetQuery = this.GetAssetOptions();

            SynapseSqlContext assetContext = await this.datasetsComponent.GetContext(cancellationToken);
            Func<IQueryable<AssetQualityScoreEntity>> x = () => assetQuery.ApplyTo(this.GetScoreForAssetId(assetContext, this.Context.DataAssetId)) as IQueryable<AssetQualityScoreEntity>;
            IQueryable <AssetQualityScoreEntity> assetScore = this.datasetsComponent.GetDataset(x) as IQueryable<AssetQualityScoreEntity>;

            return assetScore.Select(x => new DataQualityScoresModel()
            {
                    QualityScore = x.QualityScore,
                    LastRefreshedAt = x.LastRefreshedAt,
                    BusinessDomainId = x.BusinessDomainId,
                    DataAssetId = x.DataAssetId,
                    DataProductId = x.DataProductId,
                
            }).Single();
        }

        if (this.Context.DataProductId != Guid.Empty)
        {
            ODataQueryOptions<DataProductQualityScoreEntity> productQuery = this.GetProductOptions();

            SynapseSqlContext productContext = await this.datasetsComponent.GetContext(cancellationToken);
            Func<IQueryable<DataProductQualityScoreEntity>> y = () => productQuery.ApplyTo(this.GetScoreForDataProductId(productContext, this.Context.DataProductId)) as IQueryable<DataProductQualityScoreEntity>;
            IQueryable<DataProductQualityScoreEntity> productScore = this.datasetsComponent.GetDataset(y) as IQueryable<DataProductQualityScoreEntity>;

            return productScore.Select(y => new DataQualityScoresModel()
            {
                QualityScore = y.QualityScore,
                LastRefreshedAt = y.LastRefreshedAt,
                BusinessDomainId = y.BusinessDomainId,
                DataProductId = y.DataProductId
            }).Single();
        }

        ODataQueryOptions<BusinessDomainQualityScoreEntity> query = this.GetBusinessDomainOptions();

        SynapseSqlContext context = await this.datasetsComponent.GetContext(cancellationToken);
        Func<IQueryable<BusinessDomainQualityScoreEntity>> z = () => query.ApplyTo(this.GetScoreForBusinessDomainId(context, this.Context.BusinessDomainId)) as IQueryable<BusinessDomainQualityScoreEntity>;
        IQueryable<BusinessDomainQualityScoreEntity> score = this.datasetsComponent.GetDataset(z) as IQueryable<BusinessDomainQualityScoreEntity>;

        return score.Select(z => new DataQualityScoresModel()
        {
            QualityScore = z.QualityScore,
            LastRefreshedAt = z.LastRefreshedAt,
            BusinessDomainId = z.BusinessDomainId
        }).Single();
    }

    private ODataQueryOptions<AssetQualityScoreEntity> GetAssetOptions()
    {
        IEdmModel model = this.modelProvider.GetEdmModel(this.requestHeaderContext.ApiVersion);
        IEdmEntitySet entitySet = model.FindDeclaredEntitySet("AssetQualityScore");

        ODataPath odataPath = new(new EntitySetSegment(entitySet));
        ODataQueryContext context = new(model, typeof(AssetQualityScoreEntity), odataPath);

        return new(context, this.requestHeaderContext.HttpContent.Request);
    }

    private ODataQueryOptions<DataProductQualityScoreEntity> GetProductOptions()
    {
        IEdmModel model = this.modelProvider.GetEdmModel(this.requestHeaderContext.ApiVersion);
        IEdmEntitySet entitySet = model.FindDeclaredEntitySet("ProductQualityScore");

        ODataPath odataPath = new(new EntitySetSegment(entitySet));
        ODataQueryContext context = new(model, typeof(DataProductQualityScoreEntity), odataPath);

        return new(context, this.requestHeaderContext.HttpContent.Request);
    }

    private ODataQueryOptions<BusinessDomainQualityScoreEntity> GetBusinessDomainOptions()
    {
        IEdmModel model = this.modelProvider.GetEdmModel(this.requestHeaderContext.ApiVersion);
        IEdmEntitySet entitySet = model.FindDeclaredEntitySet("DomainQualityScore");

        ODataPath odataPath = new(new EntitySetSegment(entitySet));
        ODataQueryContext context = new(model, typeof(BusinessDomainQualityScoreEntity), odataPath);

        return new(context, this.requestHeaderContext.HttpContent.Request);
    }

    private IQueryable GetScoreForAssetId(SynapseSqlContext dbContext, Guid assetId)
    {
        return dbContext.AssetQualityScores.Where(score => score.DataAssetId == assetId);
    }

    private IQueryable GetScoreForDataProductId(SynapseSqlContext dbContext, Guid dataProductId)
    {
        return dbContext.DataProductQualityScores.Where(score => score.DataProductId == dataProductId);
    }

    private IQueryable GetScoreForBusinessDomainId(SynapseSqlContext dbContext, Guid businessDomainId)
    {
        return dbContext.BusinessDomainQualityScores.Where(score => score.BusinessDomainId == businessDomainId);
    }
}
