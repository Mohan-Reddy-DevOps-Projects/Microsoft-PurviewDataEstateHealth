package com.microsoft.azurepurview.dataestatehealth.computegovernedassets.main

case class ComputeGovernedAssetsCountResult(CountOfAssetsInDataMap: Long,
                                            CountOfAssetsWithTermInDataMap: Long,
                                            CountOfAssetsInDG: Long,
                                            CountOfGovernedAssets: Long,
                                            ExceptionMsg: String)

