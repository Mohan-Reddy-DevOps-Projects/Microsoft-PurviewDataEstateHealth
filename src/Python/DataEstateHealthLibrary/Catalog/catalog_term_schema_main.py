from pyspark.sql.functions import *
from pyspark.sql import SparkSession
import sys
from DataEstateHealthLibrary.Catalog.Terms.build_term import BuildTerm
from DataEstateHealthLibrary.Catalog.DataProduct.build_data_product import BuildDataProduct
from DataEstateHealthLibrary.Catalog.DataAsset.build_data_asset import BuildDataAsset
from DataEstateHealthLibrary.Catalog.BusinessDomain.build_business_domain import BuildBusinessDomain
from DataEstateHealthLibrary.ActionCenter.build_action_center import BuildActionCenter
from DataEstateHealthLibrary.HealthSummary.build_health_summary import BuildHealthSummary
from DataEstateHealthLibrary.CdmcScore.build_scores import BuildScores

if __name__ == "__main__":
    #create spark session
    spark = SparkSession \
    .builder \
    .appName("python spark test") \
    .config("spark.some.config.option", "some-value") \
    .getOrCreate()
    
    
    print(sys.argv[1])
    print(sys.argv[2])
    
    token_library = spark._jvm.com.microsoft.azure.synapse.tokenlibrary.TokenLibrary
    blob_sas_token = token_library.getConnectionString("dgprocessingwus2esraapo")
    spark.conf.set(
    'fs.azure.sas.%s.%s.dfs.core.windows.net' % (sys.argv[1], "dgprocessingwus2esraapo"),
    blob_sas_token)

    #TERM SOURCE
    path = sys.argv[2]+"Term"
    print(path)
    source_term_df = spark.read.load(sys.argv[2]+"Term", format='parquet')
    
    #Generate termdomain_association_df
    termdomain_association_df = BuildTerm.build_term_business_domain_association_schema(source_term_df)
    termdomain_association_df.write.format("delta").mode("overwrite").save(sys.argv[2]+"TermDomainAssociation")
    
    #Generate termcontact_association_df
    termcontact_association_df = BuildTerm.build_term_contact_association(source_term_df)
    termcontact_association_df.write.format("delta").mode("overwrite").save(sys.argv[3]+"TermContactAssociation")
    
    #Generate termschema_df
    termschema_df = BuildTerm.build_term_schema(source_term_df)
    #termschema_df.write.format("delta").mode("overwrite").save(sys.argv[3]+"TermSchema")
