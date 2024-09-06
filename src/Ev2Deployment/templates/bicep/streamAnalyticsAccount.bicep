param userAssignedIdentity string
param inputName string
param location string = resourceGroup().location
param eventHubNamespacename string
param eventHubNamespacealias string
param eventHubName string
param cosmosAccountName string
param cosmosDbDatabaseName string
param partitionKey string
param documentId string
param streamNames array
param streamNameList string 
param forceUpdateTag string = utcNow()
param subscriptionId string = subscription().subscriptionId
param streamAnalyticsPrefixName string 
param streampartNameDataCatalog string
param streampartNameDataaccess string
param streampartNameDQ string 
param dghResourceGroupName string 
param principalId string 
param catalogResourceGroupName string 
param catalogSubscriptionId string 


//Create EH Consumer Group
module createEHConsumerGroups 'streamAnalyticsEventHubConsumerGroup.bicep' = [for (stream, index) in streamNames: {
        name: '${stream.streamName}CGroup' // 'businessdomainContributorAssignmentModule'     
        //scope : resourceGroup(catalogResourceGroupName)
        scope: resourceGroup(catalogSubscriptionId, catalogResourceGroupName)
        params: {
            eventHubNamespacename : eventHubNamespacename 
            eventHubName : eventHubName 
            consumerGroupName : stream.consumerGroupName
        }        
    }
]


resource stopStreamAnalytics 'Microsoft.Resources/deploymentScripts@2020-10-01' = {
  kind: 'AzurePowerShell'
  name: 'stopStreamAnalytics'
  location: location
  dependsOn: [ createEHConsumerGroups ]
  identity: {
    type: 'UserAssigned'
    userAssignedIdentities: {
      '${userAssignedIdentity}': {}
    }
  }
  properties: {
    retentionInterval: 'PT1H'
    azPowerShellVersion: '10.0'
    forceUpdateTag: forceUpdateTag
    environmentVariables: [
      {
        name: 'streamNameList'
        value: streamNameList
      }
      {
        name : 'subscriptionId'
        value : subscriptionId
      }
      {
        name : 'dghResourceGroupName'
        value : dghResourceGroupName 
      }
    ]
    scriptContent: '''

    Set-AzContext -subscription "${Env:subscriptionId}"

    $StreamNameList = "${Env:streamNameList}"
    $StreamNameArray = $StreamNameList -split ','

    foreach ($item in $StreamNameArray) {
      Write-Output "Item: $item"
      
      try 
      {
      
        # Get the Stream Analytics job status
        $streamAnalyticsJob = Get-AzStreamAnalyticsJob -ResourceGroupName ${Env:dghResourceGroupName} -Name $item

          if($streamAnalyticsJob)
          {
            $jobState = $streamAnalyticsJob.JobState
            # Check if the job status is running
            if ($streamAnalyticsJob.ProvisioningState -eq "Succeeded" -and ($jobState -eq "Running" -or $jobState -eq "Starting")) {                             
              Write-Output "Stopping Stream Analytics $item"
              Stop-AzStreamAnalyticsJob -ResourceGroupName ${Env:dghResourceGroupName} -Name $item
            }
          }
      }
      catch {              
        Write-Error $_.Exception
      }
    }


    '''

    cleanupPreference: 'OnSuccess'

  }
}



var streamAnalyticsLocation = replace(location,'euap','')

