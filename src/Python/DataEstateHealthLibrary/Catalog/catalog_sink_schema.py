from sqlite3 import Timestamp
from pyspark.sql.functions import *
from pyspark.sql.types import *

class CatalogSinkSchema:
    
    sink_data_product_schema = StructType(
        [
            StructField("DataProductId", StringType(), False),
            StructField("DataProductDisplayName", StringType(), False),
            StructField("DataProductType", StringType(), True),
            StructField("DataProductStatus", StringType(), False),
            StructField("HasValidOwner", BooleanType(), False),
            StructField("HasValidUseCase", BooleanType(), False),
            StructField("HasValidTermsOfUse", BooleanType(), False),
            StructField("AssetCount", LongType(), False),
            StructField("HasDescription", BooleanType(), False),
            StructField("IsAuthoritativeSource", BooleanType(), False),
            StructField("HasDataQualityScore", BooleanType(), False),
            StructField("ClassificationPassCount", IntegerType(), True),
            StructField("HasAccessEntitlement", BooleanType(), False),
            StructField("HasDataShareAgreementSetOrExempt", BooleanType(), False),
            StructField("GlossaryTermCount", IntegerType(), True),
            StructField("CreatedAt", TimestampType(), False),
            StructField("CreatedBy", StringType(), True),
            StructField("ModifiedAt", TimestampType(), False),
            StructField("ModifiedBy", StringType(), True),
            StructField("LastRefreshedAt", TimestampType(), False),
        ]
    )

    sink_data_product_contact_association_schema = StructType(
        [
            StructField("DataProductId", StringType(), False),
            StructField("ContactRole", StringType(), False),
            StructField("ContactId", StringType(), False),
            StructField("ContactDescription", StringType(), True)
        ]
    )

    sink_data_product_domain_association_schema = StructType(
        [
            StructField("DataProductId", StringType(), False),
            StructField("BusinessDomainId", StringType(), False),
            StructField("IsPrimaryDataProduct", BooleanType(), False)
        ]
    )
    
    sink_business_domain_schema = StructType(
        [
            StructField("BusinessDomainId", StringType(), False),
            StructField("BusinessDomainDisplayName", StringType(), False),
            StructField("HasDescription", BooleanType(), False),
            StructField("GlossaryTermsCount", IntegerType(), True),
            StructField("DataProductsCount", IntegerType(), True),
            StructField("IsRootDomain", BooleanType(), False),
            StructField("HasValidOwner", BooleanType(), False),
            StructField("CreatedAt", TimestampType(), False),
            StructField("CreatedBy", StringType(), True),
            StructField("ModifiedAt", TimestampType(), False),
            StructField("ModifiedBy", StringType(), True),
            StructField("LastRefreshedAt", TimestampType(), False),
        ]
    )
    
    sink_data_asset_schema = StructType(
        [
            StructField("DataAssetId", StringType(), False),
            StructField("BusinessDomainId", StringType(), False),
            StructField("SourceAssetId", StringType(), False),
            StructField("DisplayName", StringType(), False),
            StructField("ObjectType", StringType(), False),
            StructField("SourceType", StringType(), False),
            StructField("HasClassification", BooleanType(), False),
            StructField("HasSchema", BooleanType(), False),
            StructField("HasDescription", BooleanType(), False),
            StructField("CreatedAt", TimestampType(), False),
            StructField("CreatedBy", StringType(), True),
            StructField("ModifiedAt", TimestampType(), False),
            StructField("ModifiedBy", StringType(), True),
            StructField("LastRefreshedAt", TimestampType(), False)
        ]
    )
    
    sink_data_asset_contact_association_schema = StructType(
        [
            StructField("DataAssetId", StringType(), False),
            StructField("ContactRole", StringType(), False),
            StructField("ContactId", StringType(), False),
            StructField("ContactDescription", StringType(), True)
        ]
    )

    sink_data_asset_domain_association_schema = StructType(
        [
            StructField("DataAssetId", StringType(), False),
            StructField("BusinessDomainId", StringType(), False)
        ]
    )
    
    sink_product_asset_association_schema = StructType(
        [
            StructField("DataAssetId", StringType(), False),
            StructField("DataProductId", StringType(), False)
        ]
    )
    
    sink_term_schema = StructType(
        [
            StructField("TermId", StringType(), False),
            StructField("Name", StringType(), False),
            StructField("Status", StringType(), False),
            StructField("HasDescription", BooleanType(), False),
            StructField("CreatedAt", TimestampType(), False),
            StructField("CreatedBy", StringType(), True),
            StructField("ModifiedAt", TimestampType(), False),
            StructField("ModifiedBy", StringType(), True),
            StructField("LastRefreshedAt", TimestampType(), False)
        ]
    )
    
    sink_term_contact_association_schema = StructType(
        [
            StructField("TermId", StringType(), False),
            StructField("ContactRole", StringType(), False),
            StructField("ContactId", StringType(), False),
            StructField("ContactDescription", StringType(), True)
        ]
    )
    
    sink_term_domain_association_schema = StructType(
        [
            StructField("TermId", StringType(), False),
            StructField("BusinessDomainId", StringType(), False)
        ]
    )

    sink_action_center_schema = StructType(
        [
            StructField("RowId", StringType(), False),
            StructField("ActionId", StringType(), False),
            StructField("DisplayName", StringType(), False),
            StructField("Description", StringType(), False),
            StructField("TargetType", StringType(), False),
            StructField("TargetId", StringType(), False),
            StructField("TargetName", StringType(), False),
            StructField("OwnerContactId", StringType(), True),
            StructField("OwnerContactDisplayName", StringType(), True),
            StructField("HealthControlState", StringType(), False),
            StructField("HealthControlName", StringType(), False),
            StructField("HealthControlCategory", StringType(), False),
            StructField("ActionStatus", StringType(), False),
            StructField("BusinessDomainId", StringType(), False),
            StructField("DataProductId", StringType(), True),
            StructField("DataAssetId", StringType(), True),
            StructField("CreatedAt", TimestampType(), False),
            StructField("LastRefreshedAt", TimestampType(), False)
        ]
    )
    
    sink_health_summary_by_domain_schema= StructType(
        [
            StructField('BusinessDomainId', StringType(), False),
            StructField('BusinessDomainDisplayName', StringType(), False),
            StructField('TotalBusinessDomains', IntegerType(), False),
            StructField('TotalDataProductsCount', IntegerType(), False),
            StructField('TotalCuratedDataAssetsCount', LongType(), False),
            StructField('TotalOpenActionsCount', IntegerType(), False),
            StructField('BusinessDomainsFilterListLink', StringType(), False),
            StructField('BusinessDomainsTrendLink', StringType(), False),
            StructField('DataProductsTrendLink', StringType(), False),
            StructField('DataAssetsTrendLink', StringType(), False),
            StructField('HealthActionsTrendLink', StringType(), False),
            StructField('TotalReportsCount', IntegerType(), False),
            StructField('ActiveReportsCount', IntegerType(), False),
            StructField('DraftReportsCount', IntegerType(), False),
            StructField('TotalCuratableDataAssetsCount', LongType(), False),
            StructField('TotalNonCuratableDataAssetsCount', LongType(), False),
            StructField('TotalCompletedActionsCount', IntegerType(), False),
            StructField('TotalDismissedActionsCount', IntegerType(), False),
            StructField('LastRefreshDate', TimestampType(), False)
            ]
        )
    
    sink_health_summary_schema= StructType(
        [
            StructField('RowId', StringType(), False),
            StructField('TotalBusinessDomains', IntegerType(), False),
            StructField('TotalDataProductsCount', IntegerType(), False),
            StructField('TotalCuratedDataAssetsCount', LongType(), False),
            StructField('TotalOpenActionsCount', IntegerType(), False),
            StructField('BusinessDomainsFilterListLink', StringType(), False),
            StructField('BusinessDomainsTrendLink', StringType(), False),
            StructField('DataProductsTrendLink', StringType(), False),
            StructField('DataAssetsTrendLink', StringType(), False),
            StructField('HealthActionsTrendLink', StringType(), False),
            StructField('TotalReportsCount', IntegerType(), False),
            StructField('ActiveReportsCount', IntegerType(), False),
            StructField('DraftReportsCount', IntegerType(), False),
            StructField('TotalCuratableDataAssetsCount', LongType(), False),
            StructField('TotalNonCuratableDataAssetsCount', LongType(), False),
            StructField('TotalCompletedActionsCount', IntegerType(), False),
            StructField('TotalDismissedActionsCount', IntegerType(), False),
            StructField('LastRefreshDate', TimestampType(), False)
            ]
        )

    sink_scores_by_domain_schema = StructType(
        [
            StructField('BusinessDomainId', StringType(), False),
            StructField('Description', StringType(), False),
            StructField('Name', StringType(), False),
            StructField('ScoreKind', StringType(), False),
            StructField('ActualValue', DoubleType(), False),
            StructField('TotalDataProducts', IntegerType(), False),
            StructField('C2_Ownership', DoubleType(), False),
            StructField('C3_AuthoritativeSource', DoubleType(), False),
            StructField('C5_Catalog', DoubleType(), False),
            StructField('C6_Classification', DoubleType(), False),
            StructField('C7_Access', DoubleType(), False),
            StructField('C8_DataConsumptionPurpose', DoubleType(), False),
            StructField('C12_Quality', DoubleType(), False),
            StructField('DataHealth', DoubleType(), False),
            StructField('MetadataCompleteness', DoubleType(), False),
            StructField('Use', DoubleType(), False),
            StructField('Quality', DoubleType(), False),
            StructField('LastRefreshedAt', TimestampType(), False)
            ]
        )
    
    sink_scores_schema = StructType(
        [
            StructField('RowId', StringType(), False),
            StructField('Description', StringType(), False),
            StructField('Name', StringType(), False),
            StructField('ScoreKind', StringType(), False),
            StructField('ActualValue', DoubleType(), False),
            StructField('TotalDataProducts', IntegerType(), False),
            StructField('C2_Ownership', DoubleType(), False),
            StructField('C3_AuthoritativeSource', DoubleType(), False),
            StructField('C5_Catalog', DoubleType(), False),
            StructField('C6_Classification', DoubleType(), False),
            StructField('C7_Access', DoubleType(), False),
            StructField('C8_DataConsumptionPurpose', DoubleType(), False),
            StructField('C12_Quality', DoubleType(), False),
            StructField('DataHealth', DoubleType(), False),
            StructField('MetadataCompleteness', DoubleType(), False),
            StructField('Use', DoubleType(), False),
            StructField('Quality', DoubleType(), False),
            StructField('TotalBusinessDomains', IntegerType(), False),
            StructField('LastRefreshedAt', TimestampType(), False)
            ]
        )

    sink_business_domain_trends_by_id_schema = StructType(
        [
            StructField('BusinessDomainTrendId', StringType(), False),
            StructField('BusinessDomainId', StringType(), False),
            StructField('DataProductCount', IntegerType(), False),
            StructField('AssetCount', LongType(), False),
            StructField('TotalOpenActionsCount', IntegerType(), False),
            StructField('ClassificationPassCount', IntegerType(), False),
            StructField('GlossaryTermPassCount', IntegerType(), False),
            StructField('DataQualityScorePassCount', IntegerType(), False),
            StructField('DataProductDescriptionPassCount', IntegerType(), False),
            StructField('ValidDataProductOwnerPassCount', IntegerType(), False),
            StructField('DataShareAgreementSetOrExemptPassCount', IntegerType(), False),
            StructField('AuthoritativeSourcePassCount', IntegerType(), False),
            StructField('ValidUseCasePassCount', IntegerType(), False),
            StructField('ValidTermsOfUsePassCount', IntegerType(), False),
            StructField('AccessEntitlementPassCount', IntegerType(), False),
            StructField('LastRefreshedAt', TimestampType(), False)
            ]
        )

    sink_aggregated_business_domain_trends_schema = StructType(
        [
            StructField('RowId', StringType(), False),
            StructField('DataProductCount', IntegerType(), False),
            StructField('AssetCount', LongType(), False),
            StructField('BusinessDomainCount', IntegerType(), False),
            StructField('TotalOpenActionsCount', IntegerType(), False),
            StructField('ClassificationPassCount', IntegerType(), False),
            StructField('GlossaryTermPassCount', IntegerType(), False),
            StructField('DataQualityScorePassCount', IntegerType(), False),
            StructField('DataProductDescriptionPassCount', IntegerType(), False),
            StructField('ValidDataProductOwnerPassCount', IntegerType(), False),
            StructField('DataShareAgreementSetOrExemptPassCount', IntegerType(), False),
            StructField('AuthoritativeSourcePassCount', IntegerType(), False),
            StructField('ValidUseCasePassCount', IntegerType(), False),
            StructField('ValidTermsOfUsePassCount', IntegerType(), False),
            StructField('AccessEntitlementPassCount', IntegerType(), False),
            StructField('LastRefreshedAt', TimestampType(), False)
            ]
        )
