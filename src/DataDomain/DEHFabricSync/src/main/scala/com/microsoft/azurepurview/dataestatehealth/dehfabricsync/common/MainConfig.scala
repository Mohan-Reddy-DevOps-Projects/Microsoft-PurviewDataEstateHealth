package com.microsoft.azurepurview.dataestatehealth.dehfabricsync.common

case class MainConfig(
                       DEHStorageAccount: String = "",
                       FabricSyncRootPath: String = "",
                       AccountId: String = "",
                       ProcessDomainModel: Boolean = true,
                       ProcessDimensionalModel: Boolean = true,
                       JobRunGuid: String = ""
                     )
