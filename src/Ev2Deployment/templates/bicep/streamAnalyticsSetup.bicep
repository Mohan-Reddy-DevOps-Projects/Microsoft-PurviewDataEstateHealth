param location string = resourceGroup().location
param dehCosmosDBDatabaseName string  // 'deh-cosmosdb-wus1-database'
param dghResourceGroupName string 
param streamAnalyticsPrefixName string // = 'gh-df-mwh'
param sharedCatalogEventHubNamespaceName string // = 'pdgdfeventhubmwh'
param sharedEventHubNamespaceAlias string // = 'pdgdfeventhubmwh'
param containerAppIdentityName string // = 'dgh-df-mwh-ca-identity'
param cosmosAccountName string // = 'deh-cosmosdb-wus3'
param subscriptionId string = subscription().subscriptionId
param dehAppIdentityName string //= 'dgh-df-mwh-ca-identity-asa'
param catalogResourceGroupName string 
param catalogSubscriptionId string
param keyVaultName string 
param coreResourceGroupName string 

resource dehResourceAppIdentity 'Microsoft.ManagedIdentity/userAssignedIdentities@2018-11-30' existing  = {
  name: containerAppIdentityName
  // location: location
}

resource eventHubNamespace 'Microsoft.EventHub/namespaces@2021-01-01-preview' existing = {
  name: sharedEventHubNamespaceAlias
}

resource database 'Microsoft.DocumentDB/databaseAccounts/sqlDatabases@2022-05-15' existing  = {  
  name: cosmosAccountName  
}



//Copy COSMODB ReadOnly Key to KeyVault
module copyCOSMODBReadkeytoKeyVault 'streamCopyCosmosDBkeytoKV.bicep' = {
    name: 'copyCOSMODBReadkeytoKeyVault'    
    //scope: resourceGroup(catalogSubscriptionId, catalogResourceGroupName)
    params: {
        coreResourceGroupName : coreResourceGroupName
        keyVaultName : keyVaultName 
        cosmosAccountName : cosmosAccountName
    }        
}


var outputName = 'output'
var inputName = 'input'
var streaAnalyticsBicepName = 'generateCatalogStreams'
var streampartNameDataCatalog= 'dc'
var catalogEventEH = 'catalogevent'
var partitionKey = 'accountId'
var documentId = 'eventId'

var businessdomain = '${streamAnalyticsPrefixName}-${streampartNameDataCatalog}-businessdomain'
var dataasset = '${streamAnalyticsPrefixName}-${streampartNameDataCatalog}-dataasset'
var dataassetwithlineage = '${streamAnalyticsPrefixName}-${streampartNameDataCatalog}-dassetwlineage'
var dataproduct = '${streamAnalyticsPrefixName}-${streampartNameDataCatalog}-dataproduct'
var relationship = '${streamAnalyticsPrefixName}-${streampartNameDataCatalog}-relationship'
var term = '${streamAnalyticsPrefixName}-${streampartNameDataCatalog}-term'
var dcatalogall= '${streamAnalyticsPrefixName}-${streampartNameDataCatalog}-dcatalogall'
var cde= '${streamAnalyticsPrefixName}-${streampartNameDataCatalog}-cde'
var okr= '${streamAnalyticsPrefixName}-${streampartNameDataCatalog}-okr'
var keyresult= '${streamAnalyticsPrefixName}-${streampartNameDataCatalog}-keyresult'
var cdc= '${streamAnalyticsPrefixName}-${streampartNameDataCatalog}-cdc'

var datasubscription = '${streamAnalyticsPrefixName}-${streampartNameDataaccess}-datasubscription'
var policyset =  '${streamAnalyticsPrefixName}-${streampartNameDataaccess}-policyset'

var dataqualityfact = '${streamAnalyticsPrefixName}-${streampartNameDQ}-dataqualityfact'
var dataqualityv2fact = '${streamAnalyticsPrefixName}-${streampartNameDQv2}-dataqualityv2fact'
var dataqualityrule = '${streamAnalyticsPrefixName}-${streampartNameDQv2}-dataqualityrule'
var dataqualityjobmetadata = '${streamAnalyticsPrefixName}-${streampartNameDQv2}-dataqualityjobmetadata'
var dataqualityassetdelete = '${streamAnalyticsPrefixName}-${streampartNameDQv2}-dataqualityassetdelete'
var dataqualityjobdelete = '${streamAnalyticsPrefixName}-${streampartNameDQv2}-dataqualityjobdelete'
var dataqualityschema = '${streamAnalyticsPrefixName}-${streampartNameDQv2}-dataqualityschema'

