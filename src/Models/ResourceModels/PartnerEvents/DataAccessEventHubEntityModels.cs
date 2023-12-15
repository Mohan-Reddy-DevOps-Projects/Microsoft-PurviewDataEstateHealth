// -----------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
// -----------------------------------------------------------

namespace Microsoft.Azure.Purview.DataEstateHealth.Models;

using System;
using Microsoft.Data.DeltaLake.Types;
using Microsoft.WindowsAzure.ResourceStack.Common.Extensions;
using Newtonsoft.Json;

// !!! TODO (TASK 2782067) Find scalable alternative to these classes.

/// <summary>
/// AppliedPolicySetEventHubEntityModel
/// </summary>
public class AppliedPolicySetEventHubEntityModel
{
    /// <summary>
    /// Policies
    /// </summary>
    [JsonProperty("policies")]
    public PoliciesEventHubEntityModel Policies { get; set; }

    /// <summary>
    /// Id
    /// </summary>
    [JsonProperty("id")]
    public string Id { get; set; }

    /// <summary>
    /// AccountId
    /// </summary>
    [JsonProperty("accountId")]
    public string AccountId { get; set; }

    /// <summary>
    /// TargetResource
    /// </summary>
    [JsonProperty("targetResource")]
    public TargetResourceEventHubEntityModel TargetResource { get; set; }

    /// <summary>
    /// Active
    /// </summary>
    [JsonProperty("active")]
    public bool Active { get; set; }

    /// <summary>
    /// Version
    /// </summary>
    [JsonProperty("version")]
    public string Version { get; set; }

    /// <summary>
    /// CreatedAt
    /// </summary>
    [JsonProperty("createdAt")]
    public DateTime CreatedAt { get; set; }

    /// <summary>
    /// CreatedBy
    /// </summary>
    [JsonProperty("createdBy")]
    public string CreatedBy { get; set; }

    /// <summary>
    /// ModifiedAt
    /// </summary>
    [JsonProperty("modifiedAt")]
    public DateTime ModifiedAt { get; set; }

    /// <summary>
    /// ModifiedBy
    /// </summary>
    [JsonProperty("modifiedBy")]
    public string ModifiedBy { get; set; }

    /// <summary>
    /// ProvisioningState
    /// </summary>
    [JsonProperty("provisioningState")]
    public string ProvisioningState { get; set; }

    /// <summary>
    /// PolicySetKind
    /// </summary>
    [JsonProperty("policySetKind")]
    public string PolicySetKind { get; set; }
}

/// <summary>
/// ApproverDecisionEventHubEntityModel
/// </summary>
public class ApproverDecisionEventHubEntityModel
{
    /// <summary>
    /// Approver
    /// </summary>
    [JsonProperty("approver")]
    public IdentityEventHubEntityModel Approver { get; set; }

    /// <summary>
    /// ApproverDecisionType
    /// </summary>
    [JsonProperty("approverDecisionType")]
    public string ApproverDecisionType { get; set; }

    /// <summary>
    /// Decision
    /// </summary>
    [JsonProperty("decision")]
    public string Decision { get; set; }
}

/// <summary>
/// PermittedUseCaseEventHubEntityModel
/// </summary>
public class PermittedUseCaseEventHubEntityModel
{
    /// <summary>
    /// Title
    /// </summary>
    [JsonProperty("Title")]
    public string Title { get; set; }

    /// <summary>
    /// Description
    /// </summary>
    [JsonProperty("Description")]
    public string Description { get; set; }
}

/// <summary>
/// PoliciesEventHubEntityModel
/// </summary>
public class PoliciesEventHubEntityModel
{
    /// <summary>
    /// Approvals
    /// </summary>
    [JsonProperty("approvals")]
    public List<IdentityEventHubEntityModel> Approvals { get; set; }

    /// <summary>
    /// PermittedUseCases
    /// </summary>
    [JsonProperty("permittedUseCases")]
    public List<PermittedUseCaseEventHubEntityModel> PermittedUseCases { get; set; }

    /// <summary>
    /// TermsOfUseRequired
    /// </summary>
    [JsonProperty("termsOfUseRequired")]
    public bool TermsOfUseRequired { get; set; }

    /// <summary>
    /// PartnerExposurePermitted
    /// </summary>
    [JsonProperty("partnerExposurePermitted")]
    public bool PartnerExposurePermitted { get; set; }

    /// <summary>
    /// CustomerExposurePermitted
    /// </summary>
    [JsonProperty("customerExposurePermitted")]
    public bool CustomerExposurePermitted { get; set; }

    /// <summary>
    /// ManagerApprovalRequired
    /// </summary>
    [JsonProperty("managerApprovalRequired")]
    public bool ManagerApprovalRequired { get; set; }

    /// <summary>
    /// PrivacyComplianceApprovalRequired
    /// </summary>
    [JsonProperty("privacyComplianceApprovalRequired")]
    public bool PrivacyComplianceApprovalRequired { get; set; }

