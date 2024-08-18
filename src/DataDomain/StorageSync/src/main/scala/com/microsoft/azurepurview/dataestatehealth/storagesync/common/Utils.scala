package com.microsoft.azurepurview.dataestatehealth.storagesync.common

object Utils {
  /**
   * Resolves the storage endpoint based on the given storage type.
   *
   * @param storageType The type of the storage (e.g., "onelake", "adlsgen2").
   * @return The corresponding storage endpoint.
   * @throws IllegalArgumentException if the storage type is unknown.
   */
  def getStorageEndpoint(storageType: String): String = {
    storageType match {
      case "Fabric" => "onelake.dfs.fabric.microsoft.com"
      case "adlsgen2" => "dfs.core.windows.net"
      case _ => throw new IllegalArgumentException(s"Unknown storage type: $storageType")
    }
  }

  def convertUrl(url: String): String = {
    // Validate the input URL
    if (!url.startsWith("https://")) {
      throw new IllegalArgumentException("URL must start with 'https://'")
    }

    // Remove the 'https://' prefix
    val strippedUrl = url.substring("https://".length)

    // Split the URL into components
    val parts = strippedUrl.split("/", 3)

    // Ensure we have at least three parts: protocol, tenant, and path
    if (parts.length < 3) {
      throw new IllegalArgumentException("URL format is incorrect")
    }

    // Extract the parts
    val domain = parts(0) // e.g., 'onelake.dfs.fabric.microsoft.com'
    val tenant = parts(1) // e.g., 'abc'
    val path = parts(2)   // e.g., 'ddd/Files/DEHDemo'

    // Construct the new URL
    s"abfss://$tenant@$domain/$path"
  }

}

