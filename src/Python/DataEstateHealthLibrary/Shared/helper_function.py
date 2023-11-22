from pickle import FALSE
from pyspark.sql.functions import *
from pyspark.sql import *
import datetime
import pyspark.sql.functions as f
from pyspark.sql.types import *

class HelperFunction:
    
    def calculate_last_refreshed_at(dataframe, colName):
        last_refreshed_at_added = dataframe.withColumn(
            colName, lit(current_timestamp())
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
    
    def cast_string_to_timestamp(dataframe,col):
        dataframe = dataframe.withColumn(col,to_timestamp(col))
        return dataframe
        
    def add_timestamp_col(dataframe, colName):
        dataframe = dataframe.withColumn("Timestamp",dataframe[colName].cast(TimestampType()))
        return dataframe
    
    def update_col_to_int(dataframe, colName):
        boolean_updated = dataframe.withColumn(
        colName, when(col(colName) == True,1).otherwise(0)
        )
        return boolean_updated
    
    def calculate_default_column_value(dataframe, colName, value):
        default_column_value_added =  dataframe.withColumn(
           colName, lit(value)
           )
        return default_column_value_added
    
    def calculate_uuid_column(dataframe, colName):
        uuid_added =  dataframe.withColumn(
           colName, f.expr("uuid()")
           )
        return uuid_added

    #it helps in updating nulls to meaningful values to avoid sql issues later.
    def update_null_values(dataframe, colName, value):
        updated_df = dataframe.withColumn(
            colName, when(col(colName).isNull(), lit(value))
                                          .otherwise(col(colName))
                                          )
        return updated_df
