package com.microsoft.azurepurview.dataestatehealth.storagesync.common

case class JobStatusSchema(Id: String,
                           AccountId: String,
                           JobId: String,
                           JobName: String,
                           JobStatus: String,
                           JobCompletionTime: String,
                           TenantId: String)
