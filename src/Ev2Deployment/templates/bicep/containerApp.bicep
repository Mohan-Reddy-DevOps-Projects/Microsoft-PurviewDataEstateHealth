param acaEnvironmentName string
param acaResourceGroupName string
param acrName string
param appSettingsJson string
param containerAppIdentityName string
param containerAppName string
param dnsZoneName string = ''
param externalIngress bool = false
param imageName string
param imageTagName string
param location string = resourceGroup().location
param maxReplicas int = 10
param minReplicas int = 1
param subdomainName string = ''
param readinessPort int
param genevaErrorLogTableId string

resource acaEnvironment 'Microsoft.App/managedEnvironments@2023-05-01' existing = {
  name: acaEnvironmentName
  scope: resourceGroup(acaResourceGroupName)

  resource sslCert 'certificates@2023-05-01' existing = {
    name: 'ssl-cert'
  }
}

resource acr 'Microsoft.ContainerRegistry/registries@2022-12-01' existing = {
  name: acrName
}

resource containerAppIdentity 'Microsoft.ManagedIdentity/userAssignedIdentities@2018-11-30' existing = {
  name: containerAppIdentityName
}

resource containerApp 'Microsoft.App/containerApps@2023-05-02-preview' = {
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
      maxInactiveRevisions: 2
      ingress: externalIngress ? {
        clientCertificateMode: 'require'
        external: true
        targetPort: 8080
        customDomains: [
          {
            name: '${subdomainName}.${dnsZoneName}'
            certificateId: acaEnvironment::sslCert.id
            bindingType: 'SniEnabled'
          }
        ]
      } : null
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
          name: imageName
          probes: [
            {
              type: 'readiness'
              timeoutSeconds: 3
              failureThreshold: 3
              httpGet: {
                port: readinessPort
                path: '/healthz/ready'
                scheme: 'HTTP'
              }
             }
             {
              type: 'startup'
              timeoutSeconds: 3
              failureThreshold: 3
              httpGet: {
                port: readinessPort
                path: '/healthz/ready'
                scheme: 'HTTP'
              }
             }
          ]
          env: [
            {
              name: 'APP_SETTINGS_JSON'
              value: appSettingsJson
            }
            {
              name: 'AZURE_CLIENT_ID'
              value: containerAppIdentity.properties.clientId
            }
            {
              name: 'BUILD_VERSION'
              value: imageTagName
            }
            {
              name: 'GENEVA_ERROR_LOG_TABLE_ID'
              value: genevaErrorLogTableId
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
