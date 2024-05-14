package com.microsoft.azurepurview.dataestatehealth.dimensionalmodel.dimension
import com.microsoft.azurepurview.dataestatehealth.dimensionalmodel.common.{Validator}
import org.apache.log4j.Logger
import org.apache.spark.sql.{DataFrame, Row, SparkSession}
import org.apache.spark.sql.types.{StructField, StructType}

class DimDataHealthControl (spark: SparkSession, logger:Logger){
  def processDimDataHealthControl(schema: org.apache.spark.sql.types.StructType):DataFrame= {
    try {

      val dataValues = Seq(
        //(1,"Data Inventory", "Data Estate Curation", "Data sources and data assets are registered in an inventory", "All Data Products", "Control 3"),
        (2,"Classification and Labeling", "Estate Curation", "Business domains and data products are classified and labeled for SITs and information protection", "Data Products", "Control 6"),
        (3,"Data Product Ownership", "Trusted Data", "Data products have authorized owners. An authorized owner is a user with the data product ownership role for the data product’s business domain.", "All Data Products", "Control 2"),
        (4,"Data estate health monitoring, alerting, and insights", "Estate Observability", "Observability for controls compliance across the data estate", "All Data Products", "Control 1"),
        (5,"Data Products Connection", "Discoverability And Understanding", "Data assets are mapped to data products for discoverability in the catalog", "Data Products", "NA"),
        (6,"Data Quality Measurement", "Trusted Data", "Data quality scoring is enabled for business domains, data products, and data assets, and is visible in the data catalog", "All Data Products", "Control 12"),
        //(7,"Data Quality Score","Trusted Data","The overall composite DQ score of the data estate computed across all business domains, data products, and data assets", "All Data Products", "NA"),
        (8,"Business OKRs Alignment", "Value Creation", "Data products serve 1 or more business OKRs", "All Data Products", "NA"),
        (9,"Critical Data Identification", "Estate Curation", "Business critical data entities and attributes are defined for business domains", "Business Domains", "NA"),
        (10,"Critical Data Connection", "Estate Curation", "Assets are mapped to CDE", "CDE", "NA"),
        //(11,"Discoverability and Understanding Score", "Data Discoverability and Understanding", "CDE is published by authorized CDE owners", "CDE", "NA"),
        (12,"Self-serve Access Enablement", "Access And Use", "Data products have self-serve policies and workflow(s) enabled to enable data consumers to self-serve in subscribing and managing subscriptions", "All Data Products", "Control 7"),
        (13,"Compliant Data Use", "Access And Use", "Data use purpose must be provided by data consumer when subscribing to a data product, and must be compliant with data use policies", "All Data Products", "Control 8"),
        (14,"Data Cataloging", "Discoverability And Understanding", "Data products are published by authorized data product owners for discoverability in the catalog. An authorized owner is a user with the data product ownership role for the data product’s business domain.", "All Data Products", "Control 5"),
        (15,"Data Security and Compliance", "Trusted Data", "Data security and compliance policies are met by business domains, data products, and data assets.", "All Data Products", "Controls 4, 9, 10, 11"),
        //(16,"Discoverability and Understanding Measurement", "Data Discoverability and Understanding (MDQ)", "MDQ scoring is enabled for the discoverability and understanding of business domains and data products", "All Data Products", "Control 14"),
        //(17,"Discoverability and Understanding Score", "Data Discoverability and Understanding (MDQ)", "A single composite MDQ score measuring the overall discoverability and understanding of the data estate.", "All Data Products", "Control 14"),
        (18,"Data Product Certification", "Trusted Data", "Data products are certified by an authorized data product owner. An authorized owner is a user with the data product ownership role for the data product’s business domain.", "All Data Products", "Control 3"),
        (19,"Access Request SLA", "Access And Use", "Access requests for data products are addressed within published SLAs", "All Data Products", "NA"),
        (20,"Zero-Trust Default Access", "Access And Use", "Data products should by default only be accessible by their owners", "All Data Products", "Control 7"),
        (21,"Operational Costs", "Value Creation", "Operational costs are reported for business domains, data products, and data assets", "All Data Products", "Control 13"),
        (22,"Business Value", "Value Creation", "Business value outcomes from OKRs served are reported for data products", "All Data Products", "NA"),
        (23,"Business Alignment", "Estate Curation", "Physical data collections and platform domains in the data map are mapped to at least 1 business domain", "Business Domains", "NA"),
        (24,"Linked Assets", "Metadata Quality Management", "NA", "All Data Products", "NA"),
        (25,"Ownership", "Metadata Quality Management", "NA", "All Data Products", "NA"),
        (26,"Usability", "Metadata Quality Management", "NA", "All Data Products", "NA"),
        (27,"Accuracy", "Data Quality Management", "NA", "All Data Products", "NA"),
        (28,"Completeness", "Data Quality Management", "NA", "All Data Products", "NA"),
        (29,"Conformity", "Data Quality Management", "NA", "All Data Products", "NA"),
        (30,"Consistency", "Data Quality Management", "NA", "All Data Products", "NA"),
        (31,"Timeliness", "Data Quality Management", "NA", "All Data Products", "NA"),
        (32,"Uniqueness", "Data Quality Management", "NA", "All Data Products", "NA")
      )

      val rowRDD = spark.sparkContext.parallelize(dataValues.map(Row.fromTuple))
      val dfProcess = spark.createDataFrame(rowRDD, schema)

      val filterString =
        s"""HealthControlId is null
           | or HealthControlDisplayName is null
           | or HealthControlGroupDisplayName is null""".stripMargin
      val validator = new Validator()
      validator.validateDataFrame(dfProcess, filterString)

      dfProcess

    }
    catch {
      case e: Exception =>
        println(s"Error Processing DimDataHealthControl Dimension: ${e.getMessage}")
        logger.error(s"Error Processing DimDataHealthControl Dimension: ${e.getMessage}")
        throw e
    }
  }
  }
