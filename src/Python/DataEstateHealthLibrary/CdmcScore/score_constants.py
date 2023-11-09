from unittest.mock import DEFAULT

from DataEstateHealthLibrary.Catalog import BusinessDomain


class ScoreConstants:
    Description = "Review your governance maturity posture based on the Cloud Data Management Council's framework."
    Scorekind = ["DataGovernance", "DataQuality", "DataCuration"]
    ScoreName = "Governance score"
    DefaultBusinessDomainDisplayName = "All business domains"
    DefaultBusinessDomainId = "00000000-0000-0000-0000-000000000000"
