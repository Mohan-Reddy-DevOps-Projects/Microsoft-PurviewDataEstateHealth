from lzma import FILTER_DELTA
from DataEstateHealthLibrary.Shared.column_functions import ColumnFunctions
from pyspark.sql.functions import *

class CatalogTransformationFunctions:

    def format_contact(owner_df, contactTypeField, contactTypeCol, explodeNewColName,):
        #add a new column to data frame
        contact_type_added = CatalogTransformationFunctions.add_contact_type(owner_df, contactTypeField, contactTypeCol)

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





