param acaEnvironmentName string
param acaResourceGroupName string
param containerAppName string
param dnsZoneName string
param dnsZoneResourceGroupName string
param dnsZoneSubscriptionId string
param subdomainName string

resource acaEnvironment 'Microsoft.App/managedEnvironments@2023-05-01' existing = {
  name: acaEnvironmentName
  scope: resourceGroup(acaResourceGroupName)
}

module dnsRecords 'dnsRecords.bicep' = {
  name: 'dnsRecords'
  scope: resourceGroup(dnsZoneSubscriptionId, dnsZoneResourceGroupName)
  params: {
    cnameRecordName: '${containerAppName}.${acaEnvironment.properties.defaultDomain}'
    dnsZoneName: dnsZoneName
    subdomain: subdomainName
    txtRecordValue: acaEnvironment.properties.customDomainConfiguration.customDomainVerificationId
  }
}
