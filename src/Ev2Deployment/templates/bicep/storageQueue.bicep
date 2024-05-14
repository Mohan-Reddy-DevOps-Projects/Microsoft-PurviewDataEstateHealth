param storageAccountName string
param queueName string

resource storageAccount 'Microsoft.Storage/storageAccounts@2021-06-01' existing = {
  name: storageAccountName

  resource queueService 'queueServices' = {
    name: 'default'

    resource queue 'queues' = {
      name: queueName
    }
  }
}
