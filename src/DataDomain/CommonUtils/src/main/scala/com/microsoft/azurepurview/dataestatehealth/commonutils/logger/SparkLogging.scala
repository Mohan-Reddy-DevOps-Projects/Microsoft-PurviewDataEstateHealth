package com.microsoft.azurepurview.dataestatehealth.commonutils.logger

import org.slf4j.Logger
import org.slf4j.LoggerFactory

trait SparkLogging {
  protected val logger: Logger = LoggerFactory.getLogger(getClass.getName)
}
