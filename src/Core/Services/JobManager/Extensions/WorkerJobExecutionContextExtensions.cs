// -----------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
// -----------------------------------------------------------

namespace Microsoft.Azure.Purview.DataEstateHealth.Core;

using System.Collections.Generic;
using System;
using System.Linq;

internal static class WorkerJobExecutionContextExtensions
{
    /// <summary>
    /// Returns all flags in the <see cref="Enum"/>.
    /// </summary>
    /// <param name="e">The <see cref="Enum"/></param>
    /// <returns>An <see cref="IEnumerable{T}"/>.</returns>
    public static IEnumerable<Enum> GetFlags(this Enum e)
    {
        return Enum.GetValues(e.GetType()).Cast<Enum>().Where(v => !Equals((int)(object)v, 0) && e.HasFlag(v));
    }

    /// <summary>
    /// Append the provided context.
    /// </summary>
    /// <param name="context">The existing <see cref="WorkerJobExecutionContext"/>.</param>
    /// <param name="appendContext">The <see cref="WorkerJobExecutionContext"/> to append.</param>
    /// <returns></returns>
    public static WorkerJobExecutionContext Append(this WorkerJobExecutionContext context, WorkerJobExecutionContext appendContext)
    {
        return context | appendContext;
    }
}
