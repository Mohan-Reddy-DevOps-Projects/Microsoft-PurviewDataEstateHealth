package com.microsoft.azurepurview.dataestatehealth.storagesync.auth

import org.apache.hadoop.conf.Configuration
import org.apache.hadoop.fs.azurebfs.extensions.CustomTokenProviderAdaptee

import com.microsoft.azurepurview.dataestatehealth.commonutils.logger.SparkLogging
import java.util.Date

/**
 * MITokenProvider is a concrete implementation of the `CustomTokenProviderAdaptee` class.
 * It provides methods for managing and utilizing an access token, including
 * initialization, retrieval, and expiration checking.
 *
 * @param configuration Configuration object containing necessary setup details.
 * @param accountName Name of the account for which the token is to be managed.
 */
class MITokenProvider extends CustomTokenProviderAdaptee with SparkLogging {

  /** Access token for authentication. */
  private var accessToken: String = _

  /** The expiry time of the access token. */
  private var expiryTime: Date = _

  /**
   * Initializes the token provider by obtaining an access token and its expiry time.
   * This method should be called to set up the provider with the current token
   * details before it can be used.
   *
   * @param configuration Configuration object used for initialization.
   * @param accountName The name of the account to be associated with the token.
   */
  override def initialize(configuration: Configuration, accountName: String): Unit = {
    logger.info(s"initialize Token called: " + TokenManager.getAccessToken.toString)
    logger.info(s"initialize expiryTime called: " + TokenManager.getExpiryTime.toString)
    accessToken = TokenManager.getAccessToken
    expiryTime = TokenManager.getExpiryTime
  }

  /**
   * Retrieves the current access token from the `TokenManager`.
   *
   * @return The current access token as a `String`.
   */
  override def getAccessToken(): String = {
    logger.info(s"getAccessToken called: " + this.accessToken.toString)
    this.accessToken
  }

  /**
   * Retrieves the expiry time of the current access token from the `TokenManager`.
   *
   * @return The expiry time of the access token as a `Date`.
   */
  override def getExpiryTime(): Date = {
    logger.info(s"getExpiryTime called: " + this.expiryTime.toString)
    this.expiryTime
  }

  /**
   * Checks if the current access token is expired.
   *
   * @return `true` if the token is expired or if the expiry time is not set,
   *         `false` otherwise.
   */
  private def isTokenExpired: Boolean = {
    expiryTime == null || new Date().after(expiryTime)
  }

}
