param acrName string
param containerAppIdentityName string
param coreResourceGroupName string
param jobStorageAccountName string
param keyVaultName string
param location string = resourceGroup().location
param vnetName string

resource containerAppIdentity 'Microsoft.ManagedIdentity/userAssignedIdentities@2018-11-30' = {
  name: containerAppIdentityName
  location: location
}

module acrRoleAssignments 'acrRoleAssignments.bicep' = {
  name: 'acrRoleAssignments'
  scope: resourceGroup(coreResourceGroupName)
  params: {
    acrName: acrName
    principalId: containerAppIdentity.properties.principalId
  }
}

resource vnet 'Microsoft.Network/virtualNetworks@2023-04-01' existing = {
  name: vnetName
  scope: resourceGroup(coreResourceGroupName)
}

module jobStorageAccount 'storageAccount.bicep' = {
  name: 'jobStorageAccount'
  params: {
    location: location
    storageAccountName: jobStorageAccountName
    subnetId: vnet.properties.subnets[0].id
  }
}

module storageRoleAssignments 'storageRoleAssignments.bicep' = {
  dependsOn: [jobStorageAccount]
  name: 'storageRoleAssignments'
  params: {
    storageAccountName: jobStorageAccountName
    principalId: containerAppIdentity.properties.principalId
  }
}

resource keyVault 'Microsoft.KeyVault/vaults@2023-02-01' = {
  location: location
  name: keyVaultName
  properties: {
    sku: {
      family: 'A'
      name: 'standard'
    }
    tenantId: tenant().tenantId
    enableRbacAuthorization: true
  }
}

module keyVaultRoleAssignments 'keyVaultRoleAssignments.bicep' = {
  dependsOn: [keyVault]
  name: 'keyVaultRoleAssignments'
  params: {
    keyVaultName: keyVaultName
    principalId: containerAppIdentity.properties.principalId
  }
}
