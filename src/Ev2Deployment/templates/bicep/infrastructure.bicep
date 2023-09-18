param acrName string
param acrResourceGroupName string
param containerAppIdentityName string
param jobStorageAccountName string
param location string = resourceGroup().location

resource containerAppIdentity 'Microsoft.ManagedIdentity/userAssignedIdentities@2018-11-30' = {
  name: containerAppIdentityName
  location: location
}

module acrRoleAssignments 'acrRoleAssignments.bicep' = {
  name: 'acrRoleAssignments'
  scope: resourceGroup(acrResourceGroupName)
  params: {
    acrName: acrName
    principalId: containerAppIdentity.properties.principalId
  }
}

resource jobStorageAccount 'Microsoft.Storage/storageAccounts@2021-06-01' = {
  name: jobStorageAccountName
  location: location
  kind: 'StorageV2'
  sku: {
    name: 'Standard_GZRS'
  }
  properties: {
    accessTier: 'Hot'
    allowBlobPublicAccess: false
    supportsHttpsTrafficOnly: true
    minimumTlsVersion: 'TLS1_2'
    networkAcls: {
      defaultAction: 'Deny'
    }
  }
}

module storageRoleAssignments 'storageRoleAssignments.bicep' = {
  name: 'storageRoleAssignments'
  params: {
    storageAccountName: jobStorageAccount.name
    principalId: containerAppIdentity.properties.principalId
  }
}
