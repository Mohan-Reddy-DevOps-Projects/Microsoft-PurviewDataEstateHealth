param cosmosAccountName string

// TODO fix role assignment
/*
param containerAppIdentityName string

resource containerAppIdentity 'Microsoft.ManagedIdentity/userAssignedIdentities@2018-11-30' existing = {
  name: containerAppIdentityName
}

// Contributor role assignment
// Document: https://learn.microsoft.com/en-us/entra/identity/managed-identities-azure-resources/tutorial-vm-managed-identities-cosmos?tabs=azure-resource-manager#grant-access
var contributorRoleDefName = '00000000-0000-0000-0000-000000000002'

module cosmosContributorRoleAssignmentModule 'cosmosRoleAssignment.bicep' = {
  name: 'cosmosContributorRoleAssignmentModule'
  params: {
    accountName: cosmosAccountName
    principalId: containerAppIdentity.properties.principalId
    roleDefinitionName: contributorRoleDefName
  }
}
*/

// Databases and containers
module cosmosDatabaseDHControl 'cosmosDatabase.bicep' = {
  name: 'cosmosDatabaseDHControl'
  params: {
    accountName: cosmosAccountName
    databaseName: 'DHControl'
    containerNames: ['DHControl','DHScore', 'DHSchedule', 'DHControlStatusPalette', 'MQAssessment']
  }
}

module cosmosDatabaseDHAction 'cosmosDatabase.bicep' = {
  name: 'cosmosDatabaseDHAction'
  params: {
    accountName: cosmosAccountName
    databaseName: 'DHAction'
    containerNames: ['DHAction']
  }
}

module cosmosDatabaseDHMonitoring 'cosmosDatabase.bicep' = {
  name: 'cosmosDatabaseDHMonitoring'
  params: {
    accountName: cosmosAccountName
    databaseName: 'DHMonitoring'
    containerNames: ['DHComputingJob']
  }
}
