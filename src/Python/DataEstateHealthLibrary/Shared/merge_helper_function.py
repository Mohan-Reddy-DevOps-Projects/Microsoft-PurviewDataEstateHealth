class MergeHelperFunction:
    def assets_reduce_func(instance1, instance2):
        """
        Returns the asset instance with the latest timestamp
        """
        if not instance1:
            return instance2
        elif not instance2:
            return instance1
        return instance2 if instance1.Timestamp < instance2.Timestamp else instance1
