param subscriptionId string = subscription().subscriptionId
param dghResourceGroupName string 
param streamName string
param contributorGuid string 
param principalId  string 

param userAssignedIdentity string

resource streamingJob 'Microsoft.StreamAnalytics/streamingjobs@2021-10-01-preview' existing = {
  name: streamName
}

// resource dehresourcegroup 'Microsoft.Resources/resourceGroups@2022-09-01' existing = {
//   name: dghResourceGroupName
// }

resource streamAnalyticsContributorRoleDefinition 'Microsoft.Authorization/roleDefinitions@2022-04-01' existing = {
  scope: streamingJob// dehresourcegroup
  name: contributorGuid
}

resource roleAssignment 'Microsoft.Authorization/roleAssignments@2020-04-01-preview'  = {
  name: guid(streamAnalyticsContributorRoleDefinition.id, 'containerAppIdentity.name', 'ContributorRoleAssignment') // guid(stream.StreamName, containerAppIdentity.properties.principalId, 'ContributorRoleAssignment')
  scope: streamingJob  // dehresourcegroup
  properties: {
    principalId: principalId 
    roleDefinitionId: streamAnalyticsContributorRoleDefinition.id
    principalType: 'ServicePrincipal'
  }
}
