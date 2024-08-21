// <copyright file="IDHStorageConnectionTest.cs" company="Microsoft Corporation">
// Copyright (c) Microsoft Corporation. All rights reserved.
// </copyright>

namespace Microsoft.Purview.DataEstateHealth.BusinessLogic.Services
{
    using Microsoft.Purview.DataEstateHealth.DHModels.Services.Control.StorageConfig;
    using System.Threading.Tasks;

    public interface IDHStorageConnectionTest<T> where T : DHStorageConfigBaseWrapper
    {
        public Task TestConnection(T entity, string token);
    }
}
