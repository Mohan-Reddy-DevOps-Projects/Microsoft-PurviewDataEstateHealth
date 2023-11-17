// -----------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
// -----------------------------------------------------------

namespace Microsoft.Azure.Purview.DataEstateHealth.Models;

using Microsoft.Data.DeltaLake.Types;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

// !!! TODO (TASK 2782067) Find scalable alternative to these classes.

/// <summary>
/// Thumbnail Wrapper.
/// </summary>
public class ThumbnailEventHubEntityModel
{
    /// <summary>
    /// Color.
    /// </summary>
    [JsonProperty("color")]
    public string Color { get; set; }
}

/// <summary>
/// The catalog contact model.
/// </summary>
public class ContactEventHubEntityModel
{
    /// <summary>
    /// Owner.
    /// </summary>
    [JsonProperty("owner")]
    public IEnumerable<ContactItemEventHubEntityModel> Owner { get; set; }

    /// <summary>
    /// Expert.
    /// </summary>
    [JsonProperty("expert")]
    public IEnumerable<ContactItemEventHubEntityModel> Expert { get; set; }

    /// <summary>
    /// Database Admin.
    /// </summary>
    [JsonProperty("databaseAdmin")]
    public IEnumerable<ContactItemEventHubEntityModel> DatabaseAdmin { get; set; }
}

/// <summary>
/// The catalog contact item model.
/// </summary>
public class ContactItemEventHubEntityModel
{
    /// <summary>
    /// Description of contact.
    /// </summary>
    [JsonProperty("description")]
    public string Description { get; set; }

    /// <summary>
    /// Id of contact.
    /// </summary>
    [JsonProperty("id")]
    public string Id { get; set; }
}

/// <summary>
/// Catalog system data.
/// </summary>
public class SystemDataEventHubEntityModel
{
    /// <summary>
    /// Last Modified At.
    /// </summary>
    [JsonProperty("lastModifiedAt")]
    public string LastModifiedAt { get; set; }

    /// <summary>
    /// Last Modified By.
    /// </summary>
    [JsonProperty("lastModifiedBy")]
    public string LastModifiedBy { get; set; }

    /// <summary>
    /// Created At.
    /// </summary>
    [JsonProperty("createdAt")]
    public string CreatedAt { get; set; }

    /// <summary>
    /// Created By.
    /// </summary>
    [JsonProperty("createdBy")]
    public string CreatedBy { get; set; }

    /// <summary>
    /// Expired At.
    /// </summary>
    [JsonProperty("expiredAt")]
    public string ExpiredAt { get; set; }

    /// <summary>
    /// Expired By.
    /// </summary>
    [JsonProperty("expiredBy")]
    public string ExpiredBy { get; set; }
}

/// <summary>
/// The catalog data product model.
/// </summary>
public class SchemaEventHubEntityModel
{
    /// <summary>
    /// name.
    /// </summary>
    [JsonProperty("name")]
    public string Name { get; set; }

    /// <summary>
    /// Description.
    /// </summary>
    [JsonProperty("description")]
    public string Description { get; set; }

    /// <summary>
    /// Type.
    /// </summary>
    [JsonProperty("type")]
    public string Type { get; set; }

    /// <summary>
    /// Classifications.
    /// </summary>
    [JsonProperty("classifications")]
    public IEnumerable<string> Classifications { get; set; }
}

/// <summary>
/// Data asset source.
/// </summary>
public class SourceEventHubEntityModel
{
    /// <summary>
    /// Asset Id.
    /// </summary>
    [JsonProperty("assetId")]
    public string AssetId { get; set; }

    /// <summary>
    /// Asset Type.
    /// </summary>
    [JsonProperty("assetType")]
    public string AssetType { get; set; }

    /// <summary>
    /// Last Refreshed At.
    /// </summary>
    [JsonProperty("lastRefreshedAt")]
    public string LastRefreshedAt { get; set; }

    /// <summary>
    /// Last Refreshed by
    /// </summary>
    [JsonProperty("lastRefreshedBy")]
    public string LastRefreshedBy { get; set; }

    /// <summary>
    /// type.
    /// </summary>
    [JsonProperty("type")]
    public string Type { get; set; }
}

/// <summary>
/// Additional Properties.
/// </summary>
public class DataProductAdditionalPropertiesEventHubEntityModel
{
    /// <summary>
    /// Asset Count.
    /// </summary>
    [JsonProperty("assetCount")]
    public string AssetCount { get; set; }
}

/// <summary>
/// The catalog external link model.
/// </summary>
public class DataProductExternalLinkEventHubEntityModel
{
    /// <summary>
    /// Name.
    /// </summary>
    [JsonProperty("name")]
    public string Name { get; set; }

    /// <summary>
    /// Data Asset Id.
    /// </summary>
    [JsonProperty("dataAssetId")]
    public string DataAssetId { get; set; }