var provisioningdata = '${streamAnalyticsPrefixName}-${streampartNamePV}-provisionevent'

module streamAnalyticsJobsCatalog 'streamAnalyticsAccount.bicep' = {
  name: streaAnalyticsBicepName
  params: {
    location: location
    userAssignedIdentity: dehResourceAppIdentity.id
    inputName: inputName
    eventHubNamespacealias: eventHubNamespace.name  //sharedEventHubNamespaceName 
    eventHubNamespacename : sharedCatalogEventHubNamespaceName //sharedEventHubNamespaceName 
    eventHubName: catalogEventEH 
    cosmosAccountName: cosmosAccountName
    cosmosDbDatabaseName: dehCosmosDBDatabaseName
    partitionKey: partitionKey 
    documentId : documentId
    streamNameList: '${businessdomain},${dataasset},${dataproduct},${relationship},${term},${dataassetwithlineage},${dcatalogall},${cde},${okr},${keyresult},${cdc}'
    dghResourceGroupName : dghResourceGroupName 
    principalId : dehResourceAppIdentity.properties.principalId
    subscriptionId : subscriptionId
    streamAnalyticsPrefixName : streamAnalyticsPrefixName
    streampartNameDataCatalog : streampartNameDataCatalog
    streampartNameDataaccess : streampartNameDataaccess
    streampartNameDQ : streampartNameDQ
    catalogResourceGroupName : catalogResourceGroupName 
    catalogSubscriptionId : catalogSubscriptionId 
    streamNames: [
      { StreamName: businessdomain // '${streamAnalyticsPrefixName}-${streampartNameDataCatalog}-businessdomain'
        OutputName: outputName
        collectionName: 'businessdomain'
        consumerGroupName: businessdomain
        fromSelect : ' * '
        toSelect : ' * '
        FilterField1 : 'eventSource'
        FilterValue1 : 'DataCatalog'
        Operator : ' And '
        FilterField2 : 'payloadKind'
        FilterValue2 : 'BusinessDomain'
      }
      { StreamName: dataasset // '${streamAnalyticsPrefixName}-${streampartNameDataCatalog}-dataasset'
        OutputName: outputName
        collectionName: 'dataasset'
        consumerGroupName: dataasset
        fromSelect : '''
                    eventSource, payloadKind, operationType, preciseTimestamp, tenantId, accountId, 
                    udf.dataAssetHealLineage(payload) as payload, changedBy, eventId, correlationId,EventEnqueuedUtcTime, PartitionId, EventProcessedUtcTime 
                    '''
        toSelect : ' * '
        FilterField1 : 'eventSource'
        FilterValue1 : 'DataCatalog'
        Operator : ' And '
        FilterField2 : 'payloadKind'
        FilterValue2 : 'DataAsset'
      }
      { StreamName: dataassetwithlineage  // '${streamAnalyticsPrefixName}-${streampartNameDataCatalog}-dataasset'
        OutputName: outputName
        collectionName: 'dataassetwithlineage'
        consumerGroupName: dataassetwithlineage
        fromSelect : ' * '
        toSelect : ' * '
        FilterField1 : 'eventSource'
        FilterValue1 : 'DataCatalog'
        Operator : ' And '
        FilterField2 : 'payloadKind'
        FilterValue2 : 'DataAsset'
      }
      { StreamName: dataproduct // '${streamAnalyticsPrefixName}-${streampartNameDataCatalog}-dataproduct'
        OutputName: outputName
        collectionName: 'dataproduct'
        consumerGroupName: dataproduct
        fromSelect : ' * '
        toSelect : ' * '
        FilterField1 : 'eventSource'
        FilterValue1 : 'DataCatalog'
        Operator : ' and '
        FilterField2 : 'payloadKind'
        FilterValue2 : 'DataProduct'
      }
      { StreamName: relationship // '${streamAnalyticsPrefixName}-${streampartNameDataCatalog}-relationship'
        OutputName: outputName
        collectionName: 'relationship'
        consumerGroupName: relationship
        fromSelect : ' * '
        toSelect : ' * '
        FilterField1 : 'eventSource'
        FilterValue1 : 'DataCatalog'
        Operator : ' And '
        FilterField2 : 'payloadKind'
        FilterValue2 : 'Relationship'
      }
      { StreamName: term // '${streamAnalyticsPrefixName}-${streampartNameDataCatalog}-term'
        OutputName: outputName
        collectionName: 'term'
        consumerGroupName: term
        fromSelect : ' * '
        toSelect : ' * '
        FilterField1 : 'eventSource'
        FilterValue1 : 'DataCatalog'
        Operator : ' And '
        FilterField2 : 'payloadKind'
        FilterValue2 : 'Term'
      }
       { StreamName: dcatalogall // '${streamAnalyticsPrefixName}-${streampartNameDataCatalog}-term'
        OutputName: outputName
        collectionName: 'dcatalogall'
        consumerGroupName: dcatalogall
        fromSelect : ' * '
        toSelect : ' * '
        FilterField1 : 'eventSource'
        FilterValue1 : 'DataCatalog'
        Operator : ' And '
        FilterField2 : 'eventSource'
        FilterValue2 : 'DataCatalog'
      }
      { StreamName: cde // '${streamAnalyticsPrefixName}-${streampartNameDataCatalog}-term'
        OutputName: outputName
        collectionName: 'cde'
        consumerGroupName: cde
        fromSelect : ' * '
        toSelect : ' * '
        FilterField1 : 'eventSource'
        FilterValue1 : 'DataCatalog'
        Operator : ' And '
        FilterField2 : 'payloadKind'
        FilterValue2 : 'CriticalDataElement'
      }
      { StreamName: okr // '${streamAnalyticsPrefixName}-${streampartNameDataCatalog}-term'
        OutputName: outputName
        collectionName: 'okr'
        consumerGroupName: okr
        fromSelect : ' * '
        toSelect : ' * '
        FilterField1 : 'eventSource'
        FilterValue1 : 'DataCatalog'
        Operator : ' And '
        FilterField2 : 'payloadKind'
        FilterValue2 : 'OKR'
      }
      { StreamName: keyresult // '${streamAnalyticsPrefixName}-${streampartNameDataCatalog}-term'
        OutputName: outputName
        collectionName: 'keyresult'
        consumerGroupName: keyresult
        fromSelect : ' * '
        toSelect : ' * '
        FilterField1 : 'eventSource'
        FilterValue1 : 'DataCatalog'
        Operator : ' And '
        FilterField2 : 'payloadKind'
        FilterValue2 : 'KeyResult'
      }
      { StreamName: cdc // '${streamAnalyticsPrefixName}-${streampartNameDataCatalog}-term'
        OutputName: outputName
        collectionName: 'cdc'
        consumerGroupName: cdc
        fromSelect : ' * '
        toSelect : ' * '
        FilterField1 : 'eventSource'
        FilterValue1 : 'DataCatalog'
        Operator : ' And '
        FilterField2 : 'payloadKind'
        FilterValue2 : 'CDC'
      }
    ]
  }
}

