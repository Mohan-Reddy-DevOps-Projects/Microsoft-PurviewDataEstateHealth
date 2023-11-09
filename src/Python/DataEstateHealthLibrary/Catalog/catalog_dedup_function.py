class CatalogDedupFunction():

    def business_domain_id_dedup_func(instance1, instance2):
        """
        Returns the instance with the business domain id
        """
        if not instance1:
            return instance2
        elif not instance2:
            return instance1
        return instance2 if instance1.BusinessDomainId == instance2.BusinessDomainId else instance1

    def data_product_id_dedup_func(instance1, instance2):
        """
        Returns the instance with the same data product id
        """
        if not instance1:
            return instance2
        elif not instance2:
            return instance1
        return instance2 if instance1.DataProductId == instance2.DataProductId else instance1

    def term_id_dedup_func(instance1, instance2):
        """
        Returns the instance with the same term id
        """
        if not instance1:
            return instance2
        elif not instance2:
            return instance1
        return instance2 if instance1.TermId == instance2.TermId else instance1
    
    def asset_id_dedup_func(instance1, instance2):
        if not instance1:
            return instance2
        elif not instance2:
            return instance1
        return instance2 if instance1.DataAssetId == instance2.DataAssetId else instance1
