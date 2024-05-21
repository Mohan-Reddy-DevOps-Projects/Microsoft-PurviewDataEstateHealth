namespace Microsoft.Purview.DataEstateHealth.BusinessLogic.Services;

using Microsoft.Azure.Purview.DataEstateHealth.Loggers;
using Microsoft.Azure.Purview.DataEstateHealth.Models;
using Microsoft.Purview.DataEstateHealth.BusinessLogic.Exceptions;
using Microsoft.Purview.DataEstateHealth.BusinessLogic.Exceptions.Model;
using Microsoft.Purview.DataEstateHealth.DHDataAccess;
using Microsoft.Purview.DataEstateHealth.DHDataAccess.Repositories.DHControl;
using Microsoft.Purview.DataEstateHealth.DHModels.Services.Control.Alert;
using Microsoft.Purview.DataEstateHealth.DHModels.Wrapper.Attributes;
using System;
using System.Linq;
using System.Threading.Tasks;

public class DHAlertService(
    DHAlertRepository alertRepository,
    IRequestHeaderContext requestHeaderContext,
    IDataEstateHealthRequestLogger logger
    )
{
    public async Task<IBatchResults<DHAlertWrapper>> ListAlertsAsync()
    {
        using (logger.LogElapsed($"{this.GetType().Name}#{nameof(ListAlertsAsync)}"))
        {
            var results = await alertRepository.GetAllAsync().ConfigureAwait(false);

            return new BatchResults<DHAlertWrapper>(results, results.Count());
        }
    }

    public async Task<DHAlertWrapper> GetAlertByIdAsync(string id)
    {
        ArgumentNullException.ThrowIfNull(id);

        using (logger.LogElapsed($"{this.GetType().Name}#{nameof(GetAlertByIdAsync)}: Get by ID {id}."))
        {
            var entity = await alertRepository.GetByIdAsync(id).ConfigureAwait(false);

            if (entity == null)
            {
                throw new EntityNotFoundException(new ExceptionRefEntityInfo(EntityCategory.Alert.ToString(), id));
            }

            return entity;
        }
    }

    public async Task<DHAlertWrapper> CreateAlertAsync(DHAlertWrapper entity)
    {
        ArgumentNullException.ThrowIfNull(entity);

        using (logger.LogElapsed($"{this.GetType().Name}#{nameof(CreateAlertAsync)}"))
        {
            entity.Validate();
            entity.NormalizeInput();

            entity.OnCreate(requestHeaderContext.ClientObjectId);

            await alertRepository.AddAsync(entity).ConfigureAwait(false);

            return entity;
        }
    }

    public async Task<DHAlertWrapper> UpdateAlertByIdAsync(string id, DHAlertWrapper entity)
    {
        ArgumentNullException.ThrowIfNull(id);
        ArgumentNullException.ThrowIfNull(entity);

        using (logger.LogElapsed($"{this.GetType().Name}#{nameof(UpdateAlertByIdAsync)}: Update by ID {id}."))
        {
            entity.Validate();
            entity.NormalizeInput();


            var existingEntity = await alertRepository.GetByIdAsync(id).ConfigureAwait(false);

            if (existingEntity == null)
            {
                throw new EntityNotFoundException(new ExceptionRefEntityInfo(EntityCategory.Alert.ToString(), id));
            }

            entity.OnUpdate(existingEntity, requestHeaderContext.ClientObjectId);

            await alertRepository.UpdateAsync(entity).ConfigureAwait(false);

            return entity;
        }
    }

    public async Task DeleteAlertByIdAsync(string id)
    {
        ArgumentNullException.ThrowIfNull(id);

        using (logger.LogElapsed($"{this.GetType().Name}#{nameof(DeleteAlertByIdAsync)}: Delete by ID {id}."))
        {
            var existingEntity = await alertRepository.GetByIdAsync(id).ConfigureAwait(false);

            if (existingEntity == null)
            {
                logger.LogWarning($"Alert with ID {id} not found. No action taken.");

                return;
            }

            await alertRepository.DeleteAsync(existingEntity).ConfigureAwait(false);
        }
    }

    public async Task DeprovisionForAlertsAsync()
    {
        using (logger.LogElapsed($"{this.GetType().Name}#{nameof(DeprovisionForAlertsAsync)}"))
        {
            try
            {
                await alertRepository.DeprovisionAsync().ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                logger.LogError($"Error in {this.GetType().Name}#{nameof(DeprovisionForAlertsAsync)}", ex);
                throw;
            }
        }
    }
}
