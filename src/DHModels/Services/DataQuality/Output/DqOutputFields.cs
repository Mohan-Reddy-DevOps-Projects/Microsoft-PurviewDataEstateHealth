﻿// -----------------------------------------------------------------------
// <copyright file="DqOutputFields.cs" company="Microsoft Corporation">
//        Copyright (c) Microsoft Corporation. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------



namespace Microsoft.Purview.DataEstateHealth.DHModels.Services.DataQuality.Output
{
    public static class DqOutputFields
    {
        public const string DP_ID = "DataProductID";
        public const string DP_NAME = "DataProductDisplayName";
        public const string DP_STATUS = "DataProductStatusDisplayName";
        public const string BD_ID = "BusinessDomainId";
        public const string DP_OWNER_IDS = "DataProductOwnerIds";
        
        public const string keyBdCdeCount = "BusinessDomainCriticalDataElementCount";
    }
}
