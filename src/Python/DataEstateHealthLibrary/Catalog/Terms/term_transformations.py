from pyspark.sql.functions import *
from DataEstateHealthLibrary.Shared.column_functions import ColumnFunctions

class TermTransformations:
    def calculate_has_description(term_df):
        has_description_added = term_df.withColumn(
            "HasDescription", when(col("Description").isNotNull(), 1)
            .otherwise(0)
        )

        return has_description_added
    
    def calculate_term_count_for_domain(term_domain_association_df):
        terms_count_added = term_domain_association_df.groupBy("BusinessDomainId").count()
        terms_count_added = ColumnFunctions.rename_col(term_domain_association_df, "count", "GlossaryTermsCount")
        return terms_count_added
