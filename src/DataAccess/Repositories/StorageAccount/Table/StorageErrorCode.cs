// -----------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
// -----------------------------------------------------------

namespace Microsoft.Azure.Purview.DataEstateHealth.DataAccess;

internal static class StorageErrorCode
{
    // Please refer to the particular service docs if you need to add new error codes here.

    public const string EntityNotFound = "EntityNotFound";
    public const string EntityAlreadyExists = "EntityAlreadyExists";
    public const string InvalidInput = "InvalidInput";
    public const string UpdateConditionNotSatisfied = "UpdateConditionNotSatisfied";
    public const string QueueNotFound = "QueueNotFound";
}