resource streamingJobs 'Microsoft.StreamAnalytics/streamingjobs@2021-10-01-preview' = [for (stream, index) in streamNames: {
  name: stream.StreamName
  location: streamAnalyticsLocation  
  dependsOn: [ stopStreamAnalytics ]
  identity: {
    type: 'UserAssigned'
    userAssignedIdentities: {
      '${userAssignedIdentity}': {}
    }
  }
  properties: {
    sku: {
      name: 'StandardV2'
    }
    eventsOutOfOrderPolicy: 'Adjust'
    compatibilityLevel: '1.2'
    eventsOutOfOrderMaxDelayInSeconds: 0
    eventsLateArrivalMaxDelayInSeconds: 5
    dataLocale: 'en-US'
    functions: [
      {
        name: 'dataAssetHealLineage'
       properties: {
          properties: {
            binding: {
              type: 'Microsoft.StreamAnalytics/JavascriptUdf'
              // For remaining properties, see FunctionBinding objects
              properties: {
                script: '''      

                        // UDF to remove lineage node from payload json
                        function dataAssetHealLineage(dataAssetPayload) {
                            var dataAsset = JSON.stringify(dataAssetPayload);
                            var payload = JSON.parse(dataAsset);
                            if (payload.before != null) {
                                if (payload.before.lineage !== undefined) {
                                    payload.before.lineage = null;
                                }
                            }

                            if (payload.after != null) {
                                if (payload.after.lineage !== undefined) {
                                    payload.after.lineage = null;
                                }
                            }
                            return payload;
                        }

  
                '''
              }
            }
            inputs: [
              {
                dataType: 'record'
                isConfigurationParameter: null
              }
            ]
            output: {
              dataType: 'record'              
            }
                        
          }

          type: 'Scalar'
          
        }
      }
    ]
    inputs: [ {
        name: '${stream.StreamName}-${inputName}'
        properties: {
          type: 'Stream'
          datasource: {
            type: 'Microsoft.ServiceBus/EventHub'
            properties: {
              serviceBusNamespace: eventHubNamespacealias 
              eventHubName: eventHubName
              consumerGroupName: stream.consumerGroupName
              //if in case we need to use access key
              //sharedAccessPolicyName: 'RootManageSharedAccessKey'
              //sharedAccessPolicyKey: listKeys(authRule.id, '2021-01-01-preview').primaryKey
              // Use user-assigned identity for authentication
              authenticationMode: 'msi'
            }
          }
          serialization: {
            type: 'Json'
            properties: {
              encoding: 'UTF8'
            }
          }
        }
      }
    ]
    outputs: [
      {
        name: '${stream.StreamName}${stream.OutputName}'
        // name: stream.OutputName // '${stream.name}${stream.OutputName}'
        // parent: stream.name
        properties: {
          datasource: {
            type: 'Microsoft.Storage/DocumentDB'
            properties: {
              accountId: cosmosAccountName
              //accountKey: listKeys(cosmosDbAccount.id, '2023-11-15').primaryMasterKey
              database: cosmosDbDatabaseName
              collectionNamePattern: stream.collectionName
              partitionKey: partitionKey  //'accountId'
              documentId: documentId 
              authenticationMode: 'msi'
            }
          }
          serialization: {
            type: 'Json'
            properties: {
              encoding: 'UTF8'
            }
          }
        }
      }
    ]

    transformation: {

      name: inputName
      properties: {
        streamingUnits: 10
        query: 'WITH\n\r [${stream.StreamName}${inputName}] AS (\n\r SELECT ${stream.fromSelect}\n FROM [${stream.StreamName}-${inputName}] \n TIMESTAMP BY EventEnqueuedUtcTime \n ), \n\r [filter] As  ( \n Select * \n  From [${stream.StreamName}${inputName}] \n Where ([${stream.StreamName}${inputName}].[${stream.FilterField1}] = \'${stream.FilterValue1}\') ${stream.Operator} ([${stream.StreamName}${inputName}].[${stream.FilterField2}] = \'${stream.FilterValue2}\') \n\r)   \n\r SELECT \n ${stream.toSelect} INTO [${stream.StreamName}${stream.OutputName}] \n FROM [filter] '

      }
    }
  }
}
]
    
var contributorGuid = 'b24988ac-6180-42a0-ab88-20f7382dd24c'

module processContributorAssignmentModule 'streamAnalyticsRoleAssignment.bicep' = [for (stream, index) in streamNames: {
        name: stream.streamName // 'businessdomainContributorAssignmentModule'
        params: {
        contributorGuid: contributorGuid
        dghResourceGroupName: dghResourceGroupName
        userAssignedIdentity: userAssignedIdentity
        streamName: stream.streamName
        principalId : principalId
        }
        dependsOn: [streamingJobs]
    }
]


resource startStreamAnalytics 'Microsoft.Resources/deploymentScripts@2020-10-01' = {
  kind: 'AzurePowerShell'
  name: 'startStreamAnalytics'
  location: location
  dependsOn: [ processContributorAssignmentModule ]
  identity: {
    type: 'UserAssigned'
    userAssignedIdentities: {
      '${userAssignedIdentity}': {}
    }
  }
  properties: {
    retentionInterval: 'PT1H'
    azPowerShellVersion: '10.0'
    forceUpdateTag: forceUpdateTag
    environmentVariables: [
      {
        name: 'streamNameList'
        value: streamNameList
      }
      {
        name : 'subscriptionId'
        value : subscriptionId
      }
      {
        name : 'dghResourceGroupName'
        value : dghResourceGroupName 
      }
    ]
    scriptContent: '''

    Set-AzContext -subscription "${Env:subscriptionId}"

    $StreamNameList = "${Env:streamNameList}"
    $StreamNameArray = $StreamNameList -split ','

    foreach ($item in $StreamNameArray) {
      Write-Output "Item: $item"
      
      try 
      {
      
        # Get the Stream Analytics job status
        $streamAnalyticsJob = Get-AzStreamAnalyticsJob -ResourceGroupName ${Env:dghResourceGroupName} -Name $item

          if($streamAnalyticsJob)
          {
            $jobState = $streamAnalyticsJob.JobState
            # Check if the job status is running
            if ($streamAnalyticsJob.ProvisioningState -eq "Succeeded") {
              Write-Output "Starting Stream Analytics $item"
              #Stop-AzStreamAnalyticsJob -ResourceGroupName ${Env:dghResourceGroupName} -Name $item
              Start-AzStreamAnalyticsJob -ResourceGroupName ${Env:dghResourceGroupName} -Name $item
            }
          }
      }
      catch {              
        Write-Error $_.Exception
      }
    }
    
    '''

    cleanupPreference: 'OnSuccess'

  }
}