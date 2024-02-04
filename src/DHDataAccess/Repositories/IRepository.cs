// <copyright file="IRepository.cs" company="Microsoft Corporation">
// Copyright (c) Microsoft Corporation. All rights reserved.
// </copyright>

namespace Microsoft.Purview.DataEstateHealth.DHDataAccess.Repositories
{
    using Microsoft.Purview.DataEstateHealth.DHModels.Common;
    using System.Collections.Generic;
    using System.Threading.Tasks;

    public interface IRepository<T> where T : IContainerEntityWrapper
    {
        Task<IEnumerable<T>> GetAllAsync();

        Task<T?> GetByIdAsync(string id);

        Task AddAsync(T entity);

        Task AddAsync(IEnumerable<T> entities);

        Task UpdateAsync(T entity);

        Task DeleteAsync(T entity);

        Task DeleteAsync(string id);
    }
}
