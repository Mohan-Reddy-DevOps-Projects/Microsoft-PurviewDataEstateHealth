package com.microsoft.azurepurview.dataestatehealth.domainmodel.common

case class MainConfig(
                       CosmosDBLinkedServiceName: String = "",
                       AdlsTargetDirectory: String = "",
                       AccountId: String = "",
                       RefreshType: String = "",
                       ReProcessingThresholdInMins: Int = 0,
                       JobRunGuid:String =""
                     )
