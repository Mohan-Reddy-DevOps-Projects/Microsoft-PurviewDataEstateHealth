// -----------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
// -----------------------------------------------------------

namespace Microsoft.Purview.DataEstateHealth.Core;

using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Azure.Purview.DataEstateHealth.Models;
using Microsoft.Purview.DataGovernance.DeltaWriter;

internal interface IDeltaTableEventProcessor
{
    /// <summary>
    /// Persists data to storage based on event operation types.
    /// </summary>
    /// <param name="models">A dictionary of entity models.</param>
    /// <param name="deltaTableWriter">The Delta Lake table writer.</param>
    /// <param name="prefix">Prefix for the path.</param>
    /// <param name="isSourceEvent">Flag to determine source event.</param>
    Task PersistToStorage<T>(Dictionary<EventOperationType, List<T>> models, IDeltaLakeOperator deltaTableWriter, string prefix, bool isSourceEvent = true) where T : BaseEventHubEntityModel;
}