    /// <summary>
    /// Url.
    /// </summary>
    [JsonProperty("url")]
    public string Url { get; set; }
}

/// <summary>
/// Term Attributes.
/// </summary>
public class TermAttributesEventHubEntityModel
{
    /// <summary>
    /// name of term.
    /// </summary>
    [JsonProperty("limit")]
    public string Limit { get; set; }

    /// <summary>
    /// sponsors.
    /// </summary>
    [JsonProperty("sponsors")]
    public List<string> Sponsors { get; set; }
}

/// <summary>
/// The catalog business domain model.
/// </summary>
public class BusinessDomainEventHubEntityModel : BaseEventHubEntityModel
{
    /// <summary>
    /// name of business domain.
    /// </summary>
    [JsonProperty("name")]
    public string Name { get; set; }

    /// <summary>
    /// Description of business domain.
    /// </summary>
    [JsonProperty("description")]
    public string Description { get; set; }

    /// <summary>
    /// Status of business domain.
    /// </summary>
    [JsonProperty("status")]
    public string Status { get; set; }

    /// <summary>
    /// Id of business domain.
    /// </summary>
    [JsonProperty("id")]
    public string Id { get; set; }

    /// <summary>
    /// Parent Id of business domain.
    /// </summary>
    [JsonProperty("parentId")]
    public string ParentId { get; set; }

    /// <summary>
    /// System data of business domain.
    /// </summary>
    [JsonProperty("systemData")]
    [JsonConverter(typeof(CustomTypeConverter<SystemDataEventHubEntityModel>))]
    public string SystemData { get; set; }

    /// <summary>
    /// Thumbnail business domain.
    /// </summary>
    [JsonProperty("thumbnail")]
    [JsonConverter(typeof(CustomTypeConverter<ThumbnailEventHubEntityModel>))]
    public string Thumbnail { get; set; }

    /// <summary>
    /// Related Collections business domain.
    /// </summary>
    [JsonProperty("relatedCollections")]
    [JsonConverter(typeof(CustomTypeConverter<IEnumerable<string>>))]
    public string RelatedCollections { get; set; }

    /// <inheritdoc/>
    public override PayloadKind GetPayloadKind() => PayloadKind.BusinessDomain;

    /// <inheritdoc/>
    public override StructType GetSchemaDefinition() => new(new StructField[]
        {
            new StructField("Name", DataTypes.String),
            new StructField("Description", DataTypes.String),
            new StructField("Status", DataTypes.String),
            new StructField("Id", DataTypes.String),
            new StructField("ParentId", DataTypes.String),
            new StructField("SystemData", DataTypes.String),
            new StructField("Thumbnail", DataTypes.String),
            new StructField("RelatedCollections", DataTypes.String),
        });
}

/// <summary>
/// The catalog data asset model.
/// </summary>
public class DataAssetEventHubEntityModel : BaseEventHubEntityModel
{
    /// <summary>
    /// name of data asset.
    /// </summary>
    [JsonProperty("name")]
    public string Name { get; set; }

    /// <summary>
    /// Description of data asset.
    /// </summary>
    [JsonProperty("description")]
    public string Description { get; set; }

    /// <summary>
    /// Type of data asset.
    /// </summary>
    [JsonProperty("type")]
    public string Type { get; set; }

    /// <summary>
    /// Domain of data asset.
    /// </summary>
    [JsonProperty("domain")]
    public string Domain { get; set; }

    /// <summary>
    /// Id of data asset.
    /// </summary>
    [JsonProperty("id")]
    public string Id { get; set; }

    /// <summary>
    /// Sensitivity Label.
    /// </summary>
    [JsonProperty("sensitivityLabel")]
    public string SensitivityLabel { get; set; }

    /// <summary>
    /// Contacts data asset.
    /// </summary>
    [JsonProperty("contacts")]
    [JsonConverter(typeof(CustomTypeConverter<ContactEventHubEntityModel>))]
    public string Contacts { get; set; }

    /// <summary>
    /// System data.
    /// </summary>
    [JsonProperty("systemData")]
    [JsonConverter(typeof(CustomTypeConverter<SystemDataEventHubEntityModel>))]
    public string SystemData { get; set; }

    /// <summary>
    /// Source of data asset.
    /// </summary>
    [JsonProperty("source")]
    [JsonConverter(typeof(CustomTypeConverter<SourceEventHubEntityModel>))]
    public string Source { get; set; }

    /// <summary>
    /// Classifications.
    /// </summary>
    [JsonProperty("classifications")]
    [JsonConverter(typeof(CustomTypeConverter<IEnumerable<string>>))]
    public string Classifications { get; set; }

    /// <summary>
    /// Lineage.
    /// </summary>
    [JsonProperty("lineage")]
    [JsonConverter(typeof(CustomTypeConverter<JObject>))]
    public string Lineage { get; set; }

