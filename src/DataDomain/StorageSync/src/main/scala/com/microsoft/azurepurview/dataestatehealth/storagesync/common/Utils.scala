package com.microsoft.azurepurview.dataestatehealth.storagesync.common

import com.microsoft.azurepurview.dataestatehealth.commonutils.logger.SparkLogging

object Utils extends SparkLogging {

  /**
   * Converts a given HTTPS URL into a specific ABFS (Azure Blob File System) format.
   *
   * This function validates the input URL, ensures it follows the expected format,
   * and then extracts the domain, container, and path components to construct
   * a new ABFS URL.
   *
   * @param url The HTTPS URL to be converted. It must start with "https://".
   *            Example: "https://onelake.dfs.fabric.microsoft.com/abc/ddd/Files/DEHDemo".
   * @return A string representing the converted ABFS URL in the format
   *         "abfss://<container>@<domain>/<path>".
   * @throws IllegalArgumentException If the provided URL does not start with "https://"
   *                                  or if it is missing the required components.
   *
   * Usage example:
   * {{{
   * val abfsUrl = convertUrl("https://onelake.dfs.fabric.microsoft.com/abc/ddd/Files/DEHDemo")
   * // returns "abfss://abc@onelake.dfs.fabric.microsoft.com/ddd/Files/DEHDemo"
   * }}}
   */
  def convertUrl(url: String): (String, String) = {
    logger.info(s"Starting URL conversion for: $url")

    // Validate the input URL
    if (!url.startsWith("https://")) {
      logger.error("Invalid URL: URL must start with 'https://'")
      throw new IllegalArgumentException("URL must start with 'https://'")
    }

    // Remove the 'https://' prefix
    val strippedUrl = url.substring("https://".length)

    // Split the URL into components
    val parts = strippedUrl.split("/", 3)

    // Print the parts for debugging
    logger.info(s"Split URL parts: ${parts.mkString(", ")}")

    // Ensure we have at least three parts: protocol, tenant, and path
    if (parts.length < 2) {
      logger.error("Invalid URL format: Missing container or domain")
      throw new IllegalArgumentException("URL format is incorrect")
    }

    // Extract the parts
    val domain = parts(0) // e.g., 'onelake.dfs.fabric.microsoft.com'
    val container = parts(1) // e.g., 'abc'
    val path = if (parts.length > 2) parts(2) else "" // e.g., 'ddd/Files/DEHDemo'

    logger.info(s"Extracted domain: $domain, tenant: $container, path: $path")

    // Construct the new URL
    val convertedUrl = s"abfss://$container@$domain/$path".replace(
      ".blob.core.windows.net",".dfs.core.windows.net")
    logger.info(s"Successfully converted URL: $convertedUrl")
    (domain, convertedUrl)
  }
}
