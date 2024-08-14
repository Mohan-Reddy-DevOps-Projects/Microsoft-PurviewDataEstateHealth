# Introduction 
This is the Data Engineering Repository for Purview Data Governance. The repository includes DomainModel (3NF) and DataProductModel (Fact-Dimension).

# Getting Started
The repository includes DomainModel and DataProductModel, DEHFabricSync (BYOCT) all of them are Scala code repositories. Dependencies includes - 
1.	IDE - Intellij IDEA Community Edition 22.1.4, ensure adding the following plugins - "Scala", "Azure Toolkit for Intellij", "Maven Helper", "Azure DevOps".
2.	JarExplorer-jdk1.5-2.2.1
3.	Java Version 1.8.0_401
4.	Java(TM) SE Runtime Environment build 1.8.0_401-b10
6.  Apache-Maven-3.9.6
7.  Download winutils and Set HADOOP_HOME Environment Variable on your local.
8.  Set Environment Variables for JRE, JDK, Maven on your local.
9.  Additional Spark and Scala Dependencies Described in the POM.XML of respective repositories.

# Build and Test
To build the code execute "mvn clean package" from Intellij IDE Terminal. It will create 2 JAR executables.
## DomainModel
1.  dataestatehealthanalytics-domainmodel-azure-purview-1.0-jar.jar
2.  dataestatehealthanalytics-domainmodel-azure-purview-1.0-tests.jar

## DimensionalModel
1.  dataestatehealthanalytics-dimensionalmodel-azure-purview-1.0-jar.jar
2.  dataestatehealthanalytics-dimensionalmodel-azure-purview-1.0-tests.jar

## DEHFabricSync
1.  dataestatehealthanalytics-dehfabricsync-azure-purview-1.0-jar.jar
2.  dataestatehealthanalytics-dehfabricsync-azure-purview-1.0-tests.jar

Target Spark Compute Cluster Spark Configurations: 

 - fs.azure.account.auth.type.<storageaccount>.dfs.core.windows.net,SAS
 - fs.azure.sas.token.provider.type.<storageaccount>.dfs.core.windows.net,org.apache.hadoop.fs.azurebfs.sas.FixedSASTokenProvider
 - fs.azure.sas.fixed.token.<storageaccount>.dfs.core.windows.net,<key>
 - spark.microsoft.delta.optimizeWrite.enabled,true
 - spark.serializer,org.apache.spark.serializer.KryoSerializer
 - spark.jars.packages,com.github.scopt:scopt_2.12:4.0.1

