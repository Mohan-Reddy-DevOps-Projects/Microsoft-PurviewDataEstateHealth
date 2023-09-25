param cnameRecordName string
param dnsZoneName string
param subdomain string
param txtRecordValue string

resource dnsZone 'Microsoft.Network/dnsZones@2018-05-01' existing = {
  name: dnsZoneName

  resource cname 'CNAME@2018-05-01' = {
    name: subdomain
    properties: {
      TTL: 3600
      CNAMERecord: {
        cname: cnameRecordName
      }
    }
  }

  resource verification 'TXT@2018-05-01' = {
    name: 'asuid.${subdomain}'
    properties: {
      TTL: 3600
      TXTRecords: [
        {
          value: [txtRecordValue]
        }
      ]
    }
  }
}