var streaAnalyticsBicepNameDataAccess = 'generateDataAccessStream'
var streampartNameDataaccess = 'da'
var dataAccessEH= 'dataaccessevent'

module streamAnalyticsJobsDataAccess 'streamAnalyticsAccount.bicep' = {
  name: streaAnalyticsBicepNameDataAccess  
  dependsOn: [ streamAnalyticsJobsCatalog ]
  params: {
    location: location
    userAssignedIdentity: dehResourceAppIdentity.id
    inputName: inputName
    eventHubNamespacealias: eventHubNamespace.name  //sharedEventHubNamespaceName 
    eventHubNamespacename : sharedCatalogEventHubNamespaceName  //sharedEventHubNamespaceName 
    eventHubName: dataAccessEH
    cosmosAccountName: cosmosAccountName
    cosmosDbDatabaseName: dehCosmosDBDatabaseName
    streamNameList: '${datasubscription},${policyset}'
    partitionKey: partitionKey 
    documentId : documentId
    dghResourceGroupName : dghResourceGroupName 
    principalId : dehResourceAppIdentity.properties.principalId
    subscriptionId : subscriptionId
    streamAnalyticsPrefixName : streamAnalyticsPrefixName
    streampartNameDataCatalog : streampartNameDataCatalog
    streampartNameDataaccess : streampartNameDataaccess
    streampartNameDQ : streampartNameDQ
    catalogResourceGroupName : catalogResourceGroupName 
    catalogSubscriptionId : catalogSubscriptionId 
    streamNames: [
      { StreamName: datasubscription // '${streamAnalyticsPrefixName}-${streampartNameDataaccess}-datasubscription'
        OutputName: outputName
        collectionName: 'datasubscription'
        consumerGroupName: datasubscription
        fromSelect : ' * '
        toSelect : ' * '
        FilterField1 : 'eventSource'
        FilterValue1 : 'DataAccess'
        Operator : ' And '
        FilterField2 : 'payloadKind'
        FilterValue2 : 'DataSubscription'
      }
      { StreamName: policyset // '${streamAnalyticsPrefixName}-${streampartNameDataaccess}-policyset'
        OutputName: outputName
        collectionName: 'policyset'
        consumerGroupName: policyset
        fromSelect : ' * '
        toSelect : ' * '
        FilterField1 : 'eventSource'
        FilterValue1 : 'DataAccess'
        Operator : ' And '
        FilterField2 : 'payloadKind'
        FilterValue2 : 'PolicySet'
      } 
    ]
  }
}


