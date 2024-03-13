namespace Microsoft.Purview.DataEstateHealth.DHDataAccess.Repositories.DHControl.Models;
using System.Collections.Generic;

public class ControlNodeFilters
{
    public IList<string>? AssessmentIds { get; set; }
    public string? StatusPaletteId { get; set; }
    public IList<string>? ParentControlIds { get; set; }
}
