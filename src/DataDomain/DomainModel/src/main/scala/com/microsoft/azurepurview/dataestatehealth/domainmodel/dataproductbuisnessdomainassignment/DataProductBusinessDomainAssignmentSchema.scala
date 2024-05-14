package com.microsoft.azurepurview.dataestatehealth.domainmodel.dataproductbuisnessdomainassignment

import org.apache.spark.sql.types.{IntegerType, StringType, StructField, StructType, TimestampType}

class DataProductBusinessDomainAssignmentSchema {
  val dataProductBusinessDomainAssignmentSchema: StructType = StructType(
    Array(
      StructField("DataProductID", StringType, nullable = false),
      StructField("BusinessDomainId", StringType, nullable = false),
      StructField("AssignedByUserId", StringType, nullable = true),
      StructField("AssignmentDateTime", TimestampType, nullable = true),
      StructField("ActiveFlag", IntegerType, nullable = false),
      StructField("ActiveFlagLastModifiedDateTime", TimestampType, nullable = false)
    )
  )

}
