from pyspark.sql.functions import *
from pyspark.sql import *
from DataEstateHealthLibrary.AtladRdd.atlas_rdd_map import AtlasRddMap
from itertools import chain

class HelperFunction:
    def create_source_platform_provider_mapping():
        try:
            provider_mapping_json = {}
            platform_mapping_json = {}
            for provider, provider_val in AtlasRddMap.PlatformProviderSourceMapping.items():
                for platform, platform_val in provider_val.items():
                    provider_mapping_json.update({source: provider for source in platform_val})
                    platform_mapping_json.update({source: platform for source in platform_val})
            provider_mapping = create_map([lit(x) for x in chain(*provider_mapping_json.items())])
            platform_mapping = create_map([lit(x) for x in chain(*platform_mapping_json.items())])

            return provider_mapping, platform_mapping
        except Exception as e:
            raise Exception("Fail to load filename type list {0}".format(str(e)))
