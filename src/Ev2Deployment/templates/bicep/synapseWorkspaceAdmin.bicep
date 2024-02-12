param synapseWorkspaceName string
param synapseAdminObjectId string
param synapseAdminDisplayName string

resource synapseWorkspace 'Microsoft.Synapse/workspaces@2021-06-01' existing = {
  name: synapseWorkspaceName
}

resource synapseWorkspaceAdmin 'Microsoft.Synapse/workspaces/administrators@2021-06-01' = {
  name: 'activeDirectory'
  parent: synapseWorkspace
  properties: {
    administratorType: 'Enterprise application'
    login: synapseAdminDisplayName
    sid: synapseAdminObjectId
    tenantId: tenant().tenantId
  }
}
