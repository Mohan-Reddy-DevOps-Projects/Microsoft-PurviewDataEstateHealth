package com.microsoft.azurepurview.dataestatehealth.domainmodel.businessdomain

import org.apache.spark.sql.types.{BooleanType, IntegerType, StringType, StructField, StructType, TimestampType, LongType}

class BusinessDomainSchema {
  val businessDomainAssetSchema: StructType = StructType(
    Array(
      StructField("BusinessDomainId", StringType, nullable = false),
      StructField("ParentBusinessDomainId", StringType, nullable = true),
      StructField("BusinessDomainName", StringType, nullable = false),
      StructField("BusinessDomainDisplayName", StringType, nullable = true),
      StructField("BusinessDomainDescription", StringType, nullable = true),
      StructField("Status", StringType, nullable = true),
      StructField("IsRootDomain", BooleanType, nullable = true),
      StructField("HasValidOwner", BooleanType, nullable = true),
      StructField("AccountId", StringType, nullable = false),
      StructField("CreatedDatetime", TimestampType, nullable = true),
      StructField("CreatedByUserId", StringType, nullable = true),
      StructField("ModifiedDateTime", TimestampType, nullable = true),
      StructField("ModifiedByUserId", StringType, nullable = true),
      StructField("EventProcessingTime", LongType, nullable = false),
      StructField("OperationType", StringType, nullable = false)
    )
  )
}