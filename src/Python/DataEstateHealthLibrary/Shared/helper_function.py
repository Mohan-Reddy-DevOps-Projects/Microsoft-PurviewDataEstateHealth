from pyspark.sql.functions import *
from pyspark.sql import *
import datetime

class HelperFunction:
    def calculate_last_refreshed_at(dataframe):
        now = datetime.datetime.now()
        date_string = now.strftime("%Y%m%d")
        date_int = int(date_string)
        last_refreshed_at_added = dataframe.withColumn(
            "LastRefreshedAt", lit(date_int)
        )

        return last_refreshed_at_added
    
    def calculate_default_business_domain_dispaly_name(dataframe,value):
        default_business_domain_dispaly_name_added = dataframe.withColumn(
        "BusinessDomainDisplayName", lit(value)
        )

        return default_business_domain_dispaly_name_added
    
    def calculate_default_business_domain_id(dataframe,value):
        default_business_domain_id_added = dataframe.withColumn(
            "BusinessDomainId", lit(value)
        )

        return default_business_domain_id_added
    
    def cast_to_specific_type(dataframe,type,colName):
        dataframe = dataframe.withColumn(colName, dataframe[colName].cast(type))
        return dataframe
