namespace Microsoft.Purview.DataEstateHealth.BusinessLogic.InternalServices;
using Microsoft.Purview.DataEstateHealth.DHDataAccess.Repositories.DHControl;
using Microsoft.Purview.DataEstateHealth.DHDataAccess.Repositories.DHControl.Models;
using Microsoft.Purview.DataEstateHealth.DHModels.Services.Control.Palette;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

public class DHStatusPaletteInternalService(
    DHControlStatusPaletteRepository dhControlStatusPaletteRepository
    )
{
    public async Task<IEnumerable<DHControlStatusPaletteWrapper>> QueryStatusPalettesAsync(StatusPaletteFilters filters)
    {
        var systemEntities = SystemDefaultStatusPalettes.Where(e => filters.ids?.Contains(e.Id) ?? true);

        var entities = await dhControlStatusPaletteRepository.QueryStatusPalettesAsync(filters).ConfigureAwait(false);

        return [.. systemEntities, .. entities];
    }

    public async Task<DHControlStatusPaletteWrapper?> TryGetStatusPaletteByIdInternalAsync(string id)
    {
        ArgumentNullException.ThrowIfNull(id);

        var systemEntity = SystemDefaultStatusPalettes.FirstOrDefault(e => string.Equals(e.Id, id, StringComparison.OrdinalIgnoreCase));

        if (systemEntity != null)
        {
            return systemEntity;
        }

        var entity = await dhControlStatusPaletteRepository.GetByIdAsync(id).ConfigureAwait(false);

        return entity;
    }

    public IEnumerable<DHControlStatusPaletteWrapper> AppendSystemDefauleStatusPalettes(IEnumerable<DHControlStatusPaletteWrapper> results)
    {
        IEnumerable<DHControlStatusPaletteWrapper> resp = [.. SystemDefaultStatusPalettes, .. results];

        return resp;
    }

    private static List<DHControlStatusPaletteWrapper> SystemDefaultStatusPalettes { get; } = new List<DHControlStatusPaletteWrapper>
    {
        new DHControlStatusPaletteWrapper
        {
            Id = "00000000-0000-0000-0000-000000000001",
            Name = "Undefiend",
            Color = "#949494",
            Reserved = true
        },
        new DHControlStatusPaletteWrapper
        {
            Id = "00000000-0000-0000-0000-000000000002",
            Name = "Healthy",
            Color = "#009b51",
            Reserved = true
        },
        new DHControlStatusPaletteWrapper
        {
            Id = "00000000-0000-0000-0000-000000000003",
            Name = "Fair",
            Color = "#e67e00",
            Reserved = true
        },
        new DHControlStatusPaletteWrapper
        {
            Id = "00000000-0000-0000-0000-000000000004",
            Name = "Not healthy",
            Color = "#d13438",
            Reserved = true
        },
        new DHControlStatusPaletteWrapper
        {
            Id = "00000000-0000-0000-0000-000000000005",
            Name = "Critical",
            Color = "#6b3f9e",
            Reserved = true
        },
    };
}