    /// <summary>
    /// Type Properties.
    /// </summary>
    [JsonProperty("typeProperties")]
    [JsonConverter(typeof(CustomTypeConverter<JObject>))]
    public string TypeProperties { get; set; }

    /// <summary>
    /// Schema.
    /// </summary>
    [JsonProperty("schema")]
    [JsonConverter(typeof(CustomTypeConverter<IEnumerable<SchemaEventHubEntityModel>>))]
    public string Schema { get; set; }

    /// <inheritdoc/>
    public override PayloadKind GetPayloadKind() => PayloadKind.DataAsset;

    /// <inheritdoc/>
    public override StructType GetSchemaDefinition() => new(new StructField[]
       {
            new StructField("Name", DataTypes.String),
            new StructField("Description", DataTypes.String),
            new StructField("Type", DataTypes.String),
            new StructField("Domain", DataTypes.String),
            new StructField("Id", DataTypes.String),
            new StructField("Contacts", DataTypes.String),
            new StructField("SystemData", DataTypes.String),
            new StructField("Source", DataTypes.String),
            new StructField("SensitivityLabel", DataTypes.String),
            new StructField("Classifications", DataTypes.String),
            new StructField("Lineage", DataTypes.String),
            new StructField("TypeProperties", DataTypes.String),
            new StructField("Schema", DataTypes.String),
       });
}

/// <summary>
/// The catalog data product model.
/// </summary>
public class DataProductEventHubEntityModel : BaseEventHubEntityModel
{
    /// <summary>
    /// name of data product.
    /// </summary>
    [JsonProperty("name")]
    public string Name { get; set; }

    /// <summary>
    /// Description of data product.
    /// </summary>
    [JsonProperty("description")]
    public string Description { get; set; }

    /// <summary>
    /// Type of data product.
    /// </summary>
    [JsonProperty("type")]
    public string Type { get; set; }

    /// <summary>
    /// Business Use of data product.
    /// </summary>
    [JsonProperty("businessUse")]
    public string BusinessUse { get; set; }

    /// <summary>
    /// Status of data product.
    /// </summary>
    [JsonProperty("status")]
    public string Status { get; set; }

    /// <summary>
    /// Domain of data product.
    /// </summary>
    [JsonProperty("domain")]
    public string Domain { get; set; }

    /// <summary>
    /// Id of data product.
    /// </summary>
    [JsonProperty("id")]
    public string Id { get; set; }

    /// <summary>
    /// Sensitivity Label.
    /// </summary>
    [JsonProperty("sensitivityLabel")]
    public string SensitivityLabel { get; set; }

    /// <summary>
    /// Update Frequency.
    /// </summary>
    [JsonProperty("updateFrequency")]
    public string UpdateFrequency { get; set; }

    /// <summary>
    /// System data.
    /// </summary>
    [JsonProperty("systemData")]
    [JsonConverter(typeof(CustomTypeConverter<SystemDataEventHubEntityModel>))]
    public string SystemData { get; set; }

    /// <summary>
    /// Additional Properties data product.
    /// </summary>
    [JsonProperty("additionalProperties")]
    [JsonConverter(typeof(CustomTypeConverter<DataProductAdditionalPropertiesEventHubEntityModel>))]
    public string AdditionalProperties { get; set; }

    /// <summary>
    /// Contacts data product.
    /// </summary>
    [JsonProperty("contacts")]
    [JsonConverter(typeof(CustomTypeConverter<ContactEventHubEntityModel>))]
    public string Contacts { get; set; }

    /// <summary>
    /// Terms Of use.
    /// </summary>
    [JsonProperty("termsOfUse")]
    [JsonConverter(typeof(CustomTypeConverter<IEnumerable<DataProductExternalLinkEventHubEntityModel>>))]
    public string TermsOfUse { get; set; }

    /// <summary>
    /// Documentation.
    /// </summary>
    [JsonProperty("documentation")]
    [JsonConverter(typeof(CustomTypeConverter<IEnumerable<DataProductExternalLinkEventHubEntityModel>>))]
    public string Documentation { get; set; }

    /// <inheritdoc/>
    public override PayloadKind GetPayloadKind() => PayloadKind.DataProduct;

    /// <inheritdoc/>
    public override StructType GetSchemaDefinition() => new(new StructField[]
       {
            new StructField("Name", DataTypes.String),
            new StructField("Description", DataTypes.String),
            new StructField("Type", DataTypes.String),
            new StructField("BusinessUse", DataTypes.String),
            new StructField("Status", DataTypes.String),
            new StructField("Domain", DataTypes.String),
            new StructField("Id", DataTypes.String),
            new StructField("SensitivityLabel", DataTypes.String),
            new StructField("UpdateFrequency", DataTypes.String),
            new StructField("SystemData", DataTypes.String),
            new StructField("AdditionalProperties", DataTypes.String),
            new StructField("Contacts", DataTypes.String),
            new StructField("TermsOfUse", DataTypes.String),
            new StructField("Documentation", DataTypes.String),
       });
}

