package com.microsoft.azurepurview.dataestatehealth.storagesync.common

case class MainConfig(
                       DEHStorageAccount: String = "",
                       SyncRootPath: String = "",
                       SyncType: String = "",
                       AccountId: String = "",
                       JobRunGuid: String = ""
                     )
