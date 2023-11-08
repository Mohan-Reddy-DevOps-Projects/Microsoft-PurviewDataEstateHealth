param storageAccountName string
param tableName string

resource storageAccount 'Microsoft.Storage/storageAccounts@2021-06-01' existing = {
  name: storageAccountName

  resource tableService 'tableServices' = {
    name: 'default'

    resource table 'tables' = {
      name: tableName
    }
  }
}
