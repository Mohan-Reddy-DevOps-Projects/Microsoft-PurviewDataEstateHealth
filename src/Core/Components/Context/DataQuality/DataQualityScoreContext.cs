// -----------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
// -----------------------------------------------------------

namespace Microsoft.Azure.Purview.DataEstateHealth.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

/// <inheritdoc cref="IDataQualityScoreContext" />
internal class DataQualityScoreContext : ComponentContext, IDataQualityScoreContext
{
    public Guid BusinessDomainId { get; set; }
}
