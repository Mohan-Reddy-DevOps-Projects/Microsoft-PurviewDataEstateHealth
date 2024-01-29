namespace Microsoft.Azure.Purview.DataEstateHealth.DataAccess;

using Microsoft.DGP.ServiceBasics.DataAccess;

internal interface IDataAcessCache<TEntity, in TEntityLocator> :
     IGetSingleOperation<TEntity, TEntityLocator>,
     IDeleteOperation<TEntityLocator>
{
}
