// <copyright file="IRepository.cs" company="Microsoft Corporation">
// Copyright (c) Microsoft Corporation. All rights reserved.
// </copyright>

namespace Microsoft.Purview.DataEstateHealth.DHDataAccess.Repositories
{
    using System.Collections.Generic;
    using System.Threading.Tasks;

    public interface IRepository<T>
    {
        /// <summary>
        /// Asynchronously retrieves all entities of type T for a specified tenant.
        /// </summary>
        /// <param name="tenantId">The tenant identifier for which entities are retrieved.</param>
        /// <returns>A task that represents the asynchronous operation. The task result contains an enumerable of entities of type T.</returns>
        Task<IEnumerable<T>> GetAllAsync(string tenantId);

        /// <summary>
        /// Asynchronously retrieves a specific entity of type T by its identifier for a given tenant.
        /// </summary>
        /// <param name="id">The unique identifier of the entity.</param>
        /// <param name="tenantId">The tenant identifier for which the entity is retrieved.</param>
        /// <returns>A task that represents the asynchronous operation. The task result contains the entity of type T with the specified ID, or null if not found.</returns>
        Task<T?> GetByIdAsync(string id, string tenantId);

        /// <summary>
        /// Asynchronously adds a new entity of type T to the repository for a specified tenant, optionally associating it with an account.
        /// </summary>
        /// <param name="entity">The entity to add.</param>
        /// <param name="tenantId">The tenant identifier under which the entity is added.</param>
        /// <param name="accountId">The optional account identifier associated with the entity.</param>
        /// <returns>A task that represents the asynchronous operation. The task result contains the added entity of type T.</returns>
        Task<T> AddAsync(T entity, string tenantId, string? accountId);

        /// <summary>  
        /// Asynchronously adds a range of entities of type T to the repository for a specified tenant, optionally associating them with an account.  
        /// </summary>  
        /// <param name="entities">The collection of entities to be added.</param>  
        /// <param name="tenantId">The tenant identifier under which the entities are added.</param>  
        /// <param name="accountId">The optional account identifier associated with the entities.</param>  
        /// <returns>A task that represents the asynchronous operation. The task result contains a tuple with the collection of successfully added entities of type T and the collection of entities that failed to add.</returns>
        Task<(IReadOnlyCollection<T> SucceededItems, IReadOnlyCollection<T> FailedItems, IReadOnlyCollection<T> IgnoredItems)> AddAsync(IReadOnlyList<T> entities, string tenantId, string? accountId);

        /// <summary>
        /// Asynchronously updates an existing entity of type T in the repository for a specified tenant, optionally associating it with an account.
        /// </summary>
        /// <param name="entity">The entity to update.</param>
        /// <param name="tenantId">The tenant identifier under which the entity is updated.</param>
        /// <param name="accountId">The optional account identifier associated with the entity.</param>
        /// <returns>A task that represents the asynchronous operation. The task result contains the updated entity of type T.</returns>
        Task<T> UpdateAsync(T entity, string tenantId, string? accountId);

        /// <summary>
        /// Asynchronously updates a range of entities of type T in the repository for a specified tenant, optionally associating them with an account.
        /// </summary>
        /// <param name="entities">The collection of entities to update.</param>
        /// <param name="tenantId">The tenant identifier under which the entities are updated.</param>
        /// <param name="accountId">The optional account identifier associated with the entities.</param>
        /// <returns>A task that represents the asynchronous operation. The task result contains a tuple with the collection of successfully updated entities of type T and the collection of entities that failed to update.</returns>
        Task<(IReadOnlyCollection<T> SucceededItems, IReadOnlyCollection<T> FailedItems)> UpdateAsync(IReadOnlyList<T> entities, string tenantId, string? accountId);

        /// <summary>
        /// Asynchronously deletes an existing entity of type T from the repository for a specified tenant.
        /// </summary>
        /// <param name="entity">The entity to delete.</param>
        /// <param name="tenantId">The tenant identifier under which the entity is deleted.</param>
        /// <returns>A task that represents the asynchronous operation. The task result is the deleted entity of type T.</returns>
        Task DeleteAsync(T entity, string tenantId);

        /// <summary>
        /// Asynchronously deletes a specific entity of type T by its identifier from the repository for a given tenant.
        /// </summary>
        /// <param name="id">The unique identifier of the entity to delete.</param>
        /// <param name="tenantId">The tenant identifier under which the entity is deleted.</param>
        /// <returns>A task that represents the asynchronous operation. The task result is the deleted entity of type T.</returns>
        Task DeleteAsync(string id, string tenantId);
    }
}
