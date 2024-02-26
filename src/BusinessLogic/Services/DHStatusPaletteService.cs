#nullable enable

namespace Microsoft.Purview.DataEstateHealth.BusinessLogic.Services
{
    using Microsoft.Azure.Purview.DataEstateHealth.Models;
    using Microsoft.Purview.DataEstateHealth.BusinessLogic.Exceptions;
    using Microsoft.Purview.DataEstateHealth.BusinessLogic.Exceptions.Model;
    using Microsoft.Purview.DataEstateHealth.DHDataAccess;
    using Microsoft.Purview.DataEstateHealth.DHDataAccess.Repositories.DHControl;
    using Microsoft.Purview.DataEstateHealth.DHModels.Services.Control.Palette;
    using Microsoft.Purview.DataEstateHealth.DHModels.Wrapper.Attributes;
    using Microsoft.Purview.DataEstateHealth.DHModels.Wrapper.Exceptions;
    using System;
    using System.Collections.Generic;
    using System.Globalization;
    using System.Linq;
    using System.Threading.Tasks;

    public class DHStatusPaletteService(DHControlStatusPaletteRepository dhControlStatusPaletteRepository, IRequestHeaderContext requestHeaderContext)
    {
        public async Task<IBatchResults<DHControlStatusPaletteWrapper>> ListStatusPalettesAsync()
        {
            var results = await dhControlStatusPaletteRepository.GetAllAsync().ConfigureAwait(false);

            IEnumerable<DHControlStatusPaletteWrapper> resp = [.. SystemDefaultStatusPalettes, ..results];

            return new BatchResults<DHControlStatusPaletteWrapper>(resp, resp.Count());
        }

        public async Task<DHControlStatusPaletteWrapper> GetStatusPaletteByIdAsync(string id)
        {
            ArgumentNullException.ThrowIfNull(id);

            var systemEntity = SystemDefaultStatusPalettes.FirstOrDefault(e => string.Equals(e.Id, id, StringComparison.OrdinalIgnoreCase));

            if (systemEntity != null)
            {
                return systemEntity;
            }

            var entity = await dhControlStatusPaletteRepository.GetByIdAsync(id).ConfigureAwait(false);

            if (entity == null)
            {
                throw new EntityNotFoundException(new ExceptionRefEntityInfo(EntityCategory.StatusPalette.ToString(), id));
            }

            return entity;
        }

        public async Task<DHControlStatusPaletteWrapper> CreateStatusPaletteAsync(DHControlStatusPaletteWrapper entity)
        {
            ArgumentNullException.ThrowIfNull(entity);

            entity.Validate();
            entity.NormalizeInput();

            entity.OnCreate(requestHeaderContext.ClientObjectId);

            await dhControlStatusPaletteRepository.AddAsync(entity).ConfigureAwait(false);
            return entity;
        }

        public async Task<DHControlStatusPaletteWrapper> UpdateStatusPaletteByIdAsync(string id, DHControlStatusPaletteWrapper entity)
        {
            ArgumentNullException.ThrowIfNull(id);

            ArgumentNullException.ThrowIfNull(entity);

            var systemEntity = SystemDefaultStatusPalettes.FirstOrDefault(e => string.Equals(e.Id, id, StringComparison.OrdinalIgnoreCase));

            if (systemEntity != null)
            {
                throw new EntityValidationException(String.Format(CultureInfo.InvariantCulture, StringResources.ErrorMessageCannotChangeSystemDefinedEntity));
            }

            if (!string.IsNullOrEmpty(entity.Id) && !string.Equals(id, entity.Id, StringComparison.OrdinalIgnoreCase))
            {
                throw new EntityValidationException(String.Format(CultureInfo.InvariantCulture, StringResources.ErrorMessageUpdateEntityIdNotMatch, EntityCategory.StatusPalette.ToString(), entity.Id, id));
            }

            entity.Validate();
            entity.NormalizeInput();

            var existEntity = await dhControlStatusPaletteRepository.GetByIdAsync(id).ConfigureAwait(false);

            if (existEntity == null)
            {
                throw new EntityNotFoundException(new ExceptionRefEntityInfo(EntityCategory.StatusPalette.ToString(), id));
            }

            entity.OnUpdate(existEntity, requestHeaderContext.ClientObjectId);

            await dhControlStatusPaletteRepository.UpdateAsync(entity).ConfigureAwait(false);

            return entity;
        }

        public async Task DeleteStatusPaletteByIdAsync(string id)
        {
            ArgumentNullException.ThrowIfNull(id);

            var systemEntity = SystemDefaultStatusPalettes.FirstOrDefault(e => string.Equals(e.Id, id, StringComparison.OrdinalIgnoreCase));

            if (systemEntity != null)
            {
                throw new EntityValidationException(String.Format(CultureInfo.InvariantCulture, StringResources.ErrorMessageCannotChangeSystemDefinedEntity));
            }

            var existEntity = await dhControlStatusPaletteRepository.GetByIdAsync(id).ConfigureAwait(false);

            if (existEntity == null)
            {
                // Log

                return;
            }

            await dhControlStatusPaletteRepository.DeleteAsync(id).ConfigureAwait(false);
        }

        private static List<DHControlStatusPaletteWrapper> SystemDefaultStatusPalettes { get; } = new List<DHControlStatusPaletteWrapper>
        {
            new DHControlStatusPaletteWrapper
            {
                Id = "00000000-0000-0000-0000-000000000001",
                Name = "Undefiend",
                Color = "#949494"
            },
            new DHControlStatusPaletteWrapper
            {
                Id = "00000000-0000-0000-0000-000000000002",
                Name = "Healthy",
                Color = "#009b51"
            },
            new DHControlStatusPaletteWrapper
            {
                Id = "00000000-0000-0000-0000-000000000003",
                Name = "Fair",
                Color = "#e67e00"
            },
            new DHControlStatusPaletteWrapper
            {
                Id = "00000000-0000-0000-0000-000000000004",
                Name = "Not healthy",
                Color = "#d13438"
            },
            new DHControlStatusPaletteWrapper
            {
                Id = "00000000-0000-0000-0000-000000000005",
                Name = "Critical",
                Color = "#6b3f9e"
            },
        };
    }
}
