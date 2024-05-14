package com.microsoft.azurepurview.dataestatehealth.dimensionalmodel.factdgcontrols

class DGControlsConfigIni {
  private val dGControlsConfigIni = List(
    //DGControlsConfig("Data Inventory", "Data Estate Curation",0)
    DGControlsConfig("Classification and Labeling", "Estate Curation", 1)
    ,DGControlsConfig("Data Product Ownership", "Trusted Data", 1)
    ,DGControlsConfig("Data estate health monitoring, alerting, and insights", "Estate Observability", 1)
    ,DGControlsConfig("Data Products Connection", "Discoverability And Understanding", 1)
    ,DGControlsConfig("Data Quality Measurement", "Trusted Data", 1)
    //,DGControlsConfig("Data Quality Score","Trusted Data", 0)
    ,DGControlsConfig("Business OKRs Alignment", "Value Creation", 0)
    ,DGControlsConfig("Critical Data Identification", "Estate Curation", 0)
    ,DGControlsConfig("Critical Data Connection", "Estate Curation", 0)
    //,DGControlsConfig("Discoverability and Understanding Score", "Data Discoverability and Understanding", 0)
    ,DGControlsConfig("Self-serve Access Enablement", "Access And Use", 1)
    ,DGControlsConfig("Compliant Data Use", "Access And Use", 1)
    ,DGControlsConfig("Data Cataloging", "Discoverability And Understanding", 1)
    ,DGControlsConfig("Data Security and Compliance", "Trusted Data", 0)
    //,DGControlsConfig("Discoverability and Understanding Measurement", "Data Discoverability and Understanding (MDQ)", 1)
    //,DGControlsConfig("Discoverability and Understanding Score", "Data Discoverability and Understanding (MDQ)", 0)
    ,DGControlsConfig("Data Product Certification", "Trusted Data", 1)
    ,DGControlsConfig("Access Request SLA", "Access And Use", 0)
    ,DGControlsConfig("Zero-Trust Default Access", "Access And Use", 0)
    ,DGControlsConfig("Operational Costs", "Value Creation", 0)
    ,DGControlsConfig("Business Value", "Value Creation", 0)
    ,DGControlsConfig("Linked Assets", "Metadata Quality Management", 1)
    ,DGControlsConfig("Ownership", "Metadata Quality Management", 1)
    ,DGControlsConfig("Business Alignment", "Estate Curation", 0)
    ,DGControlsConfig("Usability", "Metadata Quality Management", 1)
    ,DGControlsConfig("Accuracy", "Data Quality Management", 0)
    ,DGControlsConfig("Completeness", "Data Quality Management", 0)
    ,DGControlsConfig("Consistency", "Data Quality Management", 0)
    ,DGControlsConfig("Timeliness", "Data Quality Management", 0)
    ,DGControlsConfig("Uniqueness", "Data Quality Management", 0)
  )

  def getProcessFlag(healthControlDisplayName: String, healthControlGroupDisplayName: String): Option[Int] = {
    dGControlsConfigIni.find(config =>
      config.HealthControlDisplayName == healthControlDisplayName &&
        config.HealthControlGroupDisplayName == healthControlGroupDisplayName
    ).map(_.ProcessFlag)
  }
}