    /// <summary>
    /// DataCopyPermitted
    /// </summary>
    [JsonProperty("dataCopyPermitted")]
    public bool DataCopyPermitted { get; set; }
}

/// <summary>
/// PolicySetValuesEventHubEntityModel
/// </summary>
public class PolicySetValuesEventHubEntityModel
{
    /// <summary>
    /// UseCase
    /// </summary>
    [JsonProperty("useCase")]
    public string UseCase { get; set; }

    /// <summary>
    /// BusinessJustification
    /// </summary>
    [JsonProperty("businessJustification")]
    public string BusinessJustification { get; set; }

    /// <summary>
    /// SubscriberManagerIdentities
    /// </summary>
    [JsonProperty("subscriberManagerIdentities")]
    public List<IdentityEventHubEntityModel> SubscriberManagerIdentities { get; set; }

    /// <summary>
    /// ApproverDecisions
    /// </summary>
    [JsonProperty("approverDecisions")]
    public List<ApproverDecisionEventHubEntityModel> ApproverDecisions { get; set; }

    /// <summary>
    /// PartnerSharingRequested
    /// </summary>
    [JsonProperty("partnerSharingRequested")]
    public bool PartnerSharingRequested { get; set; }

    /// <summary>
    /// CustomerSharingRequested
    /// </summary>
    [JsonProperty("customerSharingRequested")]
    public bool CustomerSharingRequested { get; set; }

    /// <summary>
    /// TermsOfUseAccepted
    /// </summary>
    [JsonProperty("termsOfUseAccepted")]
    public bool TermsOfUseAccepted { get; set; }
}

/// <summary>
/// IdentityEventHubEntityModel
/// </summary>
public class IdentityEventHubEntityModel
{
    /// <summary>
    /// IdentityType
    /// </summary>
    [JsonProperty("identityType")]
    public string IdentityType { get; set; }

    /// <summary>
    /// ObjectId
    /// </summary>
    [JsonProperty("objectId")]
    public string ObjectId { get; set; }

    /// <summary>
    /// TenantId
    /// </summary>
    [JsonProperty("tenantId")]
    public string TenantId { get; set; }

    /// <summary>
    /// DisplayName
    /// </summary>
    [JsonProperty("displayName")]
    public string DisplayName { get; set; }

    /// <summary>
    /// Email
    /// </summary>
    [JsonProperty("email")]
    public string Email { get; set; }
}

/// <summary>
/// TargetResourceEventHubEntityModel
/// </summary>
public class TargetResourceEventHubEntityModel
{
    /// <summary>
    /// TargetId
    /// </summary>
    [JsonProperty("TargetId")]
    public string TargetId { get; set; }

    /// <summary>
    /// TargetType
    /// </summary>
    [JsonProperty("TargetType")]
    public string TargetType { get; set; }
}

/// <summary>
/// PolicySetEventHubEntityModel
/// </summary>
public class PolicySetEventHubEntityModel : BaseEventHubEntityModel
{
    /// <summary>
    /// Id
    /// </summary>
    [JsonProperty("id")]
    public string Id { get; set; }

    /// <summary>
    /// Active
    /// </summary>
    [JsonProperty("active")]
    public string Active { get; set; }

    /// <summary>
    /// Version
    /// </summary>
    [JsonProperty("version")]
    public string Version { get; set; }

    /// <summary>
    /// CreatedAt
    /// </summary>
    [JsonProperty("createdAt")]
    public DateTime CreatedAt { get; set; }

    /// <summary>
    /// CreatedBy
    /// </summary>
    [JsonProperty("createdBy")]
    public string CreatedBy { get; set; }

    /// <summary>
    /// ModifiedAt
    /// </summary>
    [JsonProperty("modifiedAt")]
    public DateTime ModifiedAt { get; set; }

    /// <summary>
    /// ModifiedBy
    /// </summary>
    [JsonProperty("modifiedBy")]
    public string ModifiedBy { get; set; }

    /// <summary>
    /// ProvisioningState
    /// </summary>
    [JsonProperty("provisioningState")]
    public string ProvisioningState { get; set; }

    /// <summary>
    /// PolicySetKind
    /// </summary>
    [JsonProperty("policySetKind")]
    public string PolicySetKind { get; set; }

    /// <summary>
    /// Policies
    /// </summary>
    [JsonProperty("policies")]
    [JsonConverter(typeof(CustomTypeConverter<PoliciesEventHubEntityModel>))]
    public string Policies { get; set; }

    /// <summary>
    /// TargetResource
    /// </summary>
    [JsonProperty("targetResource")]
    [JsonConverter(typeof(CustomTypeConverter<TargetResourceEventHubEntityModel>))]
    public string TargetResource { get; set; }

    /// <inheritdoc/>
    public override PayloadKind GetPayloadKind() => PayloadKind.PolicySet;

