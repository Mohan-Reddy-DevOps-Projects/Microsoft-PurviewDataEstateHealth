param acrName string
param assistedIdAppObjectId string
param containerAppIdentityName string
param coreResourceGroupName string
param jobStorageAccountName string
param keyVaultName string
param location string = resourceGroup().location
param vnetName string
param synapseWorkspaceName string
param synapseStorageAccountName string
param synapseStorageAccountUrl string
param sparkPoolName string
param synapseLocation string

var keyVaultReaderRoleDefName = '21090545-7ca7-4776-b22c-e363652d74d2'
var keyVaultSecretsUserRoleDefName = '4633458b-17de-408a-b874-0445c86b69e6'
var keyVaultCertificatesOfficerRoleDefName = 'a4417e6f-fecd-4de8-b567-7b0420556985'

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

module keyVaultReaderRoleModule 'keyVaultRoleAssignment.bicep' = {
  name: 'keyVaultReaderRoleDeploy'
  params: {
    keyVaultName: keyVault.name
    principalId: containerAppIdentity.properties.principalId
    roleDefinitionName: keyVaultReaderRoleDefName
  }
}

module keyVaultSecretsUserRoleModule 'keyVaultRoleAssignment.bicep' = {
  name: 'keyVaultSecretsUserRoleDeploy'
  params: {
    keyVaultName: keyVault.name
    principalId: containerAppIdentity.properties.principalId
    roleDefinitionName: keyVaultSecretsUserRoleDefName
  }
}

// The assisted ID app needs Key Vault Certificate Officer to create certificates.
module keyVaultCertificatesOfficerRoleModule 'keyVaultRoleAssignment.bicep' = {
  name: 'keyVaultCertificatesOfficerRoleDeploy'
  params: {
    keyVaultName: keyVault.name
    principalId: assistedIdAppObjectId
    roleDefinitionName: keyVaultCertificatesOfficerRoleDefName
  }
}

module synapseStorageAccount 'synapseStorageAccount.bicep' = {
  name: 'synapseStorageAccount'
  params: {
    location: location
    storageAccountName: synapseStorageAccountName
  }
}

// resource synapseWorkspace 'Microsoft.Synapse/workspaces@2021-06-01' = {
//   dependsOn: [synapseStorageAccount]
//   name: synapseWorkspaceName
//   location: synapseLocation
//   tags: {}
//   identity: {
//     type: 'SystemAssigned'
//   }
//   properties: {
//     azureADOnlyAuthentication: false
//     publicNetworkAccess: 'Enabled'
//     managedVirtualNetwork: 'default'
//     cspWorkspaceAdminProperties: {
//     }
//     defaultDataLakeStorage: {
//       accountUrl: synapseStorageAccountUrl
//       createManagedPrivateEndpoint: true
//       filesystem: synapseStorageAccount.outputs.StorageContainerName
//       resourceId: synapseStorageAccount.outputs.StorageAccountResourceId
//     }
//     sqlAdministratorLogin: 'sqladminuser'
//     sqlAdministratorLoginPassword: null
//   }
// }

// resource sparkPool 'Microsoft.Synapse/workspaces/bigDataPools@2021-06-01' = {
//   dependsOn: [synapseWorkspace]
//   name: sparkPoolName
//   location: synapseLocation
//   tags: {}
//   parent: synapseWorkspace
//   properties: {
//     autoPause: {
//       delayInMinutes: 5
//       enabled: true
//     }
//     autoScale: {
//       enabled: true
//       minNodeCount: 3
//       maxNodeCount: 10
//     }
//     dynamicExecutorAllocation: {
//       enabled: true
//       minExecutors: 1
//       maxExecutors: 4
//     }
//     isComputeIsolationEnabled: false
//     nodeSize: 'Small'
//     nodeSizeFamily: 'MemoryOptimized'
//     sessionLevelPackagesEnabled: true
//     sparkVersion: '3.3'
//   }
// }
