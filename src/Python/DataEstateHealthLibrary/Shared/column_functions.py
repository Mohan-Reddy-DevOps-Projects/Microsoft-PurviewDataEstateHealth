from pyspark.sql.functions import *
import uuid
class ColumnFunctions:
        
    def explode_column(dataframe, col_name, new_col_name):
        column_exploded = dataframe.withColumn(
            new_col_name, explode_outer(col(col_name))
        )

        return column_exploded

    def rename_col(dataframe, old_col_name, new_col_name):
       return dataframe.withColumnRenamed(old_col_name, new_col_name)

    def add_new_col(dataframe, exisitng_col_name, new_col_name):
       return dataframe.withColumn(new_col_name, lit(col(exisitng_col_name)))
    
    def add_new_column_from_col_field(dataframe, col_name, field, new_col_name):
        new_column_from_col_field_added = dataframe.withColumn(
            new_col_name, col(col_name).getField(field)
        )

        return new_column_from_col_field_added
    
    def add_schema(dataframe, col_name, new_col_name, schema):
        added_schema = dataframe.withColumn(
            new_col_name,from_json(col(col_name), schema
            )
        )

        return added_schema
