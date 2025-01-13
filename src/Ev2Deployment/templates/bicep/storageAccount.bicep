param location string
param storageAccountName string
param commonSubnetId string = ''
param deploySubnetId string = ''

resource storageAccount 'Microsoft.Storage/storageAccounts@2021-06-01' = {
  name: storageAccountName
  location: location
  kind: 'StorageV2'
  // Remove sku configuration until all the storage accounts are migrated to GRS or GZRS through geneva actions
  /*sku: {
    // ZRS SKUs aren't available in some regions.
    name: location == 'canadaeast' || location == 'centraluseuap' || location == 'westcentralus' || location == 'westus' || location == 'centralindia' ? 'Standard_GRS' : 'Standard_GZRS'
  }*/
  properties: {
    accessTier: 'Hot'
    allowBlobPublicAccess: false
    supportsHttpsTrafficOnly: true
    allowSharedKeyAccess: false
    isLocalUserEnabled: false
    minimumTlsVersion: 'TLS1_2'
    networkAcls: !empty(commonSubnetId) ? {
      defaultAction: 'Deny'
      virtualNetworkRules: [
        {
          action: 'Allow'
          id: commonSubnetId
        }
        {
          action: 'Allow'
          id: deploySubnetId
        }
      ]
    } : null
  }
}

output storageAccountName string = storageAccount.name
