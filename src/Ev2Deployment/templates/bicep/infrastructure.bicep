param acrName string
param assistedIdAppObjectId string
param catalogResourceGroupName string
param catalogSubscriptionId string
param commonStorageAccountName string
param containerAppIdentityName string
param coreResourceGroupName string
param sharedEventHubNamespaceName string
param secondarySharedEventHubName string
param jobStorageAccountName string
param keyVaultName string
param sqlAdminUserSecretName string
param sqlAdminPassSecretName string
param location string = resourceGroup().location
param vnetName string
param synapseWorkspaceName string
param synapseStorageAccountName string
param synapseStorageAccountUrl string
param sparkPoolTableName string
param mdqFailedJobTableName string
param triggeredScheduleQueueName string
param synapseLocation string
param synapseDatabaseName string
param subscriptionId string = subscription().subscriptionId
param forceUpdateTag string = utcNow()
param eventHubs array
param consumerGroupName string

//remove after EH migration
param catalogEventHubName string
param tempEventHubName string

var contributorRoleDefName = 'b24988ac-6180-42a0-ab88-20f7382dd24c'
var azureEventHubsDataReceiverRoleDefName = 'a638d3c7-ab3a-418d-83e6-5f17a39d4fde'
var keyVaultReaderRoleDefName = '21090545-7ca7-4776-b22c-e363652d74d2'
var keyVaultSecretsOfficerRoleDefName = 'b86a8fe4-44ce-4948-aee5-eccb2c155cd7'
var keyVaultCertificatesUserRoleDefName = 'db79e9a7-68ee-4b58-9aeb-b90e7c24fcba'
var keyVaultCertificatesOfficerRoleDefName = 'a4417e6f-fecd-4de8-b567-7b0420556985'
var ownerRoleDefName = '8e3af657-a8ff-443c-a75c-2fe8c4bcb635'

resource containerAppIdentity 'Microsoft.ManagedIdentity/userAssignedIdentities@2018-11-30' = {
  name: containerAppIdentityName
  location: location
}

resource acr 'Microsoft.ContainerRegistry/registries@2023-07-01' = {
  name: toLower(acrName)
  location: location
  sku: {
    name: 'Premium'
  }
  properties: {
    adminUserEnabled: false
    anonymousPullEnabled: false
    policies: {
      quarantinePolicy: {
        status: 'disabled'
      }
      trustPolicy: {
        type: 'Notary'
        status: 'disabled'
      }
      retentionPolicy: {
        days: 7
        status: 'enabled'
      }
    }
    dataEndpointEnabled: false
    publicNetworkAccess: 'Enabled'
    zoneRedundancy: 'Disabled'
  }
}

module acrRoleAssignments 'acrRoleAssignments.bicep' = {
  name: 'acrRoleAssignments'
  params: {
    acrName: acr.name
    principalId: containerAppIdentity.properties.principalId
  }
}

resource vnet 'Microsoft.Network/virtualNetworks@2023-04-01' existing = {
  name: vnetName
  scope: resourceGroup(coreResourceGroupName)
}

module jobStorageAccount 'storageAccount.bicep' = {
  name: 'jobStorageAccount'
  params: {
    location: location
    storageAccountName: jobStorageAccountName
    commonSubnetId: vnet.properties.subnets[0].id
    deploySubnetId: vnet.properties.subnets[1].id
  }
}

module storageRoleAssignments 'storageRoleAssignments.bicep' = {
  dependsOn: [jobStorageAccount]
  name: 'storageRoleAssignments'
  params: {
    storageAccountName: jobStorageAccountName
    principalId: containerAppIdentity.properties.principalId
  }
}

module synapseStorageRoleAssignments 'synapseStorageRoleAssignments.bicep' = {
  dependsOn: [synapseStorageAccount]
  name: 'synapseStorageRoleAssignments'
  params: {
    storageAccountName: synapseStorageAccountName
    principalId: containerAppIdentity.properties.principalId
  }
}

module commonStorageAccountModule 'storageAccount.bicep' = {
  name: 'commonStorageAccountDeploy'
  params: {
    location: location
    storageAccountName: commonStorageAccountName
    commonSubnetId: vnet.properties.subnets[0].id
    deploySubnetId: vnet.properties.subnets[1].id
  }
}

