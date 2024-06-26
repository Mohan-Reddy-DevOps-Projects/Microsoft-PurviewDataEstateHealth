targetScope = 'subscription'
param containerAppIdentityPrincipalId string
param subscriptionId string
param synapseWorkspacePrincipalId string

var storageBlobDataContributorRoleDefName = 'ba92f5b4-2d11-453d-a403-e96b0029c9fe'
var storageTableDataReaderRoleDefName = '76199698-9eea-4c19-bc75-cec21354c6b6'

module processingStorageSubBlobDataContributorRoleModule 'subscriptionRoleAssignment.bicep' =  {
  name: 'processingStorageSubBlobDataContributorRoleModuleDeploy'
  params: {
    principalId: containerAppIdentityPrincipalId
    roleDefinitionName: storageBlobDataContributorRoleDefName
    subscriptionId: subscriptionId
  }
}

module processingStorageSubBlobDataContributorRoleSynapseModule 'subscriptionRoleAssignment.bicep' =  {
  name: 'processingStorageSubBlobDataContributorRoleSynapseModuleDeploy'
  params: {
    principalId: synapseWorkspacePrincipalId
    roleDefinitionName: storageBlobDataContributorRoleDefName
    subscriptionId: subscriptionId
  }
}

module processingStorageSubTableDataReaderRoleModule 'subscriptionRoleAssignment.bicep' = {
  name: 'processingStorageSubTableDataReaderRoleModuleDeploy'
  params: {
    principalId: containerAppIdentityPrincipalId
    roleDefinitionName: storageTableDataReaderRoleDefName
    subscriptionId: subscriptionId
  }
}
