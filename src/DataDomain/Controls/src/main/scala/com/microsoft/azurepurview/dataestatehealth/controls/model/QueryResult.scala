package com.microsoft.azurepurview.dataestatehealth.controls.model

import org.apache.spark.sql.DataFrame

/**
 * Represents the result of a control job query execution
 *
 * @param dataFrame The resulting DataFrame from the query
 * @param rules The rules to be applied to the results
 * @param controlId The ID of the control
 */
case class QueryResult(
  dataFrame: DataFrame,
  rules: Seq[Map[String, String]],
  controlId: String
) 