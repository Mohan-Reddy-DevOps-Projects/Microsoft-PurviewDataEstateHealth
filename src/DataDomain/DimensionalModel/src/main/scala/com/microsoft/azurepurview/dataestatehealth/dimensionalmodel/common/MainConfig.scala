package com.microsoft.azurepurview.dataestatehealth.dimensionalmodel.common

case class MainConfig(
                       AdlsTargetDirectory: String = "",
                       ReProcessingThresholdInMins:Int=0,
                       AccountId: String = "",
                       JobRunGuid:String =""
                     )