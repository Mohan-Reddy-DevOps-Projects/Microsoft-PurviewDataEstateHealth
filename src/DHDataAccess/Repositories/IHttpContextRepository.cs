// <copyright file="IRepository.cs" company="Microsoft Corporation">
// Copyright (c) Microsoft Corporation. All rights reserved.
// </copyright>

namespace Microsoft.Purview.DataEstateHealth.DHDataAccess.Repositories
{
    using System.Collections.Generic;
    using System.Threading.Tasks;

    public interface IHttpContextRepository<T>
    {
        /// <summary>
        /// Asynchronously retrieves all entities of type T.
        /// </summary>
        /// <returns>A task that represents the asynchronous operation. The task result contains an enumerable of all entities of type T.</returns>
        Task<IEnumerable<T>> GetAllAsync();

        /// <summary>
        /// Asynchronously retrieves a specific entity of type T by its identifier.
        /// </summary>
        /// <param name="id">The unique identifier of the entity to retrieve.</param>
        /// <returns>A task that represents the asynchronous operation. The task result contains the entity of type T with the specified ID, or null if no such entity exists.</returns>
        Task<T?> GetByIdAsync(string id);

        /// <summary>
        /// Asynchronously adds a new entity of type T to the repository.
        /// </summary>
        /// <param name="entity">The entity to add.</param>
        /// <returns>A task that represents the asynchronous operation. The task result contains the added entity of type T.</returns>
        Task<T> AddAsync(T entity);

        /// <summary>
        /// Asynchronously adds a range of new entities of type T to the repository.
        /// </summary>
        /// <param name="entities">The collection of entities to add.</param>
        /// <returns>A task that represents the asynchronous operation. The task result contains a tuple with the collection of successfully added entities of type T and the collection of entities that failed to add.</returns>
        Task<(IReadOnlyCollection<T> SucceededItems, IReadOnlyCollection<T> FailedItems)> AddAsync(IReadOnlyList<T> entities);

        /// <summary>
        /// Asynchronously updates an existing entity of type T in the repository.
        /// </summary>
        /// <param name="entity">The entity to update.</param>
        /// <returns>A task that represents the asynchronous operation. The task result contains the updated entity of type T.</returns>
        Task<T> UpdateAsync(T entity);

        /// <summary>
        /// Asynchronously updates a range of existing entities of type T in the repository.
        /// </summary>
        /// <param name="entities">The collection of entities to update.</param>
        /// <returns>A task that represents the asynchronous operation. The task result contains a tuple with the collection of successfully updated entities of type T and the collection of entities that failed to update.</returns>
        Task<(IReadOnlyCollection<T> SucceededItems, IReadOnlyCollection<T> FailedItems)> UpdateAsync(IReadOnlyList<T> entities);

        /// <summary>
        /// Asynchronously deletes an existing entity of type T from the repository.
        /// </summary>
        /// <param name="entity">The entity to delete.</param>
        /// <returns>A task that represents the asynchronous operation. The task result contains the deleted entity of type T.</returns>
        Task DeleteAsync(T entity);

        /// <summary>
        /// Asynchronously deletes a specific entity of type T by its identifier from the repository.
        /// </summary>
        /// <param name="id">The unique identifier of the entity to delete.</param>
        /// <returns>A task that represents the asynchronous operation. The task result contains the deleted entity of type T, or null if no such entity was found.</returns>
        Task DeleteAsync(string id);
    }
}
