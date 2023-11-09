from pyspark.sql.functions import *
from pyspark.sql import SparkSession
import sys
from DataEstateHealthLibrary.Catalog.Terms.build_term import BuildTerm
from DataEstateHealthLibrary.Catalog.DataProduct.build_data_product import BuildDataProduct
from DataEstateHealthLibrary.Catalog.DataAsset.build_data_asset import BuildDataAsset
from DataEstateHealthLibrary.Catalog.BusinessDomain.build_business_domain import BuildBusinessDomain
from DataEstateHealthLibrary.Catalog.Relationship.build_relationship import BuildRelationship
from DataEstateHealthLibrary.ActionCenter.build_action_center import BuildActionCenter
from DataEstateHealthLibrary.HealthSummary.build_health_summary import BuildHealthSummary
from DataEstateHealthLibrary.CdmcScore.build_scores import BuildScores
from DataEstateHealthLibrary.BusinessDomainTrend.build_businessdomain_trend import BuildBusinessDomainTrend

if __name__ == "__main__":
    
    #create spark session
    spark = SparkSession \
    .builder \
    .appName("python spark test") \
    .config("spark.some.config.option", "some-value") \
    .getOrCreate()

    print(sys.argv[1])
    print(sys.argv[2])
    print(sys.argv[3])
    
    container = sys.argv[1]
    source = sys.argv[2]
    sink = sys.argv[3]
    
    token_library = spark._jvm.com.microsoft.azure.synapse.tokenlibrary.TokenLibrary
    blob_sas_token = token_library.getConnectionString("dgprocessingwus2esraapo")
    spark.conf.set(
    'fs.azure.sas.%s.%s.dfs.core.windows.net' % (container, "dgprocessingwus2esraapo"),
    blob_sas_token)

    #TERM SOURCE
    source_term_df = spark.read.load(source+'Term', format='parquet')
    
    #Generate termdomain_association_df
    termdomain_association_df = BuildTerm.build_term_business_domain_association_schema(source_term_df)
    termdomain_association_df.write.format("delta").mode("overwrite").save(sink+"TermDomainAssociation")
    
    #Generate termcontact_association_df
    termcontact_association_df = BuildTerm.build_term_contact_association(source_term_df)
    termcontact_association_df.write.format("delta").mode("overwrite").save(sink+"TermContactAssociation")
    
    #Generate termschema_df
    termschema_df = BuildTerm.build_term_schema(source_term_df)
    termschema_df.write.format("delta").mode("overwrite").save(sink+"TermSchema")


    #DATA PRODUCT SOURCE
    source_dataproduct_df = spark.read.load(source+'DataProduct', format='parquet')
    
    #Generate termdomain_association_df
    dataproductdomain_association_df = BuildDataProduct.build_data_product_business_domain_association(source_dataproduct_df)
    dataproductdomain_association_df.write.format("delta").mode("overwrite").save(sink+"DataProductDomainAssociation")
    
    #Generate dataproductschema_df
    dataproductschema_df = BuildDataProduct.build_data_product_schema(source_dataproduct_df)
    dataproductschema_df.write.format("delta").mode("overwrite").save(sink+"DataProductSchema")

    #Generate dataproductcontact_association_df
    dataproductcontact_association_df = BuildDataProduct.build_data_product_contact_association(source_dataproduct_df)
    dataproductcontact_association_df.write.format("delta").mode("overwrite").save(sink+"DataProductContactAssociation")

    #DATA ASSET SOURCE
    source_dataasset_df = spark.read.load(source+'DataAsset', format='parquet')

    #Generate dataasset_domain_association_df
    dataasset_domain_association_df = BuildDataAsset.build_asset_domain_association(source_dataasset_df)
    dataasset_domain_association_df.write.format("delta").mode("overwrite").save(sink+"AssetDomainAssociation")

    #Generate dataassetschema_df
    dataassetschema_df = BuildDataAsset.build_data_asset_schema(source_dataasset_df)
    dataassetschema_df.write.format("delta").mode("overwrite").save(sink+"DataAssetSchema")

    #Generate dataassetcontact_association_df
    dataassetcontact_association_df = BuildDataAsset.build_data_asset_contact_association(source_dataasset_df)
    dataassetcontact_association_df.write.format("delta").mode("overwrite").save(sink+"DataAssetContactAssociation")

    #BUSINESS DOMAIN SOURCE
    source_businessdomain_df = spark.read.load(source+'BusinessDomain', format='parquet')
    
    #Generate businessdomainschema_df
    sink_term_domain_association_df = spark.read.load(sink+'TermDomainAssociation', format='parquet')
    sink_dataproduct_domain_association = spark.read.load(sink+'DataProductDomainAssociation', format='parquet')

    businessdomainschema_df = BuildBusinessDomain.build_business_domain_schema(source_businessdomain_df, sink_dataproduct_domain_association, sink_term_domain_association_df)
    businessdomainschema_df.write.format("delta").mode("overwrite").option("mergeSchema", "true").save(sink+"BusinessDomainSchema")

    #RELATIONSHIP SOURCE
    sink_data_asset_df = spark.read.load(sink+'DataAssetSchema', format='parquet')
    source_relationship_df = spark.read.load(source+'Relationship', format='parquet')

    #Generate relationship_association df
    product_asset_association_df = BuildRelationship.build_product_asset_domain_association(source_relationship_df,sink_data_asset_df)
    product_asset_association_df.write.format("delta").mode("overwrite").option("mergeSchema", "true").save(sink+"AssetProductAssociation")
    
    #ACTION CENTER
    sink_dataproduct_df = spark.read.load(sink+'DataProductSchema', format='parquet')
    sink_businessdomain_df = spark.read.load(sink+'BusinessDomainSchema', format='parquet')
    sink_dataproduct_contact_association = spark.read.load(sink+'DataProductContactAssociation', format='parquet')
    #sink_dataproduct_domain_association = spark.read.load('abfss://ecf09339-34e0-464b-a8fb-661209048543@dgprocessingwus2esraapo.dfs.core.windows.net/Sink/DataProductDomainAssociation', format='parquet')
    sink_dataasset_contact_association = spark.read.load(sink+'DataAssetContactAssociation', format='parquet')
    
    #Generate action_df
    action_df = BuildActionCenter.build_action_center_schema(sink_dataproduct_df,sink_businessdomain_df,sink_dataproduct_contact_association,sink_dataproduct_domain_association,sink_data_asset_df,sink_dataasset_contact_association)
    action_df.write.format("delta").mode("overwrite").save(sink+"ActionCenter")

    #HEALTH SUMMARY
    sink_assetdomain_association_df = spark.read.load(sink+'AssetDomainAssociation', format='parquet')
    sink_healthaction_df = spark.read.load(sink+'ActionCenter', format='parquet')
    
    #Generate health_summary_df
    health_summary_df = BuildHealthSummary.build_health_summary(sink_businessdomain_df, sink_healthaction_df, sink_assetdomain_association_df)
    health_summary_df.write.format("delta").mode("overwrite").save(sink+"HealthSummary")

    #SCORES
    scores_df = BuildScores.build_score(sink_dataproduct_df, sink_dataproduct_domain_association)
    scores_df.write.format("delta").mode("overwrite").save(sink+"HealthScores")

    #BUSINESS DOMAIN TRENDS
    businessdomain_trend_df = BuildBusinessDomainTrend.build_businessdomain_trend_schema(sink_businessdomain_df, sink_dataproduct_df, sink_dataproduct_domain_association, sink_assetdomain_association_df)
    businessdomain_trend_df.write.format("delta").mode("overwrite").save(sink+"BusinessDomainTrends")
