from pyspark.sql import SparkSession
import sys

if __name__ == "__main__":


    #create spark session
    spark = SparkSession \
    .builder \
    .appName("python spark test") \
    .config("spark.some.config.option", "some-value") \
    .getOrCreate()
    
    path=sys.argv[1]
    spark.sparkContext.addPyFile(path)
    
     
    from pyspark.sql import SparkSession
    from pyspark.sql.types import *
    from pyspark.sql.functions import *
    from DataEstateHealthLibrary.Catalog.Terms.build_term import BuildTerm
    from DataEstateHealthLibrary.Catalog.DataProduct.build_data_product import BuildDataProduct
    from DataEstateHealthLibrary.Catalog.DataAsset.build_data_asset import BuildDataAsset
    from DataEstateHealthLibrary.Catalog.BusinessDomain.build_business_domain import BuildBusinessDomain
    from DataEstateHealthLibrary.Catalog.Relationship.build_relationship import BuildRelationship
    from DataEstateHealthLibrary.ActionCenter.build_action_center import BuildActionCenter
    from DataEstateHealthLibrary.HealthSummary.build_health_summary import BuildHealthSummary
    from DataEstateHealthLibrary.CdmcScore.build_scores import BuildScores
    from DataEstateHealthLibrary.BusinessDomainTrend.build_businessdomain_trend import BuildBusinessDomainTrend
    from DataEstateHealthLibrary.CdmcControls.build_cdmc_controls import BuildCdmcControls
    from DataEstateHealthLibrary.Catalog.catalog_sink_schema import CatalogSinkSchema
    from DataEstateHealthLibrary.DataAccess.PolicySet.build_policy_set import BuildPolicySet
    from DataEstateHealthLibrary.DataQuality.DataQualityScore.data_quality_sink_schema import DataQualitySinkSchema


    print(sys.argv[1])
    print(sys.argv[2])
    print(sys.argv[3])
    print(sys.argv[4])
    print(sys.argv[5])
    print(sys.argv[6])
    print(sys.argv[7])
    
    container = sys.argv[2]
    source = sys.argv[3]
    sink = sys.argv[4]
    catalog = source + sys.argv[5]
    dataaccess = source+ sys.argv[6]
    catalogdeleted = catalog+sys.argv[7]
    dataaccessdeleted = dataaccess+sys.argv[7]
    
    #FETCH DELETED PRODUCTS
    try:
        deleted_dataproduct_df = spark.read.option('startingVersion', "latest").load(catalogdeleted+'DataProduct', format='delta')
    except Exception as error:
        deleted_dataproduct_df = spark.createDataFrame([], StructType([]))
        
    #FETCH DELETED ASSETS
    try:
        deleted_dataasset_df = spark.read.option('startingVersion', "latest").load(catalogdeleted+'DataAsset', format='delta')
    except Exception as error:
        deleted_dataasset_df = spark.createDataFrame([], StructType([]))

    #FETCH DELETED DOMAINS
    try:
        deleted_businessdomain_df = spark.read.option('startingVersion', "latest").load(catalogdeleted+'BusinessDomain', format='delta')
    except Exception as error:
        deleted_businessdomain_df = spark.createDataFrame([], StructType([]))
        
    #FETCH DELETED TERMS
    try:
        deleted_term_df = spark.read.option('startingVersion', "latest").load(catalogdeleted+'Term', format='delta')
    except Exception as error:
        deleted_term_df = spark.createDataFrame([], StructType([]))

    #FETCH DELETED RELATIONSHIPS
    try:
        deleted_releationship_df = spark.read.option('startingVersion', "latest").load(catalogdeleted+'Relationship', format='delta')
    except Exception as error:
        deleted_releationship_df = spark.createDataFrame([], StructType([]))

    #FETCH DELETED POLICY SET
    try:
        deleted_policyset_df = spark.read.option('startingVersion', "latest").load(dataaccessdeleted+'PolicySet', format='delta')
    except Exception as error:
        deleted_policyset_df = spark.createDataFrame([], StructType([]))

    print("fetched_deleted")

    #TERM SOURCE
    try:
        source_term_df = spark.read.option('startingVersion', "latest").load(catalog+'Term', format='delta')
    except Exception as error:
        source_term_df = spark.createDataFrame([], StructType([]))

    #DATA PRODUCT SOURCE
    try:
        source_dataproduct_df = spark.read.option('startingVersion', "latest").load(catalog+'DataProduct', format='delta')
    except Exception as error:
        source_dataproduct_df = spark.createDataFrame([], StructType([]))

    #BUSINESS DOMAIN SOURCE
    try:
        source_businessdomain_df = spark.read.option('startingVersion', "latest").load(catalog+'BusinessDomain', format='delta')
    except Exception as error:
        source_businessdomain_df = spark.createDataFrame([], StructType([]))
    
    #DATA ASSET SOURCE
    try:
        source_dataasset_df = spark.read.option('startingVersion', "latest").load(catalog+'DataAsset', format='delta')
    except Exception as error:
        source_dataasset_df = spark.createDataFrame([], StructType([]))

    #RELATIONSHIP SOURCE
    try:
        source_relationship_df = spark.read.option('startingVersion', "latest").load(catalog+'Relationship', format='delta')
    except Exception as error:
        source_relationship_df = spark.createDataFrame([], StructType([]))

    #POLICY SET SOURCE
    try:
        source_policyset_df = spark.read.option('startingVersion', "latest").load(dataaccess+'PolicySet',format='delta')
    except Exception as error:
        source_policyset_df = spark.createDataFrame([], StructType([]))

    #ACTION CENTER EXISTING SINK
    try:
        existing_action_df = spark.read.option('startingVersion', "latest").load(sink+'ActionCenter', format='delta')
    except Exception as error:
        existing_action_df = spark.createDataFrame([], CatalogSinkSchema.sink_action_center_schema)

    #PRODUCT QUALITY SCORE SINK
    try:
        sink_productquality_df = spark.read.option('startingVersion', "latest").load(sink+'ProductQualityScore', format='delta')
    except Exception as error:
        sink_productquality_df = spark.createDataFrame([], DataQualitySinkSchema.sink_product_dataqualityscore_schema)
        
    print("fetched_source")

    #GENERATE TERM SINK
    if not source_term_df.isEmpty():
        #HANDLE TERM DELETES
        source_term_df = BuildTerm.handle_term_deletes(source_term_df,deleted_term_df)

        #Generate termdomain_association_df
        termdomain_association_df = BuildTerm.build_term_business_domain_association_schema(source_term_df)
        sink_termdomain_association_df = spark.createDataFrame(termdomain_association_df.rdd, schema=CatalogSinkSchema.sink_term_domain_association_schema)
        sink_termdomain_association_df.write.format("delta").mode("overwrite").save(sink+"TermDomainAssociation")
    
        #Generate termcontact_association_df
        termcontact_association_df = BuildTerm.build_term_contact_association(source_term_df)
        sink_termcontact_association_df = spark.createDataFrame(termcontact_association_df.rdd, schema=CatalogSinkSchema.sink_term_contact_association_schema)
        sink_termcontact_association_df.write.format("delta").mode("overwrite").save(sink+"TermContactAssociation")
    
        #Generate termschema_df
        termschema_df = BuildTerm.build_term_schema(source_term_df)
        sink_termschema_df = spark.createDataFrame(termschema_df.rdd, schema=CatalogSinkSchema.sink_term_schema)
        sink_termschema_df.write.format("delta").mode("overwrite").save(sink+"TermSchema")
        
        print("GENERATED TERM SINK")
    else:
        sink_termdomain_association_df = spark.createDataFrame([], CatalogSinkSchema.sink_term_domain_association_schema)
        sink_termdomain_association_df.write.format("delta").mode("overwrite").save(sink+"TermDomainAssociation")
        
        sink_termcontact_association_df = spark.createDataFrame([], CatalogSinkSchema.sink_term_contact_association_schema)
        sink_termcontact_association_df.write.format("delta").mode("overwrite").save(sink+"TermContactAssociation")
        
        sink_termschema_df = spark.createDataFrame([], CatalogSinkSchema.sink_term_schema)
        sink_termschema_df.write.format("delta").mode("overwrite").save(sink+"TermSchema")
        
        print("GENERATED EMPTY TERM SINK")
        
    #GENERATE DATA PRODUCT SINK ASSOCIATION  
    if not source_dataproduct_df.isEmpty():
        #HANDLE PRODUCT DELETES
        if not deleted_dataproduct_df.isEmpty():
            source_dataproduct_df = BuildDataProduct.handle_dataproduct_deletes(source_dataproduct_df,deleted_dataproduct_df)
        
        #Generate productdomain_association_df
        dataproductdomain_association_df = BuildDataProduct.build_data_product_business_domain_association(source_dataproduct_df)
        sink_dataproductdomain_association_df = spark.createDataFrame(dataproductdomain_association_df.rdd, schema=CatalogSinkSchema.sink_data_product_domain_association_schema)
        sink_dataproductdomain_association_df.write.format("delta").mode("overwrite").save(sink+"DataProductDomainAssociation")
    
        #Generate dataproductcontact_association_df
        dataproductcontact_association_df = BuildDataProduct.build_data_product_contact_association(source_dataproduct_df)
        sink_dataproductcontact_association_df = spark.createDataFrame(dataproductcontact_association_df.rdd, schema=CatalogSinkSchema.sink_data_product_contact_association_schema)
        sink_dataproductcontact_association_df.write.format("delta").mode("overwrite").save(sink+"DataProductContactAssociation")
        
        print("GENERATED PRDOUCT ASSOCIATIONS SINK")
    else:
        sink_dataproductdomain_association_df = spark.createDataFrame([], CatalogSinkSchema.sink_data_product_domain_association_schema)
        sink_dataproductdomain_association_df.write.format("delta").mode("overwrite").save(sink+"DataProductDomainAssociation")
        
        sink_dataproductcontact_association_df = spark.createDataFrame([], CatalogSinkSchema.sink_data_product_contact_association_schema)
        sink_dataproductcontact_association_df.write.format("delta").mode("overwrite").save(sink+"DataProductContactAssociation")
        
        print("GENERATED EMPTY PRDOUCT ASSOCIATIONS SINK")
        
    #GENERATE DATA ASSET SINK     
    if not source_dataasset_df.isEmpty():
        #HANDLE ASSET DELETES
        source_dataasset_df = BuildDataAsset.handle_dataasset_deletes(source_dataasset_df,deleted_dataasset_df)
        
        #Generate dataasset_domain_association_df
        dataasset_domain_association_df = BuildDataAsset.build_asset_domain_association(source_dataasset_df)
        sink_dataasset_domain_association_df = spark.createDataFrame(dataasset_domain_association_df.rdd, schema=CatalogSinkSchema.sink_data_asset_domain_association_schema)
        sink_dataasset_domain_association_df.write.format("delta").mode("overwrite").save(sink+"AssetDomainAssociation")

        #Generate dataassetschema_df
        dataassetschema_df = BuildDataAsset.build_data_asset_schema(source_dataasset_df)
        sink_dataassetschema_df = spark.createDataFrame(dataassetschema_df.rdd, schema=CatalogSinkSchema.sink_data_asset_schema)
        sink_dataassetschema_df.write.format("delta").mode("overwrite").save(sink+"DataAssetSchema")
        sink_dataassetschema_df = BuildDataAsset.update_boolean_to_int(sink_dataassetschema_df)

        #Generate dataassetcontact_association_df
        dataassetcontact_association_df = BuildDataAsset.build_data_asset_contact_association(source_dataasset_df)
        sink_dataassetcontact_association_df = spark.createDataFrame(dataassetcontact_association_df.rdd, schema=CatalogSinkSchema.sink_data_asset_contact_association_schema)
        sink_dataassetcontact_association_df.write.format("delta").mode("overwrite").save(sink+"DataAssetContactAssociation")
        
        print("GENERATED ASSET SINK")
    else:
        sink_dataasset_domain_association_df = spark.createDataFrame([], CatalogSinkSchema.sink_data_asset_domain_association_schema)
        sink_dataasset_domain_association_df.write.format("delta").mode("overwrite").save(sink+"AssetDomainAssociation")
        
        sink_dataassetschema_df = spark.createDataFrame([], CatalogSinkSchema.sink_data_asset_schema)
        sink_dataassetschema_df.write.format("delta").mode("overwrite").save(sink+"DataAssetSchema")
        
        sink_dataassetcontact_association_df = spark.createDataFrame([], CatalogSinkSchema.sink_data_asset_contact_association_schema)
        sink_dataassetcontact_association_df.write.format("delta").mode("overwrite").save(sink+"DataAssetContactAssociation")
        
        print("GENERATED EMPTY ASSET SINK")
        
    #GENERATE BUSINESS DOMAIN SINK     
    if not source_businessdomain_df.isEmpty():
        #HANDLE DOMAIN DELETES
        source_businessdomain_df = BuildBusinessDomain.handle_businessdomain_deletes(source_businessdomain_df,deleted_businessdomain_df)
        
        #Generate businessdomainschema_df
        businessdomainschema_df = BuildBusinessDomain.build_business_domain_schema(source_businessdomain_df, sink_dataproductdomain_association_df, sink_termdomain_association_df)
        sink_businessdomainschema_df = spark.createDataFrame(businessdomainschema_df.rdd, schema=CatalogSinkSchema.sink_business_domain_schema)
        sink_businessdomainschema_df.write.format("delta").mode("overwrite").option("mergeSchema", "true").save(sink+"BusinessDomainSchema")
        sink_businessdomainschema_df = BuildBusinessDomain.update_boolean_to_int(sink_businessdomainschema_df)
        print("GENERATED BUSINESS DOMAIN SINK")
    else:
        sink_businessdomainschema_df = spark.createDataFrame([], CatalogSinkSchema.sink_business_domain_schema)
        sink_businessdomainschema_df.write.format("delta").mode("overwrite").option("mergeSchema", "true").save(sink+"BusinessDomainSchema")
        print("GENERATED EMPTY BUSINESS DOMAIN SINK")
        
    #GENERATE RELATIONSHIP SINK ASSOCIATION     
    if not source_relationship_df.isEmpty():
        #HANDLE RELATIONSHIP DELETES
        source_relationship_df = BuildRelationship.handle_relationship_deletes(source_relationship_df,deleted_releationship_df)
    
        #Generate relationship_association df
        assetproduct_association_df = BuildRelationship.build_asset_product_association(source_relationship_df,sink_dataassetschema_df,source_dataproduct_df)
        sink_assetproduct_association_df = spark.createDataFrame(assetproduct_association_df.rdd, schema=CatalogSinkSchema.sink_product_asset_association_schema)
        sink_assetproduct_association_df.write.format("delta").mode("overwrite").option("mergeSchema", "true").save(sink+"AssetProductAssociation")

        print("GENERATED RELATIONSHIP SINK")
    else:
        sink_assetproduct_association_df = spark.createDataFrame([], CatalogSinkSchema.sink_product_asset_association_schema)
        sink_assetproduct_association_df.write.format("delta").mode("overwrite").option("mergeSchema", "true").save(sink+"AssetProductAssociation")
        print("GENERATED EMPTY RELATIONSHIP SINK")
        
    #GENERATE POLICY SET 
    if not source_policyset_df.isEmpty():
        #HANDLE POLICY SET DELETES
        source_policyset_df = BuildPolicySet.handle_policy_set_deletes(source_policyset_df,deleted_policyset_df)
        
        #Generate data access policy set df
        policyset_df = BuildPolicySet.build_policy_set(source_policyset_df)
        
        print("CREATED POLICY SET SINK")
    else:
        policyset_df = spark.createDataFrame([], StructType([]))
        
        print("CREATED EMPTY POLICY SET SINK")
        
    #GENERATE DATA PRODUCT SINK SCHEMA     
    if not source_dataproduct_df.isEmpty():
        #Generate dataproductschema_df
        dataproductschema_df = BuildDataProduct.build_data_product_schema(source_dataproduct_df,sink_dataassetschema_df,sink_assetproduct_association_df,
                                                                          policyset_df,sink_productquality_df)
        sink_dataproductschema_df = spark.createDataFrame(dataproductschema_df.rdd, schema=CatalogSinkSchema.sink_data_product_schema)
        sink_dataproductschema_df.write.format("delta").mode("overwrite").save(sink+"DataProductSchema")
        sink_dataproductschema_df = BuildDataProduct.update_boolean_to_int(sink_dataproductschema_df)
        
        print("GENERATED DATA PRODUCT SINK")
    else:
        sink_dataproductschema_df = spark.createDataFrame([], CatalogSinkSchema.sink_data_product_schema)
        sink_dataproductschema_df.write.format("delta").mode("overwrite").save(sink+"DataProductSchema")
        
        print("GENERATED EMPTY DATA PRODUCT SINK")
        
    #ACTION CENTER
    # create empty dataframes to for joins later.
    empty_action_df = spark.createDataFrame([], CatalogSinkSchema.sink_action_center_schema)
    
    #Generate action_df
    healthaction_df = BuildActionCenter.build_action_center_schema(existing_action_df,sink_dataproductschema_df,sink_businessdomainschema_df,sink_dataproductcontact_association_df,
                                                             sink_dataproductdomain_association_df,sink_dataassetschema_df,sink_dataassetcontact_association_df,sink_assetproduct_association_df,
                                                             empty_action_df)
    if not healthaction_df.isEmpty():
        sink_healthaction_df = spark.createDataFrame(healthaction_df.rdd, schema=CatalogSinkSchema.sink_action_center_schema)
        sink_healthaction_df.write.format("delta").mode("overwrite").save(sink+"ActionCenter")
        print("GENERATED ACTION CENTER SINK")
    else:
        sink_healthaction_df  = spark.createDataFrame([], CatalogSinkSchema.sink_action_center_schema)
        sink_healthaction_df.write.format("delta").mode("overwrite").save(sink+"ActionCenter")
        print("GENERATED EMPTY ACTION CENTER SINK")

    #HEALTH SUMMARY
    # there is not point of running transformations if it is empty.
    if sink_businessdomainschema_df.isEmpty():
        sink_health_summary_by_domain_df = spark.createDataFrame([], CatalogSinkSchema.sink_health_summary_by_domain_schema)
        sink_health_summary_by_domain_df.write.format("delta").mode("overwrite").save(sink+"DomainHealthSummary")
        health_summary_df = spark.createDataFrame([], CatalogSinkSchema.sink_health_summary_schema)
        health_summary_df.write.format("delta").mode("overwrite").save(sink+"HealthSummary")
        print("GENERATED EMPTY HEALTH SUMMARY SINK")
    else:
        #HEALTH SUMMARY BY DOMAIN
        health_summary_by_domain_df = BuildHealthSummary.build_health_summary_by_domain(sink_businessdomainschema_df, sink_healthaction_df, sink_dataproductdomain_association_df ,sink_assetproduct_association_df)
        sink_health_summary_by_domain_df = spark.createDataFrame(health_summary_by_domain_df.rdd, schema=CatalogSinkSchema.sink_health_summary_by_domain_schema)
        sink_health_summary_by_domain_df.write.format("delta").mode("overwrite").save(sink+"DomainHealthSummary")

        #AGGREGATED HEALTH SUMMARY
        health_summary_df = BuildHealthSummary.build_health_summary(sink_health_summary_by_domain_df)
        sink_health_summary_df = spark.createDataFrame(health_summary_df.rdd, schema=CatalogSinkSchema.sink_health_summary_schema)
        sink_health_summary_df.write.format("delta").mode("overwrite").save(sink+"HealthSummary")
        print("GENERATED HEALTH SUMMARY SINK")

    #SCORES
    # we check only for data product schema because both product domain and dataproduct schema has same source. so any one check is enough
    # there is not point of running transformations if any one of these is empty.
    if sink_dataproductschema_df.isEmpty():
        sink_score_by_domain_df = spark.createDataFrame([], CatalogSinkSchema.sink_scores_by_domain_schema)
        sink_score_by_domain_df.write.format("delta").mode("overwrite").save(sink+"DomainHealthScores")
        sink_scores_df = spark.createDataFrame([], CatalogSinkSchema.sink_scores_schema)
        sink_scores_df.write.format("delta").mode("overwrite").save(sink+"HealthScores")
        print("GENERATED EMPTY SCORES SINK")
    else:
        #SCORES BY DOMAIN
        score_by_domain_df = BuildScores.build_score_by_domain(sink_dataproductschema_df, sink_dataproductdomain_association_df)
        sink_score_by_domain_df = spark.createDataFrame(score_by_domain_df.rdd, schema=CatalogSinkSchema.sink_scores_by_domain_schema)
        sink_score_by_domain_df.write.format("delta").mode("overwrite").save(sink+"DomainHealthScores")

        #AGGREGATED SCORES
        scores_df = BuildScores.build_score(sink_score_by_domain_df)
        sink_scores_df = spark.createDataFrame(scores_df.rdd, schema=CatalogSinkSchema.sink_scores_schema)
        sink_scores_df.write.format("delta").mode("overwrite").save(sink+"HealthScores")
        
        print("GENERATED SCORES SINK")

    #TRENDS
    # there is not point of running transformations if any one of these is empty.
    if sink_businessdomainschema_df.isEmpty() or sink_dataproductschema_df.isEmpty() :
        sink_businessdomain_trend_by_id_df = spark.createDataFrame([], CatalogSinkSchema.sink_business_domain_trends_by_id_schema)
        sink_businessdomain_trend_by_id_df.write.format("delta").mode("overwrite").save(sink+"BusinessDomainTrendsById")
        businessdomain_trend_df = spark.createDataFrame([], CatalogSinkSchema.sink_aggregated_business_domain_trends_schema)
        businessdomain_trend_df.write.format("delta").mode("overwrite").save(sink+"BusinessDomainTrends")
        print("GENERATED EMPTY TRENDS SINK")
    else:
        #BUSINESS DOMAIN TRENDS BY ID
        businessdomain_trend_by_id_df = BuildBusinessDomainTrend.build_businessdomain_trend_by_id(sink_businessdomainschema_df, sink_dataproductschema_df, sink_dataproductdomain_association_df,
                                                                                                  sink_assetproduct_association_df,sink_healthaction_df)                                                                                            
        sink_businessdomain_trend_by_id_df = spark.createDataFrame(businessdomain_trend_by_id_df.rdd, schema=CatalogSinkSchema.sink_business_domain_trends_by_id_schema)
        sink_businessdomain_trend_by_id_df.write.format("delta").mode("append").save(sink+"BusinessDomainTrendsById")

        #BUSINESS DOMAIN TRENDS
        businessdomain_trend_df = BuildBusinessDomainTrend.build_businessdomain_trend(sink_businessdomain_trend_by_id_df)
        sink_businessdomain_trend_df = spark.createDataFrame(businessdomain_trend_df.rdd, schema=CatalogSinkSchema.sink_aggregated_business_domain_trends_schema)
        sink_businessdomain_trend_df.write.format("delta").mode("append").save(sink+"BusinessDomainTrends")
        print("GENERATED TRENDS SINK")
