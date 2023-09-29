param keyVaultName string
param principalId string

resource keyVault 'Microsoft.KeyVault/vaults@2023-02-01' existing = {
  name: keyVaultName
}

resource keyVaultSecretesUserRoleDefinition 'Microsoft.Authorization/roleDefinitions@2022-04-01' existing = {
  scope: keyVault
  name: '4633458b-17de-408a-b874-0445c86b69e6'
}

resource keyVaultSecretesUserRoleAssignment 'Microsoft.Authorization/roleAssignments@2022-04-01' = {
  name: guid(keyVault.id, keyVaultSecretesUserRoleDefinition.id, principalId)
  properties: {
    roleDefinitionId: keyVaultSecretesUserRoleDefinition.id
    principalId: principalId
    principalType: 'ServicePrincipal'
  }
}
