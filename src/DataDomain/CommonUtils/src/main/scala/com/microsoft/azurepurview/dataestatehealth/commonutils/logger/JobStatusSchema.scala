package com.microsoft.azurepurview.dataestatehealth.commonutils.logger

case class JobStatusSchema(Id: String,
                           AccountId: String,
                           JobId: String,
                           JobName: String,
                           JobStatus: String,
                           JobCompletionTime: String)
