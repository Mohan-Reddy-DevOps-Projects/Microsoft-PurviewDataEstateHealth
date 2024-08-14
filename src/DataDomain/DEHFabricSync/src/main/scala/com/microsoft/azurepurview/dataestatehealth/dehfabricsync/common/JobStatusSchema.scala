package com.microsoft.azurepurview.dataestatehealth.dehfabricsync.common

case class JobStatusSchema(Id: String,
                           AccountId: String,
                           JobId: String,
                           JobName: String,
                           JobStatus: String,
                           JobCompletionTime: String)