module sparkPoolTableModule 'storageTable.bicep' = {
  name: 'sparkPoolTableDeploy'
  params: {
    storageAccountName: commonStorageAccountModule.outputs.storageAccountName
    tableName: sparkPoolTableName
  }
}

module mdqFailedJobTableModule 'storageTable.bicep' = {
  name: 'mdqFailedJobTableDeploy'
  params: {
    storageAccountName: commonStorageAccountModule.outputs.storageAccountName
    tableName: mdqFailedJobTableName
  }
}

module triggeredScheduleQueueModule 'storageQueue.bicep' = {
  name: 'triggeredScheduleQueueDeploy'
  params: {
    storageAccountName: commonStorageAccountModule.outputs.storageAccountName
    queueName: triggeredScheduleQueueName
  }
}

module commonStorageAccountRoleAssignmentsModule 'storageRoleAssignments.bicep' = {
  name: 'commonStorageAccountRoleAssignmentsDeploy'
  params: {
    storageAccountName: commonStorageAccountModule.outputs.storageAccountName
    principalId: containerAppIdentity.properties.principalId
  }
}

resource keyVault 'Microsoft.KeyVault/vaults@2023-02-01' = {
  location: location
  name: keyVaultName
  properties: {
    sku: {
      family: 'A'
      name: 'standard'
    }
    tenantId: tenant().tenantId
    enableRbacAuthorization: true
  }
}

module keyVaultReaderRoleModule 'keyVaultRoleAssignment.bicep' = {
  name: 'keyVaultReaderRoleDeploy'
  params: {
    keyVaultName: keyVault.name
    principalId: containerAppIdentity.properties.principalId
    roleDefinitionName: keyVaultReaderRoleDefName
  }
}

module keyVaultSecretsOfficerRoleModule 'keyVaultRoleAssignment.bicep' = {
  name: 'keyVaultSecretsOfficerRoleDeploy'
  params: {
    keyVaultName: keyVault.name
    principalId: containerAppIdentity.properties.principalId
    roleDefinitionName: keyVaultSecretsOfficerRoleDefName
  }
}

module keyVaultCertUserRoleModule 'keyVaultRoleAssignment.bicep' = {
  name: 'keyVaultCertUserRoleDeploy'
  params: {
    keyVaultName: keyVault.name
    principalId: containerAppIdentity.properties.principalId
    roleDefinitionName: keyVaultCertificatesUserRoleDefName
  }
}

module synapseRoleAssignmentsModule 'synapseRoleAssignment.bicep' = {
  name: 'synapseRoleAssignment'
  params: {
    synapseWorkspaceName: synapseWorkspace.name
    principalId: containerAppIdentity.properties.principalId
    roleDefinitionName: contributorRoleDefName
  }
}

// The assisted ID app needs Key Vault Certificate Officer to create certificates.
module keyVaultCertificatesOfficerRoleModule 'keyVaultRoleAssignment.bicep' = {
  name: 'keyVaultCertificatesOfficerRoleDeploy'
  params: {
    keyVaultName: keyVault.name
    principalId: assistedIdAppObjectId
    roleDefinitionName: keyVaultCertificatesOfficerRoleDefName
  }
}

module synapseStorageAccount 'synapseStorageAccount.bicep' = {
  name: 'synapseStorageAccount'
  params: {
    location: location
    storageAccountName: synapseStorageAccountName
  }
}

resource synapseWorkspace 'Microsoft.Synapse/workspaces@2021-06-01' = {
  name: synapseWorkspaceName
  location: synapseLocation
  tags: {}
  identity: {
    type: 'SystemAssigned'
  }
  properties: {
    azureADOnlyAuthentication: false
    publicNetworkAccess: 'Enabled'
    managedVirtualNetwork: 'default'
    defaultDataLakeStorage: {
      accountUrl: synapseStorageAccountUrl
      createManagedPrivateEndpoint: true
      filesystem: synapseStorageAccount.outputs.StorageContainerName
      resourceId: synapseStorageAccount.outputs.StorageAccountResourceId
    }
    sqlAdministratorLogin: generateSqlAdminCreds.properties.outputs.username
    sqlAdministratorLoginPassword: generateSqlAdminCreds.properties.outputs.password
  }
}

