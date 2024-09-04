package com.microsoft.azurepurview.dataestatehealth.computegovernedassets.main

case class ComputeGovernedAssetsCountResult(CountOfAssetsInDataMap: Long,
                                            CountOfAssetsInDG: Long,
                                            CountOfGovernedAssets: Long,
                                            ExceptionMsg: String)

