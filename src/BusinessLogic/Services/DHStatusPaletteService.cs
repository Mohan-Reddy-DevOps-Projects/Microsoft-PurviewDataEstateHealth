#nullable enable

namespace Microsoft.Purview.DataEstateHealth.BusinessLogic.Services
{
    using Microsoft.Azure.Purview.DataEstateHealth.Loggers;
    using Microsoft.Azure.Purview.DataEstateHealth.Models;
    using Microsoft.Purview.DataEstateHealth.BusinessLogic.Exceptions;
    using Microsoft.Purview.DataEstateHealth.BusinessLogic.Exceptions.Model;
    using Microsoft.Purview.DataEstateHealth.BusinessLogic.InternalServices;
    using Microsoft.Purview.DataEstateHealth.DHDataAccess;
    using Microsoft.Purview.DataEstateHealth.DHDataAccess.Repositories.DHControl;
    using Microsoft.Purview.DataEstateHealth.DHDataAccess.Repositories.DHControl.Models;
    using Microsoft.Purview.DataEstateHealth.DHModels.Services.Control.Palette;
    using Microsoft.Purview.DataEstateHealth.DHModels.Wrapper.Attributes;
    using Microsoft.Purview.DataEstateHealth.DHModels.Wrapper.Exceptions;
    using System;
    using System.Globalization;
    using System.Linq;
    using System.Threading.Tasks;

    public class DHStatusPaletteService(
        DHControlStatusPaletteRepository dhControlStatusPaletteRepository,
        DHStatusPaletteInternalService statusPaletteInternalService,
        DHControlRepository controlRepository,
        IRequestHeaderContext requestHeaderContext,
        IDataEstateHealthRequestLogger logger)
    {
        public async Task<IBatchResults<DHControlStatusPaletteWrapper>> ListStatusPalettesAsync()
        {
            using (logger.LogElapsed($"{this.GetType().Name}#{nameof(ListStatusPalettesAsync)}"))
            {
                var results = await dhControlStatusPaletteRepository.GetAllAsync().ConfigureAwait(false);

                var resp = statusPaletteInternalService.AppendSystemDefauleStatusPalettes(results);

                return new BatchResults<DHControlStatusPaletteWrapper>(resp, resp.Count());
            }
        }

        public async Task<DHControlStatusPaletteWrapper> GetStatusPaletteByIdAsync(string id)
        {
            ArgumentNullException.ThrowIfNull(id);

            using (logger.LogElapsed($"{this.GetType().Name}#{nameof(GetStatusPaletteByIdAsync)}: Read for status palette with ID {id}"))
            {
                var entity = await statusPaletteInternalService.TryGetStatusPaletteByIdInternalAsync(id).ConfigureAwait(false);

                if (entity == null)
                {
                    throw new EntityNotFoundException(new ExceptionRefEntityInfo(EntityCategory.StatusPalette.ToString(), id));
                }

                return entity;
            }
        }

        public async Task<DHControlStatusPaletteWrapper> CreateStatusPaletteAsync(DHControlStatusPaletteWrapper entity)
        {
            ArgumentNullException.ThrowIfNull(entity);

            using (logger.LogElapsed($"{this.GetType().Name}#{nameof(CreateStatusPaletteAsync)}: Create status palette"))
            {
                entity.Validate();
                entity.NormalizeInput();

                entity.OnCreate(requestHeaderContext.ClientObjectId);

                await dhControlStatusPaletteRepository.AddAsync(entity).ConfigureAwait(false);
                return entity;
            }
        }

        public async Task<DHControlStatusPaletteWrapper> UpdateStatusPaletteByIdAsync(string id, DHControlStatusPaletteWrapper entity)
        {
            ArgumentNullException.ThrowIfNull(id);

            ArgumentNullException.ThrowIfNull(entity);

            using (logger.LogElapsed($"{this.GetType().Name}#{nameof(UpdateStatusPaletteByIdAsync)}: Update status palette with ID {id}"))
            {
                if (!string.IsNullOrEmpty(entity.Id) && !string.Equals(id, entity.Id, StringComparison.OrdinalIgnoreCase))
                {
                    throw new EntityValidationException(String.Format(CultureInfo.InvariantCulture, StringResources.ErrorMessageUpdateEntityIdNotMatch, EntityCategory.StatusPalette.ToString(), entity.Id, id));
                }

                entity.Validate();
                entity.NormalizeInput();

                var existEntity = await statusPaletteInternalService.TryGetStatusPaletteByIdInternalAsync(id).ConfigureAwait(false);

                if (existEntity == null)
                {
                    throw new EntityNotFoundException(new ExceptionRefEntityInfo(EntityCategory.StatusPalette.ToString(), id));
                }

                if (existEntity.Reserved == true)
                {
                    throw new EntityValidationException(String.Format(CultureInfo.InvariantCulture, StringResources.ErrorMessageCannotChangeSystemDefinedEntity));
                }

                entity.OnUpdate(existEntity, requestHeaderContext.ClientObjectId);

                await dhControlStatusPaletteRepository.UpdateAsync(entity).ConfigureAwait(false);

                return entity;
            }
        }

        public async Task DeleteStatusPaletteByIdAsync(string id)
        {
            ArgumentNullException.ThrowIfNull(id);

            using (logger.LogElapsed($"{this.GetType().Name}#{nameof(DeleteStatusPaletteByIdAsync)}: Delete status palette with ID {id}"))
            {
                var existEntity = await statusPaletteInternalService.TryGetStatusPaletteByIdInternalAsync(id).ConfigureAwait(false);

                if (existEntity == null)
                {
                    logger.LogWarning($"Status palette with ID {id} not found. No action taken.");

                    return;
                }

                if (existEntity.Reserved == true)
                {
                    throw new EntityValidationException(String.Format(CultureInfo.InvariantCulture, StringResources.ErrorMessageCannotChangeSystemDefinedEntity));
                }

                var relatedControls = await controlRepository.QueryControlNodesAsync(new ControlNodeFilters { StatusPaletteId = id }).ConfigureAwait(false);

                if (relatedControls.Any())
                {
                    throw new EntityReferencedException(String.Format(
                        CultureInfo.InvariantCulture,
                        StringResources.ErrorMessageDeleteFailureEntityReferenced,
                        EntityCategory.StatusPalette.ToString(),
                        id,
                        EntityCategory.Control,
                        String.Join(", ", relatedControls.Select(x => $"\"{x.Id}\""))));
                }

                await dhControlStatusPaletteRepository.DeleteAsync(id).ConfigureAwait(false);
            }
        }

        public async Task DeprovisionForStatusPalettesAsync()
        {
            using (logger.LogElapsed($"{this.GetType().Name}#{nameof(DeprovisionForStatusPalettesAsync)}: Deprovision status palettes"))
            {
                try
                {
                    await dhControlStatusPaletteRepository.DeprovisionAsync().ConfigureAwait(false);
                }
                catch (Exception ex)
                {
                    logger.LogError($"Error in {this.GetType().Name}#{nameof(DeprovisionForStatusPalettesAsync)} while deprovisioning for Status Palettes", ex);
                    throw;
                }
            }
        }
    }
}