var streaAnalyticsBicepNameDataQuality= 'generateDataQualityStream'
var streampartNameDQ = 'dq'
var dataQualityEH= 'dataqualityevent'

module streamAnalyticsJobsDataQuality  'streamAnalyticsAccount.bicep' = {
  name: streaAnalyticsBicepNameDataQuality  
  dependsOn: [ streamAnalyticsJobsDataAccess ]
  params: {
    location: location
    userAssignedIdentity: dehResourceAppIdentity.id
    inputName: inputName
    eventHubNamespacealias: eventHubNamespace.name  //sharedEventHubNamespaceName 
    eventHubNamespacename : sharedCatalogEventHubNamespaceName  //sharedEventHubNamespaceName 
    eventHubName: dataQualityEH
    cosmosAccountName: cosmosAccountName
    cosmosDbDatabaseName: dehCosmosDBDatabaseName
    streamNameList: '${dataqualityfact}'
    partitionKey: partitionKey 
    documentId : documentId
    dghResourceGroupName : dghResourceGroupName 
    principalId : dehResourceAppIdentity.properties.principalId
    subscriptionId : subscriptionId
    streamAnalyticsPrefixName : streamAnalyticsPrefixName
    streampartNameDataCatalog : streampartNameDataCatalog
    streampartNameDataaccess : streampartNameDataaccess
    streampartNameDQ : streampartNameDQ
    catalogResourceGroupName : catalogResourceGroupName 
    catalogSubscriptionId : catalogSubscriptionId 
    streamNames: [
      { StreamName: dataqualityfact // '${streamAnalyticsPrefixName}-${streampartNameDQ}-dataqualityfact'
        OutputName: outputName
        collectionName: 'dataqualityfact'
        consumerGroupName: dataqualityfact
        fromSelect : ' * '
        toSelect : ' * '
        FilterField1 : 'payloadKind'
        FilterValue1 : 'dataQualityFact'
        Operator : ' And '
        FilterField2 : 'payloadKind'
        FilterValue2 : 'dataQualityFact'
      }
    ]
  }
}




var streaAnalyticsBicepNameDataQualityv2= 'generateDataQualityv2Stream'
var streampartNameDQv2 = 'dq2'
var dataQualityv2EH= 'dataqualityeventv2'

