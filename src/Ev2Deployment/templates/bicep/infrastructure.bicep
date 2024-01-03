param acrName string
param assistedIdAppObjectId string
param catalogResourceGroupName string
param catalogSubscriptionId string
param commonStorageAccountName string
param containerAppIdentityName string
param coreResourceGroupName string
param eventHubNamespaceName string
param jobStorageAccountName string
param keyVaultName string
param location string = resourceGroup().location
param processingStorageSubscriptions array
param vnetName string
param synapseWorkspaceName string
param synapseStorageAccountName string
param synapseStorageAccountUrl string
param sparkPoolTableName string
param synapseLocation string

var contributorRoleDefName = 'b24988ac-6180-42a0-ab88-20f7382dd24c'
var azureEventHubsDataReceiverRoleDefName = 'a638d3c7-ab3a-418d-83e6-5f17a39d4fde'
var keyVaultReaderRoleDefName = '21090545-7ca7-4776-b22c-e363652d74d2'
var keyVaultSecretsUserRoleDefName = '4633458b-17de-408a-b874-0445c86b69e6'
var keyVaultCertificatesOfficerRoleDefName = 'a4417e6f-fecd-4de8-b567-7b0420556985'
var storageBlobDataContributorRoleDefName = 'ba92f5b4-2d11-453d-a403-e96b0029c9fe'
var storageTableDataContributorRoleDefName = '0a9a7e1f-b9d0-4cc4-a60d-0319b160aaa3'
var ownerRoleDefName = '8e3af657-a8ff-443c-a75c-2fe8c4bcb635'

resource containerAppIdentity 'Microsoft.ManagedIdentity/userAssignedIdentities@2018-11-30' = {
  name: containerAppIdentityName
  location: location
}

resource acr 'Microsoft.ContainerRegistry/registries@2022-12-01' = {
  name: toLower(acrName)
  location: location
  sku: {
    name: 'Premium'
  }
  properties: {
    adminUserEnabled: true
    policies: {
      quarantinePolicy: {
        status: 'disabled'
      }
      trustPolicy: {
        type: 'Notary'
        status: 'disabled'
      }
      retentionPolicy: {
        days: 7
        status: 'enabled'
      }
    }
    dataEndpointEnabled: false
    publicNetworkAccess: 'Enabled'
    zoneRedundancy: 'Disabled'
  }
}