resource synapseWorkspaceAdmin 'Microsoft.Synapse/workspaces/administrators@2021-06-01' = {
  name: 'activeDirectory'
  parent: synapseWorkspace
  properties: {
    administratorType: 'Enterprise application'
    login: 'PDG Buildout App'
    sid: assistedIdAppObjectId
    tenantId: tenant().tenantId
  }
}

resource createSynapseDatabase 'Microsoft.Resources/deploymentScripts@2023-08-01' = {
  name: 'createDatabaseScript'
  location: location
  kind: 'AzurePowerShell'
  identity: {
    type: 'UserAssigned'
    userAssignedIdentities: {
      '${containerAppIdentity.id}' : {}
    }
  }
  dependsOn: [
    generateSqlAdminCreds
  ]
  properties: {
    azPowerShellVersion: '10.0'
    forceUpdateTag: forceUpdateTag
    environmentVariables: [
      {
        name: 'serverFQName'
        value: synapseWorkspace.properties.connectivityEndpoints.sqlOnDemand
      }
      {
        name: 'databaseName'
        value: synapseDatabaseName
      }
      {
        name: 'subscriptionId'
        value: subscriptionId
      }
      {
        name: 'keyVaultName'
        value: keyVault.name
      }
      {
        name: 'sqlAdminPassSecretName'
        value: sqlAdminPassSecretName
      }
      {
        name: 'sqlAdminUserSecretName'
        value: sqlAdminUserSecretName
      }
    ]
    scriptContent: '''
      Set-AzContext -subscription ${Env:subscriptionId}
      
      $userName = Get-AzKeyVaultSecret -VaultName ${Env:keyVaultName} -Name ${Env:sqlAdminUserSecretName} -AsPlainText
      $sqlPassword = Get-AzKeyVaultSecret -VaultName ${Env:keyVaultName} -Name ${Env:sqlAdminPassSecretName} -AsPlainText

      # Connect to the Synapse Analytics server
      $connStr = "Server=tcp:${Env:serverFQName};Initial Catalog=master;Persist Security Info=False;User ID=$userName;Password=$sqlPassword;MultipleActiveResultSets=False;Encrypt=True;TrustServerCertificate=False;Connection Timeout=30;"
      $maxRetries = 5
      $curTry = 0
      do {
        try {
          $conn = New-Object System.Data.SqlClient.SqlConnection
          $conn.ConnectionString = $connStr
          $conn.Open()

          # Execute the CREATE DATABASE command
          $sqlCommand = @"

          IF NOT EXISTS(SELECT * FROM sys.databases WHERE name = '${Env:databaseName}')
          BEGIN
            CREATE DATABASE [${Env:databaseName}]
          END
"@
          
          $cmd = $conn.CreateCommand()
          $cmd.CommandText = $sqlCommand
          $cmd.ExecuteNonQuery()
          # Clean up
          $conn.Close()

          break
        } catch {
          Write-Error $_.Exception
          Start-Sleep -Seconds 120
          $curTry++
        }
      } while ($curTry -lt $maxRetries)
    '''
    retentionInterval: 'PT1H'
  }
}

