// <copyright file="IRepository.cs" company="Microsoft Corporation">
// Copyright (c) Microsoft Corporation. All rights reserved.
// </copyright>

namespace Microsoft.Purview.DataEstateHealth.DHDataAccess.Repositories
{
    using Microsoft.Purview.DataEstateHealth.DHDataAccess.Repositories.Exceptions;
    using System.Collections.Generic;
    using System.Threading.Tasks;

    public interface IRepository<T>
    {
        Task<IEnumerable<T>> GetAllAsync();

        /// <summary>
        /// Asynchronously retrieves an entity of type T by its ID.
        /// </summary>
        /// <param name="id">The unique identifier of the entity to be retrieved.</param>
        /// <returns>A task that represents the asynchronous operation. The task result contains the entity of type T with the specified ID.</returns>
        /// <exception cref="DBEntityNotFoundException">Thrown if no entity with the specified ID could be found in the database.</exception>
        Task<T?> GetByIdAsync(string id);

        /// <summary>
        /// Asynchronously adds a new entity of type T to the database.
        /// </summary>
        /// <param name="entity">The entity to be added to the database.</param>
        /// <returns>A task that represents the asynchronous operation. The task result contains the added entity of type T.</returns>
        /// <exception cref="DBInvalidAffectedRowsException">Thrown if the number of affected rows is not 1.</exception>
        Task<T> AddAsync(T entity);

        /// <summary>
        /// Asynchronously adds a range of entities of type T to the database.
        /// </summary>
        /// <param name="entities">The collection of entities to be added to the database.</param>
        /// <returns>A task that represents the asynchronous operation. The task result contains the collection of added entities of type T.</returns>
        /// <exception cref="DBInvalidAffectedRowsException">Thrown if the number of affected rows is not identical to the inputed entities count.</exception>
        Task<IEnumerable<T>> AddAsync(IEnumerable<T> entities);

        /// <summary>
        /// Asynchronously updates an existing entity of type T in the database.
        /// </summary>
        /// <param name="entity">The entity to be updated.</param>
        /// <returns>A task that represents the asynchronous operation. The task result contains the updated entity of type T.</returns>
        /// <exception cref="DBEntityNotFoundException">Thrown if the entity to be updated is not found in the database.</exception>
        /// <exception cref="DBInvalidAffectedRowsException">Thrown if the number of affected rows is not 1.</exception>
        Task<T> UpdateAsync(T entity);

        /// <summary>
        /// Asynchronously deletes an existing entity of type T from the database.
        /// </summary>
        /// <param name="entity">The entity to be deleted.</param>
        /// <returns>A task that represents the asynchronous operation. The task result contains the deleted entity of type T.</returns>
        /// <exception cref="DBEntityNotFoundException">Thrown if the entity to be deleted is not found in the database.</exception>
        /// <exception cref="DBInvalidAffectedRowsException">Thrown if the number of affected rows is not 1.</exception>
        Task<T> DeleteAsync(T entity);

        /// <summary>
        /// Asynchronously deletes an entity of type T by its identifier from the database.
        /// </summary>
        /// <param name="id">The identifier of the entity to be deleted.</param>
        /// <returns>A task that represents the asynchronous operation. The task result contains the deleted entity of type T.</returns>
        /// <exception cref="DBEntityNotFoundException">Thrown if an entity with the specified identifier is not found in the database.</exception>
        /// <exception cref="DBInvalidAffectedRowsException">Thrown if the number of affected rows is not 1.</exception>
        Task<T> DeleteAsync(string id);
    }
}