1. The generated **DomainModel** Main class JAR accepts 4 parameters 
   - CosmosDBLinkedServiceName (analytical cosmosdb linked service name configured on target synapse instance)
   - adlsTargetDirectory (path of the target DomainModel example abfss://<container>@<storageaccount>.dfs.core.windows.net/DomainModel)
   - accountId (Purview Instance AccountId GUID)
   - refreshType (full or incremental)
   - ReProcessingThresholdInMins (If the Asset Has been Processed in Last X Mins, it is going to SKIP it)
   - JobRunGuid Unique JobRunGuid (CorrelationId)<br />

2. The generated **DimensionalModel** Main Class JAR accepts 2 parameters
   - AdlsTargetDirectory
   - ReProcessingThresholdInMins
   --AccountId (Purview Instance AccountId GUID)
   --JobRunGuid Unique JobRunGuid (CorrelationId)

3. The generated **DEHFabricSync** Main Class JAR accepts 5 parameters
   - DEHStorageAccount
   - FabricSyncRootPath
   - AccountId
   - ProcessDomainModel
   - ProcessDimensionaModel
   
### Arguments Example 
**DomainModel**: --CosmosDBLinkedServiceName <synapseAnalyticalCosmosLinkedServiceName> --AdlsTargetDirectory abfss://<container>@<storageaccount>.dfs.core.windows.net/DomainModel --AccountId <accountId> --RefreshType <incremental/full> --ReProcessingThresholdInMins <Int> --JobRunGuid <UUID> <br />
**DimensionalModel**: --AdlsTargetDirectory abfss://<container>@<storageaccount>.dfs.core.windows.net --ReProcessingThresholdInMins <Int> --AccountId <accountId> --JobRunGuid <UUID> <br />
**DEHFabricSync**: --DEHStorageAccount abfss://<container>@<storageaccount>.<zNumber>.dfs.storage.azure.net --FabricSyncRootPath abfss://<groupId>@msit-onelake.dfs.fabric.microsoft.com/<LakehouseId>/Files --AccountId <Account> --ProcessDomainModel <Boolean example true> --ProcessDimensionaModel <Boolean example false> <br />

# Build Pipeline on Azure DevOps

Any changes pushed to the `main` or `user` branch will trigger the build pipeline [DataEstateHealth](https://msdata.visualstudio.com/Purview%20Data%20Governance/_build?definitionId=29692&_a=summary).

## Maven Repository Compliance

In accordance with Microsoft compliance guidelines, Maven packages must be sourced from the Microsoft Maven repository. 
The `pom.xml` file for the DEH solution is configured to point to this repository. The build pipeline is giong to get all the maven dependencies from this repo and it will use PAT (personal access token) through `settings.xml` file to connect to the Microsoft maven repo.

```xml
    <repositories>
        <repository>
            <id>central</id>
            <url>https://msdata.pkgs.visualstudio.com/4031b34e-6354-4257-94de-a85346a777ae/_packaging/PurviewDataGov/maven/v1</url>
            <releases>
                <enabled>true</enabled>
            </releases>
            <snapshots>
                <enabled>true</enabled>
            </snapshots>
        </repository>
    </repositories>
    <pluginRepositories>
        <pluginRepository>
            <id>central</id>
            <url>https://msdata.pkgs.visualstudio.com/4031b34e-6354-4257-94de-a85346a777ae/_packaging/PurviewDataGov/maven/v1</url>
        </pluginRepository>
    </pluginRepositories>
```

### Access Configuration - `settings.xml`

The build pipeline (build agent) uses the `settings.xml` file to access the Microsoft Maven repository. 
Authentication to the repository is managed through the `settings.xml` file using a Personal Access Token (PAT).
Here is the content of `settings.xml`, the password should the PAT. 
Here is the [link](https://msdata.visualstudio.com/_usersSettings/tokens) to generate PAT.

```xml
<settings xmlns="http://maven.apache.org/SETTINGS/1.0.0" 
        xmlns:xsi="http://www.w3.org/2001/XMLSchema-instance"
        xsi:schemaLocation="http://maven.apache.org/SETTINGS/1.0.0
                             https://maven.apache.org/xsd/settings-1.0.0.xsd">
 <servers>
   <server>
     <id>central</id>
     <username>msdata</username>
     <password></password>
   </server>
 </servers>
</settings>
```

### Location of `settings.xml` File

The [settings.xml](https://msdata.visualstudio.com/Purview%20Data%20Governance/_library?itemType=SecureFiles) file can be found under **Library** → **Secure files**.

### Update settings.xml file

The PAT typically expires after 7 days, but it can be configured to last up to 30 days. If the build pipeline encounters 
an authentication error during the Maven build step, potential reason for failure could be the PAT token has expired. In this case, 
regenerate the PAT and update the `settings.xml` file accordingly.

After updating the `settings.xml` file locally, you must delete and re-upload it, as direct updates are not allowed.

### Grant access to Build Pipeline and other users

Once a new token is generated and updated in the `settings.xml` file, grant access to build pipelines.
Note: everytime the `security.xml` file is uploaded, grant access to build pipeline and other users within team.

- Navigate to **Library** → **Secure files**.
- Click on security.xml file and go to pipeline permissions.
- add following pipelines
   - DataEstateHealth.CI
   - DataEstateHealth.Build.PullRequest
   - DataEstateHealth-Release.Official
   - DataEstateHealth.Release.Demo
   - DataEstateHealth.Release.Dogfood
   - DataEstateHealth.Release.Dev
   - DataEstateHealth.Release.Prod
   - DataEstateHealth.Build.CodeQL
   - DataEstateHealth.Buddy-t

# Contribute
References 
- [guidelines](https://learn.microsoft.com/en-us/azure/synapse-analytics/spark/apache-spark-overview)
- [Create a Scala Maven application for Apache Spark](https://learn.microsoft.com/en-us/azure/hdinsight/spark/apache-spark-create-standalone-application)

# Contact
Keshav Singh keshav.singh@microsoft.com