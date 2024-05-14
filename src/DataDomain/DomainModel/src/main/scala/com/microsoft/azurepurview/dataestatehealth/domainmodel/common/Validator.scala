package com.microsoft.azurepurview.dataestatehealth.domainmodel.common
import org.apache.spark.sql.DataFrame

class Validator {
  def validateDataFrame(df: DataFrame, filter:String): Unit = {
    val invalidRows = df.filter(filter)
    if (!invalidRows.isEmpty) {
      throw new IllegalArgumentException(s"Invalid data for condition: $filter")
    }
  }
}
