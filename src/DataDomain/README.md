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

# Contribute
References 
- [guidelines](https://learn.microsoft.com/en-us/azure/synapse-analytics/spark/apache-spark-overview)
- [Create a Scala Maven application for Apache Spark](https://learn.microsoft.com/en-us/azure/hdinsight/spark/apache-spark-create-standalone-application)

# Contact
Keshav Singh keshav.singh@microsoft.com