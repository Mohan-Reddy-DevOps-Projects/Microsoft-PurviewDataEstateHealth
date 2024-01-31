#nullable enable
// <copyright file="IRepository.cs" company="Microsoft Corporation">
// Copyright (c) Microsoft Corporation. All rights reserved.
// </copyright>

namespace Microsoft.Purview.DataEstateHealth.DHDataAccess.Repositories
{
    using Microsoft.Purview.DataEstateHealth.DHModels.Common;
    using System;
    using System.Collections.Generic;
    using System.Threading.Tasks;

    public interface IRepository<T> where T : ContainerEntityBase
    {
        Task<IEnumerable<T>> GetAllAsync();

        Task<T?> GetByIdAsync(Guid id);

        Task AddAsync(T entity);

        Task UpdateAsync(T entity);

        Task DeleteAsync(T entity);

        Task DeleteAsync(Guid id);
    }
}
