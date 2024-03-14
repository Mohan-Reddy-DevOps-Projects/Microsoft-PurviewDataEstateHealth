namespace Microsoft.Purview.DataEstateHealth.DHDataAccess.Repositories.DHControl.Models;

using System.Collections.Generic;

public class TemplateFilters
{
    public string? TemplateName { get; set; }

    public IList<string>? TemplateEntityIds { get; set; }
}