/// <summary>
/// Catalog Relationship model.
/// </summary>
public class RelationshipEventHubEntityModel : BaseEventHubEntityModel
{
    /// <summary>
    /// Type.
    /// </summary>
    [JsonProperty("type")]
    public string Type { get; set; }

    /// <summary>
    /// SourceType.
    /// </summary>
    [JsonProperty("sourceType")]
    public string SourceType { get; set; }

    /// <summary>
    /// SourceId.
    /// </summary>
    [JsonProperty("sourceId")]
    public string SourceId { get; set; }

    /// <summary>
    /// TargetType.
    /// </summary>
    [JsonProperty("targetType")]
    public string TargetType { get; set; }

    /// <summary>
    /// TargetId.
    /// </summary>
    [JsonProperty("targetId")]
    public string TargetId { get; set; }

    /// <summary>
    /// Description.
    /// </summary>
    [JsonProperty("description")]
    public string Description { get; set; }

    /// <summary>
    /// relationshipType.
    /// </summary>
    [JsonProperty("relationshipType")]
    public string RelationshipType { get; set; }

    /// <summary>
    /// entityId.
    /// </summary>
    [JsonProperty("entityId")]
    public string EntityId { get; set; }

    /// <inheritdoc/>
    public override PayloadKind GetPayloadKind() => PayloadKind.Relationship;

    /// <inheritdoc/>
    public override StructType GetSchemaDefinition() => new(new StructField[]
   {
            new StructField("Type", DataTypes.String),
            new StructField("SourceType", DataTypes.String),
            new StructField("SourceId", DataTypes.String),
            new StructField("TargetType", DataTypes.String),
            new StructField("TargetId", DataTypes.String),
            new StructField("Description", DataTypes.String),
            new StructField("RelationshipType", DataTypes.String),
            new StructField("Entityid", DataTypes.String),
   });
}

/// <summary>
/// Catalog term model.
/// </summary>
public class TermEventHubEntityModel : BaseEventHubEntityModel
{
    /// <summary>
    /// name of term.
    /// </summary>
    [JsonProperty("name")]
    public string Name { get; set; }

    /// <summary>
    /// Description of term.
    /// </summary>
    [JsonProperty("description")]
    public string Description { get; set; }

    /// <summary>
    /// Business Use of data product.
    /// </summary>
    [JsonProperty("businessUse")]
    public string BusinessUse { get; set; }

    /// <summary>
    /// Status of term.
    /// </summary>
    [JsonProperty("status")]
    public string Status { get; set; }

    /// <summary>
    /// Domain of term.
    /// </summary>
    [JsonProperty("domain")]
    public string Domain { get; set; }

    /// <summary>
    /// Id of term.
    /// </summary>
    [JsonProperty("id")]
    public string Id { get; set; }

    /// <summary>
    /// ParentId of term.
    /// </summary>
    [JsonProperty("parentId")]
    public string ParentId { get; set; }

    /// <summary>
    /// Is leaf.
    /// </summary>
    [JsonProperty("isLeaf")]
    public string IsLeaf { get; set; }

    /// <summary>
    /// System data.
    /// </summary>
    [JsonProperty("systemData")]
    [JsonConverter(typeof(CustomTypeConverter<SystemDataEventHubEntityModel>))]
    public string SystemData { get; set; }

    /// <summary>
    /// Contacts terms.
    /// </summary>
    [JsonProperty("contacts")]
    [JsonConverter(typeof(CustomTypeConverter<ContactEventHubEntityModel>))]
    public string Contacts { get; set; }

    /// <summary>
    /// Attributes.
    /// </summary>
    [JsonProperty("attributes")]
    [JsonConverter(typeof(CustomTypeConverter<TermAttributesEventHubEntityModel>))]
    public string Attributes { get; set; }

    /// <inheritdoc/>
    public override PayloadKind GetPayloadKind() => PayloadKind.Term;

    /// <inheritdoc/>
    public override StructType GetSchemaDefinition() => new(new StructField[]
    {
                new StructField("Name", DataTypes.String),
                new StructField("Description", DataTypes.String),
                new StructField("BusinessUse", DataTypes.String),
                new StructField("Status", DataTypes.String),
                new StructField("Domain", DataTypes.String),
                new StructField("Id", DataTypes.String),
                new StructField("ParentId", DataTypes.String),
                new StructField("IsLeaf", DataTypes.String),
                new StructField("SystemData", DataTypes.String),
                new StructField("Contacts", DataTypes.String),
                new StructField("Attributes", DataTypes.String),
    });
}