resource generateSqlAdminCreds 'Microsoft.Resources/deploymentScripts@2023-08-01' = {
  name: 'generateSqlUsernamePassword'
  location: location
  kind: 'AzurePowerShell'
  identity: {
    type: 'UserAssigned'
    userAssignedIdentities: {
      '${containerAppIdentity.id}' : {}
    }
  }
  dependsOn: [
    keyVaultSecretsOfficerRoleModule
  ]
  properties: {
    azPowerShellVersion: '10.0'
    forceUpdateTag: forceUpdateTag
    environmentVariables: [
      {
        name: 'keyVaultName'
        value: keyVault.name
      }
      {
        name: 'sqlAdminPassSecretName'
        value: sqlAdminPassSecretName
      }
      {
        name: 'sqlAdminUserSecretName'
        value: sqlAdminUserSecretName
      }
      {
        name: 'subscriptionId'
        value: subscriptionId
      }
    ]
    scriptContent: '''
      # Enable error logging
      $ErrorActionPreference = "Continue"
      
      Write-Output "Starting SQL credentials generation script"
      
      try {
        Set-AzContext -subscription ${Env:subscriptionId}
        Write-Output "Azure context set successfully"

        $symbols = '!@#$%^&*'.ToCharArray()
        $alphaNumericCharacterList =  'a'..'z' + 'A'..'Z' + '0'..'9'
        $symbolCharacterList = $alphaNumericCharacterList + $symbols

        #Generates a secure random value with a default length of 14
        function GeneratePassword {
            param(
                [ValidateRange(12, 256)]
                [int]
                $length = 14,
                [switch]
                $useSymbols
            )

            if ($useSymbols)
            {
                $characterList = $symbolCharacterList
            }
            else {
                $characterList = $alphaNumericCharacterList
            }

            do {
                $password = "pdg" + -join (0..$length | % { $characterList | Get-Random }) #Need prefix for username restrictions
                [int]$hasLowerChar = $password -cmatch '[a-z]'
                [int]$hasUpperChar = $password -cmatch '[A-Z]'
                [int]$hasDigit = $password -match '[0-9]'
                [int]$hasSymbol = ($password.IndexOfAny($symbols) -ne -1) -or !$useSymbols

            }
            until (($hasLowerChar + $hasUpperChar + $hasDigit + $hasSymbol) -ge 3)

            $password | ConvertTo-SecureString -AsPlainText
        }

        Write-Output "Checking for existing username in KeyVault"
        $kvUserName = Get-AzKeyVaultSecret -VaultName ${Env:keyVaultName} -Name ${Env:sqlAdminUserSecretName} -AsPlainText
        
        if(!$kvUserName)
        {
            Write-Output "Generating new username"
            $userName = GeneratePassword
            Set-AzKeyVaultSecret -VaultName ${Env:keyVaultName} -Name ${Env:sqlAdminUserSecretName} -SecretValue $userName
            Write-Output "Username stored in KeyVault"
        }
        else {
          Write-Output "Using existing username from KeyVault"
          $userName = $kvUserName | ConvertTo-SecureString -AsPlainText
        }

        Write-Output "Checking for existing password in KeyVault"
        $kvSqlPassword = Get-AzKeyVaultSecret -VaultName ${Env:keyVaultName} -Name ${Env:sqlAdminPassSecretName} -AsPlainText
        if(!$kvSqlPassword)
        {
            Write-Output "Generating new password"
            $sqlPassword = GeneratePassword -useSymbols
            Set-AzKeyVaultSecret -VaultName ${Env:keyVaultName} -Name ${Env:sqlAdminPassSecretName} -SecretValue $sqlPassword
            Write-Output "Password stored in KeyVault"
        }
        else {
          Write-Output "Using existing password from KeyVault"
          $sqlPassword = $kvSqlPassword | ConvertTo-SecureString -AsPlainText
        }

        Write-Output "Setting deployment script outputs"
        $DeploymentScriptOutputs = @{}
        # Fix variable name typo - using $userName instead of $username
        $DeploymentScriptOutputs['username'] = (ConvertFrom-SecureString -SecureString $userName -AsPlainText)
        $DeploymentScriptOutputs['password'] = (ConvertFrom-SecureString -SecureString $sqlPassword -AsPlainText)
        Write-Output "Deployment script outputs set successfully"
        
        Write-Output "Script completed successfully"
      }
      catch {
        Write-Output "Exception occurred in SQL credentials generation script"
        Write-Error "Exception details: $_"
        throw  # rethrow the exception after logging it
      }
    '''
    cleanupPreference: 'OnExpiration'
    retentionInterval: 'PT3H'
  }
}

resource synapseAllowEv2 'Microsoft.Synapse/workspaces/firewallRules@2021-06-01' = {
  name: 'AllowAllWindowsAzureIps'
  parent: synapseWorkspace
  properties: {
    endIpAddress: '0.0.0.0'
    startIpAddress: '0.0.0.0'
  }
}

