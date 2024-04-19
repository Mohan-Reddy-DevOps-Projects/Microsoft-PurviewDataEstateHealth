param eventHubNamespaceName string
param eventHubName string
param partitionCount int
param messageRetentionDays int

resource eventHubNamespace 'Microsoft.EventHub/namespaces@2022-10-01-preview' existing = {
  name: eventHubNamespaceName
}

resource eventHub 'Microsoft.EventHub/namespaces/eventhubs@2022-10-01-preview' = {
  name: eventHubName
  parent: eventHubNamespace
  properties: {
    partitionCount: partitionCount
    messageRetentionInDays: messageRetentionDays
  }
}
