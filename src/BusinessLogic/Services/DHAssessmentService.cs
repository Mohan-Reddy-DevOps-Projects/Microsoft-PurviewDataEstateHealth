// <copyright file="DHAssessmentService.cs" company="Microsoft Corporation">
// Copyright (c) Microsoft Corporation. All rights reserved.
// </copyright>
#nullable enable

namespace Microsoft.Purview.DataEstateHealth.BusinessLogic.Services;

using Microsoft.Azure.Purview.DataEstateHealth.Models;
using Microsoft.Purview.DataEstateHealth.BusinessLogic.Exceptions;
using Microsoft.Purview.DataEstateHealth.BusinessLogic.Exceptions.Model;
using Microsoft.Purview.DataEstateHealth.DHDataAccess;
using Microsoft.Purview.DataEstateHealth.DHDataAccess.Repositories.DHControl;
using Microsoft.Purview.DataEstateHealth.DHDataAccess.Repositories.DHControl.Models;
using Microsoft.Purview.DataEstateHealth.DHModels.Services.Control.DHAssessment;
using Microsoft.Purview.DataEstateHealth.DHModels.Wrapper.Attributes;
using Microsoft.Purview.DataEstateHealth.DHModels.Wrapper.Exceptions;
using System;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;

public class DHAssessmentService(
    DHAssessmentRepository assessmentRepository,
    DHControlRepository controlRepository,
    IRequestHeaderContext requestHeaderContext)
{
    public async Task<IBatchResults<DHAssessmentWrapper>> ListAssessmentsAsync()
    {
        var entities = await assessmentRepository.GetAllAsync().ConfigureAwait(false);

        return new BatchResults<DHAssessmentWrapper>(entities, entities.Count());
    }

    public async Task<DHAssessmentWrapper> GetAssessmentByIdAsync(string id)
    {
        ArgumentNullException.ThrowIfNull(id);

        var entity = await assessmentRepository.GetByIdAsync(id).ConfigureAwait(false);

        if (entity == null)
        {
            throw new EntityNotFoundException(new ExceptionRefEntityInfo(EntityCategory.Assessment.ToString(), id));
        }

        return entity;
    }

    public async Task<DHAssessmentWrapper> CreateAssessmentAsync(DHAssessmentWrapper entity, bool isSystem = false)
    {
        ArgumentNullException.ThrowIfNull(entity);

        entity.Validate();

        if (!isSystem)
        {
            entity.NormalizeInput();
        }

        entity.OnCreate(requestHeaderContext.ClientObjectId);

        await assessmentRepository.AddAsync(entity).ConfigureAwait(false);

        return entity;
    }

    public async Task<DHAssessmentWrapper> CreateEmptyAssessmentAsync(string assessmentName)
    {
        ArgumentException.ThrowIfNullOrEmpty(assessmentName);

        var entity = DHAssessmentWrapper.Create([]);

        entity.Name = assessmentName;

        var aggregationWrapper = new DHAssessmentSimpleAggregationWrapper();
        aggregationWrapper.Type = DHAssessmentAggregationBaseWrapperDerivedTypes.Simple;
        aggregationWrapper.AggregationType = DHAssessmentSimpleAggregationType.Average;
        entity.AggregationWrapper = aggregationWrapper;

        return await this.CreateAssessmentAsync(entity).ConfigureAwait(false);
    }

    public async Task<DHAssessmentWrapper> UpdateAssessmentByIdAsync(string id, DHAssessmentWrapper entity)
    {
        ArgumentNullException.ThrowIfNull(id);
        ArgumentNullException.ThrowIfNull(entity);

        if (!string.IsNullOrEmpty(entity.Id) && !string.Equals(id, entity.Id, StringComparison.OrdinalIgnoreCase))
        {
            throw new EntityValidationException(String.Format(CultureInfo.InvariantCulture, StringResources.ErrorMessageUpdateEntityIdNotMatch, EntityCategory.Assessment.ToString(), entity.Id, id));
        }

        entity.Validate();
        entity.NormalizeInput();

        var existEntity = await assessmentRepository.GetByIdAsync(id).ConfigureAwait(false);

        if (existEntity == null)
        {
            throw new EntityNotFoundException(new ExceptionRefEntityInfo(EntityCategory.Assessment.ToString(), id));
        }

        entity.OnUpdate(existEntity, requestHeaderContext.ClientObjectId);

        await assessmentRepository.UpdateAsync(entity).ConfigureAwait(false);

        return entity;
    }

    public async Task DeleteAssessmentByIdAsync(string id)
    {
        ArgumentNullException.ThrowIfNull(id);

        var entity = await assessmentRepository.GetByIdAsync(id).ConfigureAwait(false);

        if (entity == null)
        {
            // Log

            return;
        }

        var relatedControls = await controlRepository.QueryControlNodesAsync(new ControlNodeFilters { AssessmentIds = [id] }).ConfigureAwait(false);

        if (relatedControls.Any())
        {
            throw new EntityReferencedException(String.Format(
                CultureInfo.InvariantCulture,
                StringResources.ErrorMessageEntityReferenced,
                EntityCategory.Assessment.ToString(),
                id,
                EntityCategory.Control,
                String.Join(", ", relatedControls.Select(x => $"\"{x.Id}\""))));
        }

        await assessmentRepository.DeleteAsync(entity).ConfigureAwait(false);
    }
}