resource synapseOwnerRoleDef 'Microsoft.Authorization/roleDefinitions@2022-04-01' existing = {
  scope: synapseWorkspace
  name: ownerRoleDefName
}

resource acrPullRoleAssignment 'Microsoft.Authorization/roleAssignments@2022-04-01' = {
  name: guid(synapseWorkspaceName, ownerRoleDefName, assistedIdAppObjectId)
  scope: synapseWorkspace
  properties: {
    roleDefinitionId: synapseOwnerRoleDef.id
    principalId: assistedIdAppObjectId
    principalType: 'ServicePrincipal'
  }
}

module eventHubNamespaceRoleModule 'eventHubNamespaceRoleAssignment.bicep' = {
  name: 'sharedEventHubNamespaceRoleDeploy'
  scope: resourceGroup(coreResourceGroupName)
  params: {
    eventHubNamespaceName: sharedEventHubNamespaceName
    principalId: containerAppIdentity.properties.principalId
    roleDefinitionName: azureEventHubsDataReceiverRoleDefName
  }
}
module secondaryEventHubNamespaceRoleModule 'eventHubNamespaceRoleAssignment.bicep' = {
  name: 'secondarySharedEventHubNamespaceRoleDeploy'
  scope: resourceGroup(coreResourceGroupName)
  params: {
    eventHubNamespaceName: secondarySharedEventHubName
    principalId: containerAppIdentity.properties.principalId
    roleDefinitionName: azureEventHubsDataReceiverRoleDefName
  }
}

// TODO: Need to move shared event hub creation code to shared infrastructure repo.
// TODO: Uncomment event hub code

module sharedEventHubModule 'eventHub.bicep' = [for eventHub in eventHubs: {
  name: 'sharedEventHubDeploy${eventHub.name}'
  scope: resourceGroup(coreResourceGroupName)
  params: {
    eventHubNamespaceName: sharedEventHubNamespaceName
    eventHubName: eventHub.name
    partitionCount: eventHub.partitionCount
    messageRetentionDays: eventHub.messageRetentionDays
  }
}]


module sharedEventHubConsumerGroupModule 'eventHubConsumerGroups.bicep' = [for eventHub in eventHubs: {
  name: 'sharedEventHubConsumerGroupDeploy${eventHub.name}'
  scope: resourceGroup(coreResourceGroupName)
  dependsOn: [sharedEventHubModule]
  params: {
    eventHubNamespaceName: sharedEventHubNamespaceName
    eventHubName: eventHub.name
    consumerGroupName: consumerGroupName
  }
}]

output containerAppIdentityClientId string = containerAppIdentity.properties.clientId
output containerAppIdentityObjectId string = containerAppIdentity.properties.principalId
output synapseWorkspacePrincipalId string = synapseWorkspace.identity.principalId


//Remove after EH migration
module catalogEventHubNamespaceRoleModule 'eventHubNamespaceRoleAssignment.bicep' = {
  name: 'catalogEventHubNamespaceRoleDeploy'
  scope: resourceGroup(catalogSubscriptionId, catalogResourceGroupName)
  params: {
    eventHubNamespaceName: catalogEventHubName
    principalId: containerAppIdentity.properties.principalId
    roleDefinitionName: azureEventHubsDataReceiverRoleDefName
  }
}

module tempEventHubNamespaceRoleModule 'eventHubNamespaceRoleAssignment.bicep' = {
  name: 'tempeventHubNamespaceRoleDeploy'
  scope: resourceGroup(coreResourceGroupName)
  params: {
    eventHubNamespaceName: tempEventHubName
    principalId: containerAppIdentity.properties.principalId
    roleDefinitionName: azureEventHubsDataReceiverRoleDefName
  }
}

module catalogEventHubConsumerGroupModule 'eventHubConsumerGroups.bicep' = [for eventHub in eventHubs: {
  name: 'catalogEventHubConsumerGroupDeploy${eventHub.name}'
  scope: resourceGroup(catalogSubscriptionId, catalogResourceGroupName)
  params: {
    eventHubNamespaceName: catalogEventHubName
    eventHubName: eventHub.name
    consumerGroupName: consumerGroupName
  }
}]
