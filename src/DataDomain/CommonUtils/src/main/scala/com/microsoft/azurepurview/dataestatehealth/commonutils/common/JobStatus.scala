package com.microsoft.azurepurview.dataestatehealth.commonutils.common

object JobStatus extends Enumeration {
  type JobStatus = Value
  val Started, Completed, Failed, Cancelled = Value

}
