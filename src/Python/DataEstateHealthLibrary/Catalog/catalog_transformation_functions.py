from lzma import FILTER_DELTA
from DataEstateHealthLibrary.Shared.column_functions import ColumnFunctions
from pyspark.sql.functions import *
from pyspark.sql.types import *
import datetime

class CatalogTransformationFunctions:
    
    def add_timestamp_col(catalog_df):
        catalog_df = catalog_df.withColumn("Timestamp",catalog_df['ModifiedAt'].cast(TimestampType()))
        return catalog_df
    
    def calculate_is_active(action_df):
        is_active_added = action_df.withColumn(
            "IsActive", lit(1)
        )

        return is_active_added
    
    def format_contact(contact_df, contactTypeField, contactTypeCol, explodeNewColName,):
        #add a new column to data frame
        contact_type_added = CatalogTransformationFunctions.add_contact_type(contact_df, contactTypeField, contactTypeCol)

        #add contact role for type
        contact_role_added = CatalogTransformationFunctions.add_contact_role(contact_type_added,contactTypeCol)

        #explode column because it is an array and merge into contacts
        exploded_contacts = ColumnFunctions.explode_column(contact_role_added, contactTypeCol, explodeNewColName)
        
        #drop intermediate column as it is not needed 
        exploded_contacts = exploded_contacts.drop(col(contactTypeCol))

        #add contact id
        contact_attributes_added = CatalogTransformationFunctions.add_contact_attributes(exploded_contacts, "id", "ContactId")

        #add contact description 
        contact_attributes_added = CatalogTransformationFunctions.add_contact_attributes(contact_attributes_added, "description", "ContactDescription")

        #add is active
        contact_attributes_added = CatalogTransformationFunctions.calculate_is_active(contact_attributes_added)
        
        #filter out null columns 
        contact_formatted = contact_attributes_added.filter(col("Contacts").isNotNull())
        return contact_formatted
    
    def add_contact_role(catalog_contact_df, colName):
        contact_role_added = catalog_contact_df.withColumn(
            "ContactRole", when(col(colName).isNotNull(),
                                colName))
        return contact_role_added

    def add_contact_attributes(catalog_df, field, newColName):
        contact_attributes_added = catalog_df.withColumn(
            newColName, catalog_df.Contacts.getField(field)
        )

        return contact_attributes_added

    def add_contact_type(catalog_df, field, newColName):
        contact_type_added = catalog_df.withColumn(
            newColName, when(col("Contacts").isNotNull() &
                           col("Contacts").getField(field).isNotNull(),
                           col("Contacts").getField(field))
        )

        return contact_type_added





