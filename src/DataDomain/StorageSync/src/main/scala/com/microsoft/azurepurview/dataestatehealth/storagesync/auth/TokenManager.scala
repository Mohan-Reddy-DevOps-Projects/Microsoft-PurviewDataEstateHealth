package com.microsoft.azurepurview.dataestatehealth.storagesync.auth

import java.util.Date

/**
 * TokenManager is a singleton object responsible for managing the access token
 * and its expiry time. It provides methods to initialize, retrieve, and check
 * the status of the access token.
 */
object TokenManager {

  /** The current access token. */
  private var accessToken: String = _

  /** The expiry time of the current access token. */
  private var expiryTime: Date = _

  /**
   * Initializes the TokenManager with the provided access token and expiry time.
   * This method sets the current access token and its expiry time.
   *
   * @param token The access token to be set.
   * @param expiry The expiry time of the access token.
   */
  def initialize(token: String, expiry: Date): Unit = {
    accessToken = token
    expiryTime = expiry
  }

  /**
   * Retrieves the current access token.
   *
   * @return The current access token as a `String`.
   */
  def getAccessToken: String = accessToken

  /**
   * Retrieves the expiry time of the current access token.
   *
   * @return The expiry time of the access token as a `Date`.
   */
  def getExpiryTime: Date = expiryTime

  /**
   * Checks if the current access token is expired.
   *
   * @return `true` if the token is expired or if the expiry time is not set,
   *         `false` otherwise.
   */
  def isTokenExpired: Boolean = {
    expiryTime == null || new Date().after(expiryTime)
  }
}
