param eventHubNamespaceName string
param eventHubName string
param consumerGroupName string

resource eventHubNamespace 'Microsoft.EventHub/namespaces@2022-10-01-preview' existing = {
  name: eventHubNamespaceName
}

resource eventHub 'Microsoft.EventHub/namespaces/eventhubs@2022-10-01-preview' = {
  name: eventHubName
  parent: eventHubNamespace
}

resource roleAssignment 'Microsoft.EventHub/namespaces/eventhubs/consumergroups@2022-10-01-preview' = {
  name: consumerGroupName
  parent: eventHub
}
