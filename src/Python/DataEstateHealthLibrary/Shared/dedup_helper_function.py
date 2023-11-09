class DedupHelperFunction:
    def dedup_by_timestamp(instance1, instance2):
        """
        Returns the instance with the latest timestamp
        """
        if not instance1:
            return instance2
        elif not instance2:
            return instance1
        return instance2 if instance1.Timestamp < instance2.Timestamp else instance1
