package com.microsoft.azurepurview.dataestatehealth.domainmodel.subscription

import org.apache.log4j.{Level, Logger}
import org.apache.spark.sql.SparkSession
import org.apache.spark.sql.types._

import java.sql.Timestamp

object DataSubscriberRequestTest {
  def main(args: Array[String]): Unit = {

    val logger = Logger.getLogger(getClass.getName)
    logger.setLevel(Level.INFO)

    println("=== DataSubscriberRequest Logic Test (Simplified) ===")

    val spark = SparkSession.builder
      .appName("DataSubscriberRequestTest")
      .master("local[*]")
      .config("spark.sql.adaptive.enabled", "false")
      .getOrCreate()

    import spark.implicits._

    try {
      val currentTime = new Timestamp(System.currentTimeMillis())
      val sameModifiedTime = new Timestamp(1699200000000L)

      // Create simplified test data that directly matches the expected input format
      val testDataJson =
        s"""
      [
        {
          "accountId": "account1",
          "operationType": "Create",
          "_ts": 1699200000,
          "payload": {
            "After": {
              "id": "req-001",
              "dataProductId": "dp-001",
              "domainId": "domain-001",
              "appliedPolicySet": {"id": "policy-001"},
              "subscriberIdentity": {
                "identityType": "User",
                "objectId": "user-001",
                "tenantId": "tenant-001",
                "email": "user1@test.com"
              },
              "requestorIdentity": {
                "identityType": "User",
                "objectId": "user-002",
                "tenantId": "tenant-001",
                "email": "user2@test.com"
              },
              "provisioningState": "Active",
              "subscriptionStatus": "Pending",
              "writeAccess": true,
              "accessDecisionDate": "${sameModifiedTime.getTime}",
              "version": "1.0",
              "createdBy": "creator-001",
              "createdAt": "${currentTime.getTime}",
              "modifiedBy": "modifier-001",
              "modifiedAt": "${sameModifiedTime.getTime}",
              "policySetValues": {
                "approverDecisions": [{"decision": "Pending"}]
              }
            }
          }
        },
        {
          "accountId": "account1",
          "operationType": "Update",
          "_ts": 1699200001,
          "payload": {
            "After": {
              "id": "req-001",
              "dataProductId": "dp-001",
              "domainId": "domain-001",
              "appliedPolicySet": {"id": "policy-001"},
              "subscriberIdentity": {
                "identityType": "User",
                "objectId": "user-001",
                "tenantId": "tenant-001",
                "email": "user1@test.com"
              },
              "requestorIdentity": {
                "identityType": "User",
                "objectId": "user-002",
                "tenantId": "tenant-001",
                "email": "user2@test.com"
              },
              "provisioningState": "Active",
              "subscriptionStatus": "Completed",
              "writeAccess": true,
              "accessDecisionDate": "${sameModifiedTime.getTime}",
              "version": "1.0",
              "createdBy": "creator-001",
              "createdAt": "${currentTime.getTime}",
              "modifiedBy": "modifier-001",
              "modifiedAt": "${sameModifiedTime.getTime}",
              "policySetValues": {
                "approverDecisions": [{"decision": "Approved"}]
              }
            }
          }
        },
        {
          "accountId": "account1",
          "operationType": "Create",
          "_ts": 1699200002,
          "payload": {
            "After": {
              "id": "req-002",
              "dataProductId": "dp-002",
              "domainId": "domain-002",
              "appliedPolicySet": {"id": "policy-002"},
              "subscriberIdentity": {
                "identityType": "User",
                "objectId": "user-003",
                "tenantId": "tenant-001",
                "email": "user3@test.com"
              },
              "requestorIdentity": {
                "identityType": "User",
                "objectId": "user-004",
                "tenantId": "tenant-001",
                "email": "user4@test.com"
              },
              "provisioningState": "Active",
              "subscriptionStatus": "Pending",
              "writeAccess": false,
              "accessDecisionDate": "${sameModifiedTime.getTime}",
              "version": "1.0",
              "createdBy": "creator-002",
              "createdAt": "${currentTime.getTime}",
              "modifiedBy": "modifier-002",
              "modifiedAt": "${sameModifiedTime.getTime}",
              "policySetValues": {
                "approverDecisions": [{"decision": "Approved"}]
              }
            }
          }
        }
      ]"""

      val testDF = spark.read.json(Seq(testDataJson).toDS())

      println("=== INPUT TEST DATA ===")
      testDF.show(truncate = false)

      // Define the expected output schema
      val expectedSchema = StructType(Array(
        StructField("SubscriberRequestId", StringType, true),
        StructField("DataProductId", StringType, true),
        StructField("BusinessDomainId", StringType, true),
        StructField("AccessPolicySetId", StringType, true),
        StructField("SubscriberIdentityTypeDisplayName", StringType, true),
        StructField("RequestorIdentityTypeDisplayName", StringType, true),
        StructField("SubscriberRequestStatus", StringType, true),
        StructField("RequestStatusDisplayName", StringType, true),
        StructField("SubscribedByUserId", StringType, true),
        StructField("SubscribedByUserTenantId", StringType, true),
        StructField("SubscribedByUserEmail", StringType, true),
        StructField("RequestedByUserId", StringType, true),
        StructField("RequestedByUserTenantId", StringType, true),
        StructField("RequestedByUserEmail", StringType, true),
        StructField("RequestWriteAccess", BooleanType, true),
        StructField("RequestAccessDecisionDateTime", TimestampType, true),
        StructField("Version", StringType, true),
        StructField("AccountId", StringType, true),
        StructField("CreatedDatetime", TimestampType, true),
        StructField("CreatedByUserId", StringType, true),
        StructField("ModifiedDateTime", TimestampType, true),
        StructField("ModifiedByUserId", StringType, true),
        StructField("EventProcessingTime", LongType, true),
        StructField("OperationType", StringType, true)
      ))

      // Create processor and run test
      val processor = new DataSubscriberRequest(spark, logger)
      val result = processor.processDataSubscriberRequest(testDF, expectedSchema)

      println("\n=== TEST RESULTS ===")
      result.show(truncate = false)

      println("\n=== ANALYSIS ===")
      println("Final count: " + result.count())

      println("\nBy SubscriberRequestId:")
      result.groupBy("SubscriberRequestId").count().orderBy("SubscriberRequestId").show()

      println("\nBy RequestStatusDisplayName:")
      result.groupBy("RequestStatusDisplayName").count().show()

      println("\n=== EXPECTED vs ACTUAL ===")
      val finalResults = result.collect()
      finalResults.foreach { row =>
        val id = row.getAs[String]("SubscriberRequestId")
        val status = row.getAs[String]("RequestStatusDisplayName")
        val operation = row.getAs[String]("OperationType")

        val expected = id match {
          case "req-001" => "Completed" // Update operation should win
          case "req-002" => "Approved" // Pending + Approved should become Approved
          case _ => "Unknown"
        }

        val correct = if (status == expected) "✓ CORRECT" else "✗ INCORRECT"
        println(s"$id: $status (expected: $expected) $correct")
      }

    } catch {
      case e: Exception =>
        println(s"Test failed: ${e.getMessage}")
        e.printStackTrace()
        logger.error(s"Test failed: ${e.getMessage}")
    } finally {
      spark.stop()
    }
  }
}
