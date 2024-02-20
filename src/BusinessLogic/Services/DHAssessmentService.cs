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
using Microsoft.Purview.DataEstateHealth.DHModels.Services.Control.MQAssessment;
using Microsoft.Purview.DataEstateHealth.DHModels.Wrapper.Attributes;
using Microsoft.Purview.DataEstateHealth.DHModels.Wrapper.Exceptions;
using System;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;

public class DHAssessmentService(MQAssessmentRepository assessmentRepository, IRequestHeaderContext requestHeaderContext)
{
    public async Task<IBatchResults<MQAssessmentWrapper>> ListAssessmentsAsync()
    {
        var entities = await assessmentRepository.GetAllAsync().ConfigureAwait(false);

        return new BatchResults<MQAssessmentWrapper>(entities, entities.Count());
    }

    public async Task<MQAssessmentWrapper> GetAssessmentByIdAsync(string id)
    {
        ArgumentNullException.ThrowIfNull(id);

        var entity = await assessmentRepository.GetByIdAsync(id).ConfigureAwait(false);

        if (entity == null)
        {
            throw new EntityNotFoundException(new ExceptionRefEntityInfo(EntityCategory.Assessment.ToString(), id));
        }

        return entity;
    }

    public async Task<MQAssessmentWrapper> CreateAssessmentAsync(MQAssessmentWrapper entity)
    {
        ArgumentNullException.ThrowIfNull(entity);

        entity.Validate();
        entity.NormalizeInput();

        entity.OnCreate(requestHeaderContext.ClientObjectId);

        await assessmentRepository.AddAsync(entity).ConfigureAwait(false);

        return entity;
    }

    public async Task<MQAssessmentWrapper> UpdateAssessmentByIdAsync(string id, MQAssessmentWrapper entity)
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

        await assessmentRepository.DeleteAsync(entity).ConfigureAwait(false);
    }
}
