param principalId string
param storageAccountName string

resource storageAccount 'Microsoft.Storage/storageAccounts@2021-06-01' existing = {
  name: storageAccountName
}

resource storageQueueDataContributorRoleDefinition 'Microsoft.Authorization/roleDefinitions@2022-04-01' existing = {
  scope: storageAccount
  name: '974c5e8b-45b9-4653-ba55-5f855dd0fb88'
}

resource storageTableDataContributorRoleDefinition 'Microsoft.Authorization/roleDefinitions@2022-04-01' existing = {
  scope: storageAccount
  name: '0a9a7e1f-b9d0-4cc4-a60d-0319b160aaa3'
}

resource storageQueueDataContributorRoleAssignment 'Microsoft.Authorization/roleAssignments@2022-04-01' = {
  name: guid(storageAccount.id, storageQueueDataContributorRoleDefinition.id, principalId)
  properties: {
    roleDefinitionId: storageQueueDataContributorRoleDefinition.id
    principalId: principalId
    principalType: 'ServicePrincipal'
  }
}

resource storageTableDataContributorRoleAssignment 'Microsoft.Authorization/roleAssignments@2022-04-01' = {
  name: guid(storageAccount.id, storageTableDataContributorRoleDefinition.id, principalId)
  properties: {
    roleDefinitionId: storageTableDataContributorRoleDefinition.id
    principalId: principalId
    principalType: 'ServicePrincipal'
  }
}
