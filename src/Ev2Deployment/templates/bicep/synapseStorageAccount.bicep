param location string
param storageAccountName string
param fileSystem string = 'dghsynapse'

resource synapseStorageAccount 'Microsoft.Storage/storageAccounts@2021-06-01' = {
  name: storageAccountName
  location: location
  kind: 'StorageV2'
  sku: {
    name: location == 'canadaeast' || location == 'centraluseuap' || location == 'westcentralus' || location == 'westus' || location == 'centralindia' ? 'Standard_GRS' : 'Standard_GZRS'
  }
  properties: {
    minimumTlsVersion: 'TLS1_2'
    isHnsEnabled: true
    allowSharedKeyAccess: false
    isLocalUserEnabled: false
    allowBlobPublicAccess: false
  }
}

resource blob 'Microsoft.Storage/storageAccounts/blobServices@2021-09-01' = {
  parent: synapseStorageAccount
  name:  'default'
}

resource containera 'Microsoft.Storage/storageAccounts/blobServices/containers@2021-09-01' = {
  name: '${synapseStorageAccount.name}/default/${fileSystem}'
  properties: {
    publicAccess: 'None'
  }
} 

output StorageContainerName string = fileSystem
output StorageAccountResourceId string = resourceId('Microsoft.Storage/storageAccounts', storageAccountName)
