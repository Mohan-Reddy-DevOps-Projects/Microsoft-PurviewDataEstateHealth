// -----------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
// -----------------------------------------------------------

namespace Microsoft.Azure.Purview.DataEstateHealth.DataAccess;

/// <summary>
/// Holds information needed to retrieve health control by controlId.
/// </summary>
public class HealthControlKey
{
    /// <summary>
    /// Initializes a new instance of the <see cref="HealthControlKey"/> class.
    /// </summary>
    public HealthControlKey(Guid controlId)
    {
        this.ControlId = controlId;
    }

    /// <summary>
    ///ControlId.
    /// </summary>
    public Guid ControlId { get; set; }

    /// <inheritdoc />
    public override string ToString()
    {
        return this.ControlId.ToString();
    }
}
