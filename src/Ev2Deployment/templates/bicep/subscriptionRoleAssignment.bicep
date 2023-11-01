targetScope = 'subscription'

param principalId string
param roleDefinitionName string
param subscriptionId string

resource subscriptionRoleDefinition 'Microsoft.Authorization/roleDefinitions@2022-04-01' existing = {
  name: roleDefinitionName
}

resource subscriptionRoleAssignment 'Microsoft.Authorization/roleAssignments@2022-04-01' = {
  name: guid(subscriptionId, subscriptionRoleDefinition.id, principalId)
  properties: {
    roleDefinitionId: subscriptionRoleDefinition.id
    principalId: principalId
    principalType: 'ServicePrincipal'
  }
}
