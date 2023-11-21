param eventHubNamespaceName string
param principalId string
param roleDefinitionName string

resource eventHubNamespace 'Microsoft.EventHub/Namespaces@2021-11-01' existing = {
  name: eventHubNamespaceName
}

resource roleDefinition 'Microsoft.Authorization/roleDefinitions@2022-04-01' existing = {
  scope: eventHubNamespace
  name: roleDefinitionName
}

resource roleAssignment 'Microsoft.Authorization/roleAssignments@2022-04-01' = {
  name: guid(eventHubNamespace.id, roleDefinition.id, principalId)  
  scope: eventHubNamespace
  properties: {
    roleDefinitionId: roleDefinition.id
    principalId: principalId
    principalType: 'ServicePrincipal'
  }
}
