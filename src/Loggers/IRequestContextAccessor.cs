// -----------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
// -----------------------------------------------------------

namespace Microsoft.Azure.Purview.DataEstateHealth.Loggers;

using Microsoft.Azure.Purview.DataEstateHealth.Models;

internal interface IRequestContextAccessor
{
    IRequestHeaderContext GetRequestContext();

    void SetRequestContext(IRequestHeaderContext requestContext);
}
