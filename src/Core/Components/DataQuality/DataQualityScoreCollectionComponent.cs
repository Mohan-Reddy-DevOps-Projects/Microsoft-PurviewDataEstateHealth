// -----------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
// -----------------------------------------------------------

namespace Microsoft.Azure.Purview.DataEstateHealth.Core;

using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.OData.Query;
using Microsoft.Azure.Purview.DataEstateHealth.Common;
using Microsoft.Azure.Purview.DataEstateHealth.DataAccess;
using Microsoft.Azure.Purview.DataEstateHealth.Models;
using Microsoft.DGP.ServiceBasics.BaseModels;
using Microsoft.DGP.ServiceBasics.Components;
using Microsoft.DGP.ServiceBasics.Services.FieldInjection;
using Microsoft.Purview.DataGovernance.DataLakeAPI.Entities;
using Microsoft.Purview.DataGovernance.DataLakeAPI;
using Microsoft.OData.Edm;
using Microsoft.OData.UriParser;

[Component(typeof(IDataQualityScoreCollectionComponent), ServiceVersion.V1)]
internal class DataQualityScoreCollectionComponent :
    BaseComponent<IDataQualityListContext>,
    IDataQualityScoreCollectionComponent
{
#pragma warning disable 649
    [Inject]
    protected readonly IComponentContextFactory contextFactory;

    [Inject]
    private readonly IRequestHeaderContext requestHeaderContext;

    [Inject]
    private readonly IDatasetsComponent datasetsComponent;

    [Inject]
    private readonly IODataModelProvider modelProvider;
#pragma warning restore 649

    public DataQualityScoreCollectionComponent(IDataQualityListContext context, int version) : base(context, version)
    {
    }

    [Initialize]
    public void Initialize()
    {
    }

    /// <inheritdoc />
    public async Task<IBatchResults<IDataQualityScoresModel>> GetDomainScores(
        CancellationToken cancellationToken,
    string skipToken = null)
    {
        ODataQueryOptions<BusinessDomainQualityScoreEntity> query = this.GetDomainOptions();
        await using SynapseSqlContext context = await this.datasetsComponent.GetContext(cancellationToken);
        Func<IQueryable<BusinessDomainQualityScoreEntity>> x = () => query.ApplyTo(context.BusinessDomainQualityScores.AsQueryable()) as IQueryable<BusinessDomainQualityScoreEntity>;
        IQueryable<BusinessDomainQualityScoreEntity> businessDomainQualityScoresEntitiesList = this.datasetsComponent.GetDataset(x) as IQueryable<BusinessDomainQualityScoreEntity>;

        List<IDataQualityScoresModel> businessDomainQualityScoresModelList = new();
        BaseBatchResults<IDataQualityScoresModel> result = new()
        {
            Results = businessDomainQualityScoresModelList,
            ContinuationToken = null
        };

        if (businessDomainQualityScoresModelList != null)
        {
            businessDomainQualityScoresModelList.AddRange(businessDomainQualityScoresEntitiesList.Select(businessDomainsQualityScoreEntity =>
                new DataQualityScoresModel()
                {
                    BusinessDomainId = businessDomainsQualityScoreEntity.BusinessDomainId,
                    LastRefreshedAt = businessDomainsQualityScoreEntity.LastRefreshedAt,
                    QualityScore = businessDomainsQualityScoreEntity.QualityScore,
                }));
        }

        return result;
    }

    /// <inheritdoc/>
    public IDataQualityScoreComponent GetDomainScoreById(
        Guid domainId)
    {
        return this.ComponentRuntime.Resolve<IDataQualityScoreComponent, IDataQualityContext>(
            this.contextFactory.CreateDataQualityContext(
                this.Context.Version,
                this.Context.Location,
                this.Context.AccountId,
                this.Context.TenantId,
                domainId,
                Guid.Empty,
                Guid.Empty),
            this.Context.Version.Numeric);
    }

    /// <inheritdoc/>
    public async Task<IBatchResults<IDataQualityScoresModel>> GetDataProductScores(
        CancellationToken cancellationToken,
        string skipToken = null)
    {
        ODataQueryOptions<DataProductQualityScoreEntity> query = this.GetProductOptions();
        await using SynapseSqlContext context = await this.datasetsComponent.GetContext(cancellationToken);
        Func<IQueryable<DataProductQualityScoreEntity>> x = () => query.ApplyTo(context.DataProductQualityScores.AsQueryable()) as IQueryable<DataProductQualityScoreEntity>;
        IQueryable<DataProductQualityScoreEntity> dataProductQualityScoresEntitiesList = this.datasetsComponent.GetDataset(x) as IQueryable<DataProductQualityScoreEntity>;

        List<IDataQualityScoresModel> dataProductQualityScoresModelList = new();
        BaseBatchResults<IDataQualityScoresModel> result = new()
        {
            Results = dataProductQualityScoresModelList,
            ContinuationToken = null
        };

        if (dataProductQualityScoresModelList != null)
        {
            dataProductQualityScoresModelList.AddRange(dataProductQualityScoresEntitiesList.Select(dataProductsQualityScoreEntity =>
                new DataQualityScoresModel()
                {
                    BusinessDomainId = dataProductsQualityScoreEntity.BusinessDomainId,
                    DataProductId = dataProductsQualityScoreEntity.DataProductId,
                    LastRefreshedAt = dataProductsQualityScoreEntity.LastRefreshedAt,
                    QualityScore = dataProductsQualityScoreEntity.QualityScore,
                }));
        }

        return result;
    }

    public IDataQualityScoreComponent GetDataProductScoreById(
        Guid dataProductId)
    {
        return this.ComponentRuntime.Resolve<IDataQualityScoreComponent, IDataQualityContext>(
            this.contextFactory.CreateDataQualityContext(
                this.Context.Version,
                this.Context.Location,
                this.Context.AccountId,
                this.Context.TenantId,
                Guid.Empty,
                dataProductId,
                Guid.Empty),
            this.Context.Version.Numeric);
    }

    /// <inheritdoc/>
    public async Task<IBatchResults<IDataQualityScoresModel>> GetDataAssetScores(
        CancellationToken cancellationToken,
        string skipToken = null)
    {
        ODataQueryOptions<AssetQualityScoreEntity> query = this.GetAssetOptions();
        await using SynapseSqlContext context = await this.datasetsComponent.GetContext(cancellationToken);
        Func<IQueryable<AssetQualityScoreEntity>> x = () => query.ApplyTo(context.AssetQualityScores.AsQueryable()) as IQueryable<AssetQualityScoreEntity>;
        IQueryable<AssetQualityScoreEntity> assetQualityScoresEntitiesList = this.datasetsComponent.GetDataset(x) as IQueryable<AssetQualityScoreEntity>;

        List<IDataQualityScoresModel> assetQualityScoresModelList = new();
        BaseBatchResults<IDataQualityScoresModel> result = new()
        {
            Results = assetQualityScoresModelList,
            ContinuationToken = null
        };

        if (assetQualityScoresModelList != null)
        {
            assetQualityScoresModelList.AddRange(assetQualityScoresEntitiesList.Select(assetQualityScoreEntity =>
                new DataQualityScoresModel()
                {
                    DataAssetId = assetQualityScoreEntity.DataAssetId,
                    BusinessDomainId = assetQualityScoreEntity.BusinessDomainId,
                    DataProductId = assetQualityScoreEntity.DataProductId,
                    LastRefreshedAt = assetQualityScoreEntity.LastRefreshedAt,
                    QualityScore = assetQualityScoreEntity.QualityScore,
                }));
        }

        return result;
    }

    public IDataQualityScoreComponent GetDataAssetScoreById(
        Guid dataAssetId)
    {
        return this.ComponentRuntime.Resolve<IDataQualityScoreComponent, IDataQualityContext>(
            this.contextFactory.CreateDataQualityContext(
                this.Context.Version,
                this.Context.Location,
                this.Context.AccountId,
                this.Context.TenantId,
                Guid.Empty,
                Guid.Empty,
                dataAssetId),
            this.Context.Version.Numeric);
    }

    private ODataQueryOptions<BusinessDomainQualityScoreEntity> GetDomainOptions()
    {
        IEdmModel model = this.modelProvider.GetEdmModel(this.requestHeaderContext.ApiVersion);
        IEdmEntitySet entitySet = model.FindDeclaredEntitySet("DomainQualityScore");

        ODataPath odataPath = new(new EntitySetSegment(entitySet));
        ODataQueryContext context = new(model, typeof(BusinessDomainQualityScoreEntity), odataPath);

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

    private ODataQueryOptions<AssetQualityScoreEntity> GetAssetOptions()
    {
        IEdmModel model = this.modelProvider.GetEdmModel(this.requestHeaderContext.ApiVersion);
        IEdmEntitySet entitySet = model.FindDeclaredEntitySet("AssetQualityScore");

        ODataPath odataPath = new(new EntitySetSegment(entitySet));
        ODataQueryContext context = new(model, typeof(AssetQualityScoreEntity), odataPath);

        return new(context, this.requestHeaderContext.HttpContent.Request);
    }
}
