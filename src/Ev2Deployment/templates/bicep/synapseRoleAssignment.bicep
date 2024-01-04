param synapseWorkspaceName string
param principalId string
param roleDefinitionName string

resource synapseWorkspace 'Microsoft.Synapse/workspaces@2021-06-01' existing = {
  name: synapseWorkspaceName
}

resource synapseRoleDefinition 'Microsoft.Authorization/roleDefinitions@2022-04-01' existing = {
  scope: synapseWorkspace
  name: roleDefinitionName
}

resource keyVaultRoleAssignment 'Microsoft.Authorization/roleAssignments@2022-04-01' = {
  name: guid(synapseWorkspace.id, synapseRoleDefinition.id, principalId)
  scope: synapseWorkspace
  properties: {
    roleDefinitionId: synapseRoleDefinition.id
    principalId: principalId
    principalType: 'ServicePrincipal'
  }
}
