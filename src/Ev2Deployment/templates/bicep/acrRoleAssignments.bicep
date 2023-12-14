param acrName string
param principalId string

resource acr 'Microsoft.ContainerRegistry/registries@2022-12-01' existing = {
  name: acrName
}

resource acrPullRoleDefinition 'Microsoft.Authorization/roleDefinitions@2022-04-01' existing = {
  scope: acr
  name: '7f951dda-4ed3-4680-a7ca-43fe172d538d'
}

resource acrPushRoleDefinition 'Microsoft.Authorization/roleDefinitions@2022-04-01' existing = {
  scope: acr
  name: '8311e382-0749-4cb8-b61a-304f252e45ec'
}

resource acrPullRoleAssignment 'Microsoft.Authorization/roleAssignments@2022-04-01' = {
  name: guid(acr.id, acrPullRoleDefinition.id, principalId)
  scope: acr
  properties: {
    roleDefinitionId: acrPullRoleDefinition.id
    principalId: principalId
    principalType: 'ServicePrincipal'
  }
}

resource acrPushRoleAssignment 'Microsoft.Authorization/roleAssignments@2022-04-01' = {
  name: guid(acr.id, acrPushRoleDefinition.id, principalId)
  scope: acr
  properties: {
    roleDefinitionId: acrPushRoleDefinition.id
    principalId: principalId
    principalType: 'ServicePrincipal'
  }
}


