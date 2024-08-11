param accountName string
param databaseName string
param containerNames array
param containerdehNames array
param throughput int = 6000
param partitionid string
param location string = resourceGroup().location
param containerAppIdentityName string

// param resourceGrp string = resourceGroup().name

// resource account 'Microsoft.DocumentDB/databaseAccounts@2022-05-15' existing = {

resource account 'Microsoft.DocumentDB/databaseAccounts@2022-05-15'  existing = {
  name: accountName   
}

resource containerAppIdentity 'Microsoft.ManagedIdentity/userAssignedIdentities@2018-11-30' existing = {
  name: containerAppIdentityName
}



 resource database 'Microsoft.DocumentDB/databaseAccounts/sqlDatabases@2022-05-15' = {
  parent: account
  name: databaseName
  location: location
  properties: {    
    resource: {
      id: databaseName
      analyticalStorageTtl : -1
    }
    

  }
}


resource containerdeh 'Microsoft.DocumentDB/databaseAccounts/sqlDatabases/containers@2022-05-15' = [for containerName in containerdehNames: {
  parent: database
  name: containerName
  properties: {
    resource: {
      id: containerName
      analyticalStorageTtl : -1
      partitionKey: {
        paths: [
          partitionid
        ]
        kind: 'Hash'
      }
      indexingPolicy: {
        indexingMode: 'consistent'
        includedPaths: [
          {
            path: '/*'
          }
        ]
        excludedPaths: [
          {
            path: '/_etag/?'
          }
        ]
      }
    }
    options: {
      autoscaleSettings: {
        maxThroughput: throughput
      }
    }
  }
}]



resource container 'Microsoft.DocumentDB/databaseAccounts/sqlDatabases/containers@2022-05-15' = [for containerName in containerNames: {
  parent: database
  name: containerName
  properties: {
    resource: {
      id: containerName
      partitionKey: {
        paths: [
          partitionid
        ]
        kind: 'Hash'
      }
      indexingPolicy: {
        indexingMode: 'consistent'
        includedPaths: [
          {
            path: '/*'
          }
        ]
        excludedPaths: [
          {
            path: '/_etag/?'
          }
        ]
      }
    }
    options: {
      autoscaleSettings: {
        maxThroughput: throughput
      }
    }
  }
}]


