// -----------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
// -----------------------------------------------------------

namespace Microsoft.Azure.Purview.DataEstateHealth.Core;

using System;
using System.Threading.Tasks;
using Microsoft.Azure.Purview.DataEstateHealth.Common;
using Microsoft.Azure.Purview.DataEstateHealth.DataAccess;
using Microsoft.Azure.Purview.DataEstateHealth.Models;
using Microsoft.DGP.ServiceBasics.BaseModels;
using Microsoft.DGP.ServiceBasics.Components;
using Microsoft.DGP.ServiceBasics.Errors;
using Microsoft.DGP.ServiceBasics.Services.FieldInjection;

[Component(typeof(IDataQualityScoreCollectionComponent), ServiceVersion.V1)]
internal class DataQualityScoreCollectionComponent :
    BaseComponent<IDataQualityListContext>,
    IDataQualityScoreCollectionComponent
{
#pragma warning disable 649
    [Inject]
    protected readonly IComponentContextFactory contextFactory;

    [Inject]
    private IDataQualityScoreRepository dataQualityScoreRepository;

    [Inject]
    private readonly IRequestHeaderContext requestHeaderContext;
#pragma warning restore 649

    public DataQualityScoreCollectionComponent(IDataQualityListContext context, int version) : base(context, version)
    {
    }

    [Initialize]
    public void Initialize()
    {
        this.dataQualityScoreRepository = this.dataQualityScoreRepository.ByLocation(this.Context.Location);
    }

    /// <inheritdoc />
    public async Task<IBatchResults<DataQualityScoreModel>> GetDomainScores(
        CancellationToken cancellationToken,
        string skipToken = null)
    {
        IBatchResults<DataQualityScoreModel> actionResults = await this.dataQualityScoreRepository.GetMultiple(
            new DomainDataQualityScoreKey(
                Guid.Empty,
                this.Context.AccountId,
                new Guid(this.requestHeaderContext.CatalogId)),
            cancellationToken,
            skipToken);

        if (actionResults == null)
        {
            throw new ServiceError(
               ErrorCategory.ResourceNotFound,
               ErrorCode.HealthScores_NotAvailable.Code,
               ErrorCode.HealthScores_NotAvailable.FormatMessage("Data quality not available for all business domains."))
           .ToException();
        }

        return actionResults;
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
    public async Task<IBatchResults<DataQualityScoreModel>> GetDataProductScores(
        Guid domainId,
        CancellationToken cancellationToken,
        string skipToken = null)
    {
        IBatchResults<DataQualityScoreModel> actionResults = await this.dataQualityScoreRepository.GetMultiple(
            new DataProductDataQualityScoreKey(
                Guid.Empty,
                domainId,
                this.Context.AccountId,
                new Guid(this.requestHeaderContext.CatalogId)),
            cancellationToken, skipToken);

        if (actionResults == null)
        {
            throw new ServiceError(
               ErrorCategory.ResourceNotFound,
               ErrorCode.HealthScores_NotAvailable.Code,
               ErrorCode.HealthScores_NotAvailable.FormatMessage("Data quality not available for all data products."))
           .ToException();
        }

        return actionResults;
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
    public async Task<IBatchResults<DataQualityScoreModel>> GetDataAssetScores(
        Guid domainId,
        Guid dataProductId,
        CancellationToken cancellationToken,
        string skipToken = null)
    {
        IBatchResults<DataQualityScoreModel> actionResults = await this.dataQualityScoreRepository.GetMultiple(
            new DataAssetDataQualityScoreKey(
                Guid.Empty,
                dataProductId,
                domainId,
                this.Context.AccountId,
                new Guid(this.requestHeaderContext.CatalogId)),
            cancellationToken, skipToken);

        if (actionResults == null)
        {
            throw new ServiceError(
               ErrorCategory.ResourceNotFound,
               ErrorCode.HealthScores_NotAvailable.Code,
               ErrorCode.HealthScores_NotAvailable.FormatMessage("Data quality not available for all data assets."))
           .ToException();
        }

        return actionResults;
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
}
