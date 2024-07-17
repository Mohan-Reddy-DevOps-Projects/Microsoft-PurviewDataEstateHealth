param cosmosAccountName string
param containerAppIdentityName string
param dghResourceGroupName string

resource containerAppIdentity 'Microsoft.ManagedIdentity/userAssignedIdentities@2018-11-30' existing = {
  name: containerAppIdentityName
  scope: resourceGroup(dghResourceGroupName)
}

var controlDatabaseName = 'dgh-Control'
var actionDatabaseName = 'dgh-Action'
// Contributor role assignment
// Document: https://learn.microsoft.com/en-us/entra/identity/managed-identities-azure-resources/tutorial-vm-managed-identities-cosmos?tabs=azure-resource-manager#grant-access
var contributorRoleDefName = '00000000-0000-0000-0000-000000000002'

module controlCosmosContributorRoleAssignmentModule 'cosmosRoleAssignment.bicep' = {
  name: 'controlCosmosContributorRoleAssignmentModule'
  params: {
    accountName: cosmosAccountName
    principalId: containerAppIdentity.properties.principalId
    roleDefinitionName: contributorRoleDefName
    databaseName: controlDatabaseName
  }
  dependsOn: [
    controlCosmosDatabaseDHControl
  ]
}

module actionCosmosContributorRoleAssignmentModule 'cosmosRoleAssignment.bicep' = {
  name: 'actionCosmosContributorRoleAssignmentModule'
  params: {
    accountName: cosmosAccountName
    principalId: containerAppIdentity.properties.principalId
    roleDefinitionName: contributorRoleDefName
    databaseName: actionDatabaseName
  }
  dependsOn: [
    cosmosDatabaseDHAction
  ]
}

// Databases and containers
module controlCosmosDatabaseDHControl 'cosmosDatabase.bicep' = {
  name: 'controlCosmosDatabaseDHControl'
  params: {
    accountName: cosmosAccountName
    databaseName: 'dgh-Control'
    containerNames: ['DHControl', 'DHSchedule', 'DHControlStatusPalette', 'DHAssessment', 'DHComputingJob', 'DHAlert']
  }
}

module cosmosDatabaseDHControlScoreContainer 'cosmosDatabase.bicep' = {
  name: 'cosmosDatabaseDHControlScoreContainer'
  params: {
    accountName: cosmosAccountName
    databaseName: 'dgh-Control'
    containerNames: ['DHScore']
    throughput: 40000
  }
}

module cosmosDatabaseDHAction 'cosmosDatabase.bicep' = {
  name: 'cosmosDatabaseDHAction'
  params: {
    accountName: cosmosAccountName
    databaseName: 'dgh-Action'
    containerNames: ['DHAction']
  }
}

module cosmosDatabaseDHSettings 'cosmosDatabase.bicep' = {
  name: 'cosmosDatabaDHseSettings'
  params: {
    accountName: cosmosAccountName
    databaseName: 'dgh-Settings'
    containerNames: ['DHStorageConfig']
  }
}
