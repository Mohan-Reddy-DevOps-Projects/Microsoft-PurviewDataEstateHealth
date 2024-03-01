targetScope = 'subscription'
param containerAppIdentityPrincipalId string
param subscriptionId string

var contributorRoleDefName = 'b24988ac-6180-42a0-ab88-20f7382dd24c'
var storageBlobDataContributorRoleDefName = 'ba92f5b4-2d11-453d-a403-e96b0029c9fe'
var storageTableDataContributorRoleDefName = '0a9a7e1f-b9d0-4cc4-a60d-0319b160aaa3'

module processingStorageSubContributorRoleModule 'subscriptionRoleAssignment.bicep' = {
  name: 'processingStorageSubContributorRoleModuleDeploy'
  params: {
    principalId: containerAppIdentityPrincipalId
    roleDefinitionName: contributorRoleDefName
    subscriptionId: subscriptionId
  }
}

module processingStorageSynapseSubContributorRoleModule 'subscriptionRoleAssignment.bicep' = {
  name: 'processingStorageSynapseSubContributorRoleModuleDeploy'
  params: {
    principalId: containerAppIdentityPrincipalId
    roleDefinitionName: storageBlobDataContributorRoleDefName
    subscriptionId: subscriptionId
  }
}

module processingStorageSubBlobDataContributorRoleModule 'subscriptionRoleAssignment.bicep' =  {
  name: 'processingStorageSubBlobDataContributorRoleModuleDeploy'
  params: {
    principalId: containerAppIdentityPrincipalId
    roleDefinitionName: storageBlobDataContributorRoleDefName
    subscriptionId: subscriptionId
  }
}

module processingStorageSubTableDataContributorRoleModule 'subscriptionRoleAssignment.bicep' = {
  name: 'processingStorageSubTableDataContributorRoleModuleDeploy'
  params: {
    principalId: containerAppIdentityPrincipalId
    roleDefinitionName: storageTableDataContributorRoleDefName
    subscriptionId: subscriptionId
  }
}
