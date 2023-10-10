param keyVaultName string
param principalId string

resource keyVault 'Microsoft.KeyVault/vaults@2023-02-01' existing = {
  name: keyVaultName
}

resource keyVaultReaderRoleDefinition 'Microsoft.Authorization/roleDefinitions@2022-04-01' existing = {
  scope: keyVault
  name: '21090545-7ca7-4776-b22c-e363652d74d2'
}

resource keyVaultSecretesUserRoleDefinition 'Microsoft.Authorization/roleDefinitions@2022-04-01' existing = {
  scope: keyVault
  name: '4633458b-17de-408a-b874-0445c86b69e6'
}

resource keyVaultReaderRoleAssignment 'Microsoft.Authorization/roleAssignments@2022-04-01' = {
  name: guid(keyVault.id, keyVaultReaderRoleDefinition.id, principalId)
  properties: {
    roleDefinitionId: keyVaultReaderRoleDefinition.id
    principalId: principalId
    principalType: 'ServicePrincipal'
  }
}

resource keyVaultSecretesUserRoleAssignment 'Microsoft.Authorization/roleAssignments@2022-04-01' = {
  name: guid(keyVault.id, keyVaultSecretesUserRoleDefinition.id, principalId)
  properties: {
    roleDefinitionId: keyVaultSecretesUserRoleDefinition.id
    principalId: principalId
    principalType: 'ServicePrincipal'
  }
}
