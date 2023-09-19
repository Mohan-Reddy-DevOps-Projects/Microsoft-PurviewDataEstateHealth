param acaEnvironmentName string
param acaResourceGroupName string
param acrName string
param appSettingsJson string
param containerAppIdentityName string
param containerAppName string
param imageName string
param imageTagName string
param location string = resourceGroup().location
param maxReplicas int = 10
param minReplicas int = 1

resource acaEnvironment 'Microsoft.App/managedEnvironments@2023-05-01' existing = {
  name: acaEnvironmentName
  scope: resourceGroup(acaResourceGroupName)
}

resource acr 'Microsoft.ContainerRegistry/registries@2022-12-01' existing = {
  name: acrName
  // The ACR is in the same RG as the ACA.
  scope: resourceGroup(acaResourceGroupName)
}

resource containerAppIdentity 'Microsoft.ManagedIdentity/userAssignedIdentities@2018-11-30' existing = {
  name: containerAppIdentityName
}

resource containerApp 'Microsoft.App/containerApps@2022-11-01-preview' = {
  name: containerAppName
  location: location
  identity: {
    type: 'UserAssigned'
    userAssignedIdentities: {
      '${containerAppIdentity.id}' : {}
    }
  }
  properties: {
    managedEnvironmentId: acaEnvironment.id
    configuration: {
      ingress: {
        external: true
        targetPort: 80
      }
      registries: [
        {
          identity: containerAppIdentity.id
          server: acr.properties.loginServer
        }
      ]
    }
    template: {
      containers: [
        {
          image: '${acr.properties.loginServer}/${imageName}:${imageTagName}'
          name: 'apiservice'
          env: [
            {
              name: 'APP_SETTINGS_JSON'
              value: appSettingsJson
            }
          ]
          resources: {
            cpu: json('0.5')
            memory: '1.0Gi'
          }
        }
      ]
      scale: {
        minReplicas: minReplicas
        maxReplicas: maxReplicas
      }
    }
  }
}