module acrRoleAssignments 'acrRoleAssignments.bicep' = {
  name: 'acrRoleAssignments'
  params: {
    acrName: acr.name
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
    commonSubnetId: vnet.properties.subnets[0].id
    deploySubnetId: vnet.properties.subnets[1].id
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

module synapseStorageRoleAssignments 'synapseStorageRoleAssignments.bicep' = {
  dependsOn: [synapseStorageAccount]
  name: 'synapseStorageRoleAssignments'
  params: {
    storageAccountName: synapseStorageAccountName
    principalId: containerAppIdentity.properties.principalId
  }
}

module commonStorageAccountModule 'storageAccount.bicep' = {
  name: 'commonStorageAccountDeploy'
  params: {
    location: location
    storageAccountName: commonStorageAccountName
    commonSubnetId: vnet.properties.subnets[0].id
    deploySubnetId: vnet.properties.subnets[1].id
  }
}

module sparkPoolTableModule 'storageTable.bicep' = {
  name: 'sparkPoolTableDeploy'
  params: {
    storageAccountName: commonStorageAccountModule.outputs.storageAccountName
    tableName: sparkPoolTableName
  }
}

module commonStorageAccountRoleAssignmentsModule 'storageRoleAssignments.bicep' = {
  name: 'commonStorageAccountRoleAssignmentsDeploy'
  params: {
    storageAccountName: commonStorageAccountModule.outputs.storageAccountName
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

resource synapseWorkspace 'Microsoft.Synapse/workspaces@2021-06-01' = {
  name: synapseWorkspaceName
  location: synapseLocation
  tags: {}
  identity: {
    type: 'SystemAssigned'
  }
  properties: {
    azureADOnlyAuthentication: false
    publicNetworkAccess: 'Enabled'
    managedVirtualNetwork: 'default'
    defaultDataLakeStorage: {
      accountUrl: synapseStorageAccountUrl
      createManagedPrivateEndpoint: true
      filesystem: synapseStorageAccount.outputs.StorageContainerName
      resourceId: synapseStorageAccount.outputs.StorageAccountResourceId
    }
    sqlAdministratorLogin: 'sqladminuser'
    sqlAdministratorLoginPassword: null
  }
}

resource synapseWorkspaceAdmin 'Microsoft.Synapse/workspaces/administrators@2021-06-01' = {
  name: 'activeDirectory'
  parent: synapseWorkspace
  properties: {
    administratorType: 'Enterprise application'
    login: 'PDG Buildout App'
    sid: assistedIdAppObjectId
    tenantId: tenant().tenantId
  }
}

resource synapseAllowEv2 'Microsoft.Synapse/workspaces/firewallRules@2021-06-01' = {
  name: 'AllowAllWindowsAzureIps'
  parent: synapseWorkspace
  properties: {
    endIpAddress: '0.0.0.0'
    startIpAddress: '0.0.0.0'
  }
}

resource synapseOwnerRoleDef 'Microsoft.Authorization/roleDefinitions@2022-04-01' existing = {
  scope: synapseWorkspace
  name: ownerRoleDefName
}

resource acrPullRoleAssignment 'Microsoft.Authorization/roleAssignments@2022-04-01' = {
  name: guid(synapseWorkspaceName, ownerRoleDefName, assistedIdAppObjectId)
  scope: synapseWorkspace
  properties: {
    roleDefinitionId: synapseOwnerRoleDef.id
    principalId: assistedIdAppObjectId
    principalType: 'ServicePrincipal'
  }
}


module processingStorageSubContributorRoleModule 'subscriptionRoleAssignment.bicep' = [for processingStorageSubscription in processingStorageSubscriptions: {
  name: 'processingStorageSubContributorRoleModuleDeploy_${processingStorageSubscription.stamp}'
  scope: subscription(processingStorageSubscription.id)
  params: {
    principalId: containerAppIdentity.properties.principalId
    roleDefinitionName: contributorRoleDefName
    subscriptionId: processingStorageSubscription.id
  }
}]

module processingStorageSubBlobDataContributorRoleModule 'subscriptionRoleAssignment.bicep' = [for processingStorageSubscription in processingStorageSubscriptions: {
  name: 'processingStorageSubBlobDataContributorRoleModuleDeploy_${processingStorageSubscription.stamp}'
  scope: subscription(processingStorageSubscription.id)
  params: {
    principalId: containerAppIdentity.properties.principalId
    roleDefinitionName: storageBlobDataContributorRoleDefName
    subscriptionId: processingStorageSubscription.id
  }
}]

module processingStorageSubTableDataContributorRoleModule 'subscriptionRoleAssignment.bicep' = [for processingStorageSubscription in processingStorageSubscriptions: {
  name: 'processingStorageSubTableDataContributorRoleModuleDeploy_${processingStorageSubscription.stamp}'
  scope: subscription(processingStorageSubscription.id)
  params: {
    principalId: containerAppIdentity.properties.principalId
    roleDefinitionName: storageTableDataContributorRoleDefName
    subscriptionId: processingStorageSubscription.id
  }
}]

module eventHubNamespaceRoleModule 'eventHubNamespaceRoleAssignment.bicep' = {
  name: 'eventHubNamespaceRoleDeploy'
  scope: resourceGroup(catalogSubscriptionId, catalogResourceGroupName)
  params: {
    eventHubNamespaceName: eventHubNamespaceName
    principalId: containerAppIdentity.properties.principalId
    roleDefinitionName: azureEventHubsDataReceiverRoleDefName
  }
}

output containerAppIdentityObjectId string = containerAppIdentity.properties.principalId