module streamAnalyticsJobsDataQualityv2  'streamAnalyticsAccount.bicep' = {
  name: streaAnalyticsBicepNameDataQualityv2  
  dependsOn: [ streamAnalyticsJobsDataQuality ]
  params: {
    location: location
    userAssignedIdentity: dehResourceAppIdentity.id
    inputName: inputName
    eventHubNamespacealias: eventHubNamespace.name  //sharedEventHubNamespaceName 
    eventHubNamespacename : sharedCatalogEventHubNamespaceName  //sharedEventHubNamespaceName 
    eventHubName: dataQualityv2EH
    cosmosAccountName: cosmosAccountName
    cosmosDbDatabaseName: dehCosmosDBDatabaseName
    streamNameList: '${dataqualityv2fact},${dataqualityrule},${dataqualityjobmetadata},${dataqualityassetdelete},${dataqualityjobdelete},${dataqualityschema}'
    partitionKey: partitionKey 
    documentId : documentId
    dghResourceGroupName : dghResourceGroupName 
    principalId : dehResourceAppIdentity.properties.principalId
    subscriptionId : subscriptionId
    streamAnalyticsPrefixName : streamAnalyticsPrefixName
    streampartNameDataCatalog : streampartNameDataCatalog
    streampartNameDataaccess : streampartNameDataaccess
    streampartNameDQ : streampartNameDQv2
    catalogResourceGroupName : catalogResourceGroupName 
    catalogSubscriptionId : catalogSubscriptionId 
    streamNames: [
      { StreamName: dataqualityv2fact // '${streamAnalyticsPrefixName}-${streampartNameDQ}-dataqualityv2fact'
        OutputName: outputName
        collectionName: 'dataqualityv2fact'
        consumerGroupName: dataqualityv2fact
        fromSelect : ' * '
        toSelect : ' * '
        FilterField1 : 'payloadKind'
        FilterValue1 : 'dataQualityFact'
        Operator : ' And '
        FilterField2 : 'payloadKind'
        FilterValue2 : 'dataQualityFact'
      }
      { StreamName: dataqualityrule 
        OutputName: outputName
        collectionName: 'dataqualityrule'
        consumerGroupName: dataqualityrule
        fromSelect : ' * '
        toSelect : ' * '
        FilterField1 : 'payloadKind'
        FilterValue1 : 'dataQualityRule'
        Operator : ' And '
        FilterField2 : 'payloadKind'
        FilterValue2 : 'dataQualityRule'
      }
      { StreamName: dataqualityjobmetadata 
        OutputName: outputName
        collectionName: 'dataqualityjobmetadata'
        consumerGroupName: dataqualityjobmetadata
        fromSelect : ' * '
        toSelect : ' * '
        FilterField1 : 'payloadKind'
        FilterValue1 : 'dataQualityJobMetadata'
        Operator : ' And '
        FilterField2 : 'payloadKind'
        FilterValue2 : 'dataQualityJobMetadata'
      }
      { StreamName: dataqualityassetdelete 
        OutputName: outputName
        collectionName: 'dataqualityassetdelete'
        consumerGroupName: dataqualityassetdelete
        fromSelect : ' * '
        toSelect : ' * '
        FilterField1 : 'payloadKind'
        FilterValue1 : 'dataQualityAssetDelete'
        Operator : ' And '
        FilterField2 : 'payloadKind'
        FilterValue2 : 'dataQualityAssetDelete'
      }
      { StreamName: dataqualityjobdelete 
        OutputName: outputName
        collectionName: 'dataqualityjobdelete'
        consumerGroupName: dataqualityjobdelete
        fromSelect : ' * '
        toSelect : ' * '
        FilterField1 : 'payloadKind'
        FilterValue1 : 'dataQualityJobDelete'
        Operator : ' And '
        FilterField2 : 'payloadKind'
        FilterValue2 : 'dataQualityJobDelete'
      }
      { StreamName: dataqualityschema 
        OutputName: outputName
        collectionName: 'dataqualityschema'
        consumerGroupName: dataqualityschema
        fromSelect : ' * '
        toSelect : ' * '
        FilterField1 : 'payloadKind'
        FilterValue1 : 'dataQualitySchema'
        Operator : ' And '
        FilterField2 : 'payloadKind'
        FilterValue2 : 'dataQualitySchema'
      }
    ]
  }
}

    

var streaAnalyticsBicepNameProvision= 'generateProvisionStream'
var streampartNamePV= 'pv'
var provisioningEH= 'provisionevent'

module streamAnalyticsJobsProvision  'streamAnalyticsAccount.bicep' = {
  name: streaAnalyticsBicepNameProvision  
  dependsOn: [ streamAnalyticsJobsDataQualityv2 ]
  params: {
    location: location
    userAssignedIdentity: dehResourceAppIdentity.id
    inputName: inputName
    eventHubNamespacealias: eventHubNamespace.name  //sharedEventHubNamespaceName 
    eventHubNamespacename : sharedCatalogEventHubNamespaceName  //sharedEventHubNamespaceName 
    eventHubName: provisioningEH
    cosmosAccountName: cosmosAccountName
    cosmosDbDatabaseName: dehCosmosDBDatabaseName
    streamNameList: '${provisioningdata}'
    partitionKey: partitionKey 
    documentId : documentId
    dghResourceGroupName : dghResourceGroupName 
    principalId : dehResourceAppIdentity.properties.principalId
    subscriptionId : subscriptionId
    streamAnalyticsPrefixName : streamAnalyticsPrefixName
    streampartNameDataCatalog : streampartNameDataCatalog
    streampartNameDataaccess : streampartNameDataaccess
    streampartNameDQ : streampartNamePV
    catalogResourceGroupName : catalogResourceGroupName 
    catalogSubscriptionId : catalogSubscriptionId 
    streamNames: [
      { StreamName: provisioningdata // '${streamAnalyticsPrefixName}-${streampartNameDQ}-dataqualityfact'
        OutputName: outputName
        collectionName: 'provisionevent'
        consumerGroupName: provisioningdata
        fromSelect : ' * '
        toSelect : ' * '
        FilterField1 : '1'
        FilterValue1 : '1'
        Operator : ' And '
        FilterField2 : '1'
        FilterValue2 : '1'
      }
    ]
  }
}
