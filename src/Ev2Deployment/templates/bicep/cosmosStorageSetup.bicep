param cosmosAccountName string
param containerAppIdentityName string
param dghResourceGroupName string
param sharedKeyVaultName string

resource containerAppIdentity 'Microsoft.ManagedIdentity/userAssignedIdentities@2018-11-30' existing = {
  name: containerAppIdentityName
  scope: resourceGroup(dghResourceGroupName)
}

var controlDatabaseName = 'dgh-Control'
var actionDatabaseName = 'dgh-Action'
var settingsDatabaseName = 'dgh-Settings'
var dehDatabaseName = 'dgh-DataEstateHealth'
var dehBackfill = 'dgh-Backfill'
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
    controlCosmosDatabaseDHControl, controlCosmosDatabaseDHControlJob
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

module settingsCosmosContributorRoleAssignmentModule 'cosmosRoleAssignment.bicep' = {
  name: 'settingsCosmosContributorRoleAssignmentModule'
  params: {
    accountName: cosmosAccountName
    principalId: containerAppIdentity.properties.principalId
    roleDefinitionName: contributorRoleDefName
    databaseName: settingsDatabaseName
  }
  dependsOn: [
    cosmosDatabaseDHSettings
  ]
}

module dehCosmosContributorRoleAssignmentModule 'cosmosRoleAssignment.bicep' = {
  name: 'dehCosmosContributorRoleAssignmentModule'
  params: {
    accountName: cosmosAccountName
    principalId: containerAppIdentity.properties.principalId
    roleDefinitionName: contributorRoleDefName
    databaseName: dehDatabaseName
  }
  dependsOn: [
    controlCosmosDatabaseDataEstateHealth
  ]
}

module dehBackfillCosmosContributorRoleAssignmentModule 'cosmosRoleAssignment.bicep' = {
  name: 'dehBackfillCosmosContributorRoleAssignmentModule'
  params: {
    accountName: cosmosAccountName
    principalId: containerAppIdentity.properties.principalId
    roleDefinitionName: contributorRoleDefName
    databaseName: dehBackfill
  }
  dependsOn: [
    cosmosDatabaseDehBackfill
  ]
}


// Databases and containers
module controlCosmosDatabaseDHControl 'cosmosDatabase.bicep' = {
  name: 'controlCosmosDatabaseDHControl'
  params: {
    accountName: cosmosAccountName
    databaseName: 'dgh-Control'
    partitionid: '/TenantId'
    containerAppIdentityName :  containerAppIdentityName
    containerNames: ['DHControl', 'DHSchedule', 'DHControlStatusPalette', 'DHAssessment', 'DHComputingJob', 'DHAlert']
    containerdehNames: []    
  }
}

// Databases and containers
module controlCosmosDatabaseDHControlJob 'cosmosDatabase.bicep' = {
  name: 'controlCosmosDatabaseDHControlJob'
  params: {
    accountName: cosmosAccountName
    databaseName: 'dgh-Control'
    partitionid: '/accountId'
    containerAppIdentityName :  containerAppIdentityName
    containerNames: []
    containerdehNames: ['DHControlJob']    
  }
}

// Databases and containers
module controlCosmosDatabaseDataEstateHealth 'cosmosDatabase.bicep' = {
  name: 'controlCosmosDatabaseDataEstateHealth'
  params: {
    accountName: cosmosAccountName
    databaseName: dehDatabaseName // 'dgh-DataEstateHealth'
    partitionid: '/accountId'
    containerAppIdentityName :  containerAppIdentityName
    containerdehNames: ['businessdomain', 'dataasset', 'dataproduct', 'dataqualityfact','dataqualityv2fact', 'datasubscription', 'policyset','relationship','term', 'dehsentinel','provisionevent','dataassetwithlineage', 'dcatalogall', 'cde','okr','keyresult','dataqualityrule','dataqualityjobmetadata','dataqualityassetdelete','dataqualityjobdelete','dataqualityschema','cdc']
    containerNames: []    
  }
}

module cosmosDatabaseDHControlScoreContainer 'cosmosDatabase.bicep' = {
  name: 'cosmosDatabaseDHControlScoreContainer'
  params: {
    accountName: cosmosAccountName
    databaseName: 'dgh-Control'
    containerNames: ['DHScore']
    partitionid: '/TenantId'
    throughput: 40000
    containerAppIdentityName :  containerAppIdentityName
    containerdehNames: []
  }
}

module cosmosDatabaseDHAction 'cosmosDatabase.bicep' = {
  name: 'cosmosDatabaseDHAction'
  params: {
    accountName: cosmosAccountName
    databaseName: 'dgh-Action'
    partitionid: '/TenantId'
    containerNames: []
    containerAppIdentityName :  containerAppIdentityName
    containerdehNames: ['DHAction']
  }
}


module cosmosDatabaseDHSettings 'cosmosDatabase.bicep' = {
  name: 'cosmosDatabaDHseSettings'
  params: {
    accountName: cosmosAccountName
    databaseName: 'dgh-Settings'
    partitionid: '/TenantId'
    containerNames: ['DHStorageConfig','DHStorageSchedule']
    containerAppIdentityName :  containerAppIdentityName
    containerdehNames: []
  }
}

module cosmosDatabaseDehBackfill 'cosmosDatabase.bicep' = {
  name: 'cosmosDatabaseDehBackfill'
  params: {
    accountName: cosmosAccountName
    databaseName: dehBackfill // 'dgh-Backfill'
    partitionid: '/accountId'
    containerAppIdentityName :  containerAppIdentityName
    containerdehNames: ['okr', 'businessdomain', 'cde', 'keyresult']
    containerNames: []
  }
}
