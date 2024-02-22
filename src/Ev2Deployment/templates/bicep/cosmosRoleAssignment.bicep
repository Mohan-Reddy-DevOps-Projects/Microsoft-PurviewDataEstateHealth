param accountName string
param principalId string
param roleDefinitionName string

resource databaseAccount 'Microsoft.DocumentDB/databaseAccounts@2022-05-15' existing = {
  name: accountName
}

resource sqlRoleDefinition 'Microsoft.DocumentDB/databaseAccounts/sqlRoleDefinitions@2021-04-15' existing = {
  name: roleDefinitionName
}

resource sqlRoleAssignment 'Microsoft.DocumentDB/databaseAccounts/sqlRoleAssignments@2021-04-15' = {
  // TODO: use guid
  name: roleDefinitionName
  parent: databaseAccount
  properties: {
    roleDefinitionId: sqlRoleDefinition.id
    principalId: principalId
    scope: databaseAccount.id
  }
}
