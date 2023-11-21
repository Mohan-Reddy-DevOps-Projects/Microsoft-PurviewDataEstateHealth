param keyVaultName string
param principalId string
param roleDefinitionName string

resource keyVault 'Microsoft.KeyVault/vaults@2023-02-01' existing = {
  name: keyVaultName
}

resource keyVaultRoleDefinition 'Microsoft.Authorization/roleDefinitions@2022-04-01' existing = {
  scope: keyVault
  name: roleDefinitionName
}

resource keyVaultRoleAssignment 'Microsoft.Authorization/roleAssignments@2022-04-01' = {
  name: guid(keyVault.id, keyVaultRoleDefinition.id, principalId)
  scope: keyVault
  properties: {
    roleDefinitionId: keyVaultRoleDefinition.id
    principalId: principalId
    principalType: 'ServicePrincipal'
  }
}
