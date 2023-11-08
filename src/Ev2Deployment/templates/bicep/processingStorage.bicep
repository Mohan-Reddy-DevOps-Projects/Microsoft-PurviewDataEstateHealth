param location string = resourceGroup().location
param processingStorageAccountName string
param processingStorageTableName string

module processingStorageAccountModule 'storageAccount.bicep' = {
  name: 'processingStorageAccountDeploy'
  params: {
    location: location
    storageAccountName: processingStorageAccountName
  }
}

module processingStorageTableModule 'storageTable.bicep' = {
  name: 'processingStorageTableDeploy'
  params: {
    storageAccountName: processingStorageAccountModule.outputs.storageAccountName
    tableName: processingStorageTableName
  }
}
