param coreResourceGroupName string
param keyVaultName  string
param cosmosAccountName string

// var resourceID = cosmosUpdateReadkeytoKV.outputs.resourceID
resource account 'Microsoft.DocumentDB/databaseAccounts@2022-05-15'  existing = {
  name: cosmosAccountName
  scope :  resourceGroup(coreResourceGroupName)
}

resource keyVault 'Microsoft.KeyVault/vaults@2021-10-01' existing = {
  name : keyVaultName   
}

resource secretReadOnlyKey 'Microsoft.KeyVault/vaults/secrets@2021-10-01' = {
  parent: keyVault
  name: 'cosmosDBReadonlykey'
  properties: {
    value: account.listKeys().primaryReadonlyMasterKey
  }
}

resource secretWriteKey 'Microsoft.KeyVault/vaults/secrets@2021-10-01' = {
  parent: keyVault
  name: 'cosmosDBWritekey'
  properties: {
    value: account.listKeys().primaryMasterKey
  }
}

resource secretCOSMOSDBName 'Microsoft.KeyVault/vaults/secrets@2021-10-01' = {
  parent: keyVault
  name: 'cosmosDBName'
  properties: {
    value: account.name
  }
}


resource secretCOSMOSDBEndpoint 'Microsoft.KeyVault/vaults/secrets@2021-10-01' = {
  parent: keyVault
  name: 'cosmosDBEndpoint'
  properties: {
    value: account.properties.documentEndpoint
  }
}
