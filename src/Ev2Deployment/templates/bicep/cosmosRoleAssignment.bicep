param accountName string
param databaseName string
param principalId string
param roleDefinitionName string

resource databaseAccount 'Microsoft.DocumentDB/databaseAccounts@2022-05-15' existing = {
  name: accountName
}

resource sqlRoleDefinition 'Microsoft.DocumentDB/databaseAccounts/sqlRoleDefinitions@2021-04-15' existing = {
  name: roleDefinitionName
  parent: databaseAccount
}

resource sqlRoleAssignment 'Microsoft.DocumentDB/databaseAccounts/sqlRoleAssignments@2021-04-15' = {
  name: guid(roleDefinitionName, principalId, databaseAccount.id, databaseName )
  parent: databaseAccount
  properties: {
    roleDefinitionId: sqlRoleDefinition.id
    principalId: principalId
    scope: '${databaseAccount.id}/dbs/${databaseName}'
  }
}
