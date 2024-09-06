package com.microsoft.azurepurview.dataestatehealth.computegovernedassets.common

case class JobStatusSchema(Id: String,
                           AccountId: String,
                           JobId: String,
                           JobName: String,
                           JobStatus: String,
                           JobCompletionTime: String,
                           TenantId: String,
                           Result: String = "")