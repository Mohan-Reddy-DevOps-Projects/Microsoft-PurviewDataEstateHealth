package com.microsoft.azurepurview.dataestatehealth.domainmodel.dataasset

import com.microsoft.azurepurview.dataestatehealth.domainmodel.common.{Reader, Validator}
import org.apache.log4j.Logger
import org.apache.spark.sql.expressions.Window
import org.apache.spark.sql.functions.{coalesce, col, lit, row_number, when}
import org.apache.spark.sql.types.{IntegerType, LongType, StringType, StructField, StructType, TimestampType}
import org.apache.spark.sql.{DataFrame, SparkSession, types, Row}

import java.sql.Timestamp

class DataAssetDomainAssignment (spark: SparkSession, logger:Logger){

  /**
   * Joins data product asset assignments with business domain assignments to map data assets to business domains.
   *
   * This method performs the following steps:
   * 1. Filters active records from the input DataFrames.
   * 2. Joins the filtered DataFrames on `DataProductId`.
   * 3. Selects relevant columns and adds an `OperationType` column.
   * 4. Removes duplicate records caused by one-to-many relationships:
   *    - The relationship from business domains to data products and from data products to data assets is one-to-many,
   *      which can result in multiple records for each combination of `DataAssetId` and `BusinessDomainId`.
   *    - Uses a window function to retain only the latest active record for each unique combination,
   *      based on `ModifiedDateTime` and `EventProcessingTime`.
   *
   * If any input DataFrame is missing, an empty DataFrame with the specified schema is returned.
   *
   * @param dfDataProductAssetAssignment An optional DataFrame with data product asset assignments.
   * @param dfDataProductBusinessDomainAssignment An optional DataFrame with data product business domain assignments.
   * @param schema Schema for the resulting DataFrame if any input DataFrame is missing.
   * @return A DataFrame containing the mapped data assets and business domains, or an empty DataFrame
   *         with the provided schema if any input DataFrame is missing.
   */
  def extractDataAssetDomainMapping(dfDataProductAssetAssignment: Option[DataFrame],
                                    dfDataProductBusinessDomainAssignment:Option[DataFrame],
                                    schema: StructType):DataFrame={
    try{
      (dfDataProductAssetAssignment, dfDataProductBusinessDomainAssignment) match {
        case (Some(dfProductAsset), Some(dfProductDomain)) =>

          // Filter active records
          val dfProductAssetActive = dfProductAsset
            /*.withColumn("row_number", row_number().over(
              Window.partitionBy("DataProductId","DataAssetId")
                .orderBy(
                  coalesce(col("ModifiedDateTime").cast(TimestampType),
                    lit(Timestamp.valueOf("2000-01-01 00:00:00"))).desc,
                  col("EventProcessingTime").desc
                )
            ))
            .filter(col("row_number") === 1)
            .drop("row_number")
            */.filter(col("ActiveFlag") === 1)

          val dfProductDomainActive = dfProductDomain
            /*.withColumn("row_number", row_number().over(
              Window.partitionBy("BusinessDomainId","DataProductId")
                .orderBy(
                  coalesce(col("ModifiedDateTime").cast(TimestampType),
                    lit(Timestamp.valueOf("2000-01-01 00:00:00"))).desc,
                  col("EventProcessingTime").desc
                )
            ))
            .filter(col("row_number") === 1)
            .drop("row_number")
            */.filter(col("ActiveFlag") === 1)

          // Join active DataFrames on `DataProductId` and select relevant columns
          val dfAssetDomain = dfProductAssetActive.alias("a")
            .join(dfProductDomainActive.alias("b"), Seq("DataProductId"), "inner")
            .select(col("a.DataAssetId"),
              col("b.BusinessDomainId"),
              col("a.AssignedByUserId"),
              col("a.ActiveFlagLastModifiedDateTime"),
              col("a.AssignmentLastModifiedDatetime"),
              col("a.ActiveFlag"),
              col("a.ModifiedDateTime"),
              col("a.ModifiedByUserId"),
              col("a.EventProcessingTime"))
            .withColumn("OperationType", lit("Create"))

          // Remove duplicates by retaining only the latest record per combination
          dfAssetDomain
            .withColumn("row_number", row_number().over(
              Window.partitionBy("DataAssetId", "BusinessDomainId")
                .orderBy(
                  coalesce(col("ModifiedDateTime").cast(TimestampType),
                    lit(Timestamp.valueOf("2000-01-01 00:00:00"))).desc
                )
            ))
            .filter(col("row_number") === 1)
            .drop("row_number")
            .distinct()
        case _ =>
          // Handle the case where either DataFrame is missing
          spark.createDataFrame(spark.sparkContext.emptyRDD[Row], schema=schema)
      }
    }
    catch {
      case e: Exception =>
        println(s"Error Processing DataAssetDomainAssignment Data: ${e.getMessage}")
        logger.error(s"Error Processing DataAssetDomainAssignment Data: ${e.getMessage}")
        throw e
    }
  }
}
