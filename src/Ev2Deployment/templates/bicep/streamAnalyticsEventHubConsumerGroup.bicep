param eventHubNamespacename string
param eventHubName string
param consumerGroupName string

// param catalogResourceGroupName string 
// param catalogSubscriptionId string


// resource existingResourceGroup 'Microsoft.Resources/resourceGroups@2021-04-01' existing = { 
//   name: catalogResourceGroupName
//   subscriptionId : catalogSubscriptionId
// }

resource eventHubNamespace 'Microsoft.EventHub/namespaces@2022-10-01-preview' existing = {
  name: eventHubNamespacename
}

resource eventHub 'Microsoft.EventHub/namespaces/eventhubs@2022-10-01-preview' existing = {
  name: eventHubName
  parent: eventHubNamespace
}

resource roleAssignment 'Microsoft.EventHub/namespaces/eventhubs/consumergroups@2022-10-01-preview' = {
  name: consumerGroupName
  parent: eventHub
}