    /// <inheritdoc/>
    public override StructType GetSchemaDefinition() => new(new StructField[]
       {
            new StructField("Id", DataTypes.String),
            new StructField("Active", DataTypes.String),
            new StructField("Version", DataTypes.String),
            new StructField("CreatedAt", DataTypes.Timestamp),
            new StructField("CreatedBy", DataTypes.String),
            new StructField("ModifiedAt", DataTypes.Timestamp),
            new StructField("ModifiedBy", DataTypes.String),
            new StructField("ProvisioningState", DataTypes.String),
            new StructField("PolicySetKind", DataTypes.String),
            new StructField("Policies", DataTypes.String),
            new StructField("TargetResource", DataTypes.String),
       }.ConcatArray(GetCommonSchemaFields()));
}

/// <summary>
/// DataSubscriptionEventHubEntityModel
/// </summary>
public class DataSubscriptionEventHubEntityModel : BaseEventHubEntityModel
{
    /// <summary>
    /// Id
    /// </summary>
    [JsonProperty("id")]
    public string Id { get; set; }

    /// <summary>
    /// WriteAccess
    /// </summary>
    [JsonProperty("writeAccess")]
    public string WriteAccess { get; set; }

    /// <summary>
    /// SubscriptionStatus
    /// </summary>
    [JsonProperty("subscriptionStatus")]
    public string SubscriptionStatus { get; set; }

    /// <summary>
    /// DomainId
    /// </summary>
    [JsonProperty("domainId")]
    public string DomainId { get; set; }

    /// <summary>
    /// DataProductId
    /// </summary>
    [JsonProperty("dataProductId")]
    public string DataProductId { get; set; }

    /// <summary>
    /// ProvisioningState
    /// </summary>
    [JsonProperty("provisioningState")]
    public string ProvisioningState { get; set; }

    /// <summary>
    /// CreatedAt
    /// </summary>
    [JsonProperty("createdAt")]
    public DateTime CreatedAt { get; set; }

    /// <summary>
    /// CreatedBy
    /// </summary>
    [JsonProperty("createdBy")]
    public string CreatedBy { get; set; }

    /// <summary>
    /// ModifiedAt
    /// </summary>
    [JsonProperty("modifiedAt")]
    public DateTime ModifiedAt { get; set; }

    /// <summary>
    /// ModifiedBy
    /// </summary>
    [JsonProperty("modifiedBy")]
    public string ModifiedBy { get; set; }

    /// <summary>
    /// Version
    /// </summary>
    [JsonProperty("version")]
    public string Version { get; set; }

    /// <summary>
    /// SubscriberIdentity
    /// </summary>
    [JsonProperty("subscriberIdentity")]
    [JsonConverter(typeof(CustomTypeConverter<IdentityEventHubEntityModel>))]
    public string SubscriberIdentity { get; set; }

    /// <summary>
    /// RequestorIdentity
    /// </summary>
    [JsonProperty("requestorIdentity")]
    [JsonConverter(typeof(CustomTypeConverter<IdentityEventHubEntityModel>))]
    public string RequestorIdentity { get; set; }

    /// <summary>
    /// PolicySetValues
    /// </summary>
    [JsonProperty("policySetValues")]
    [JsonConverter(typeof(CustomTypeConverter<PolicySetValuesEventHubEntityModel>))]
    public string PolicySetValues { get; set; }

    /// <summary>
    /// AppliedPolicySet
    /// </summary>
    [JsonProperty("appliedPolicySet")]
    [JsonConverter(typeof(CustomTypeConverter<AppliedPolicySetEventHubEntityModel>))]
    public string AppliedPolicySet { get; set; }

    /// <inheritdoc/>
    public override PayloadKind GetPayloadKind() => PayloadKind.DataSubscription;

    /// <inheritdoc/>
    public override StructType GetSchemaDefinition() => new(new StructField[]
       {
            new StructField("Id", DataTypes.String),
            new StructField("AccountId", DataTypes.String),
            new StructField("WriteAccess", DataTypes.String),
            new StructField("SubscriptionStatus", DataTypes.String),
            new StructField("DomainId", DataTypes.String),
            new StructField("DataProductId", DataTypes.String),
            new StructField("ProvisioningState", DataTypes.String),
            new StructField("CreatedAt", DataTypes.Timestamp),
            new StructField("CreatedBy", DataTypes.String),
            new StructField("ModifiedAt", DataTypes.Timestamp),
            new StructField("ModifiedBy", DataTypes.String),
            new StructField("Version", DataTypes.String),
            new StructField("SubscriberIdentity", DataTypes.String),
            new StructField("RequestorIdentity", DataTypes.String),
            new StructField("PolicySetValues", DataTypes.String),
            new StructField("AppliedPolicySet", DataTypes.String),
       }.ConcatArray(GetCommonSchemaFields()));
}
